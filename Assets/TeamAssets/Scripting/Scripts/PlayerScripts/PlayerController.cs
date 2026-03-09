using UnityEngine;
using System.Collections;
using Group26.Player.Inputs;

namespace Group26.Player.Movement
{
	public class PlayerController : MonoBehaviour
	{
		[Header("References")]
		private InputManager inputManager;

		[Header("Movement")]
		[SerializeField] float walkSpeed;
		[SerializeField] float sprintSpeed;
		[SerializeField] float slideSpeed;
		[SerializeField] float wallRunSpeed;
		[SerializeField] float groundDrag;
		[SerializeField] float dashSpeed;
		[SerializeField] float swingSpeed;
		[SerializeField] float dashSpeedChangeFactor;

		public float maxYSpeed;
		public float moveSpeed;       //CHANGE: made public
        float desiredMoveSpeed;
		float lastDesiredMoveSpeed;

		[SerializeField] float speedIncreaseMultiplier = 1f;
		[SerializeField] float slopeIncreaseMultiplier = 1f;

		[Header("Jumping")]
		[SerializeField] float jumpForce;
		[SerializeField] float jumpCooldown = 0.1f;
		[SerializeField] float airMultiplier = 0.4f;

		[Header("Jump Buffering")]
		[SerializeField] float jumpBufferTime = 0.15f;

		bool exitingSlope;

		[Header("Crouching")]
		[SerializeField] float crouchSpeed;
		float startYScale;
		float crouchYScale;

		[Header("Ground Check")]
		[SerializeField] float m_fGroundDistance = 0.15f;
		[SerializeField] Transform m_tGroundCheck;
		[SerializeField] LayerMask m_lGround;

		[Header("Slope Handling")]
		[SerializeField] float MaxSlopeAngle = 45f;
		RaycastHit slopeHit;
		
		public MovementState state;
		public enum MovementState 
		{ 
			freeze,
			walking, 
			sprinting, 
			crouching, 
			sliding, 
			air,
			swinging,
			wallRunning, 
			dashing
		}

		public bool m_bActiveGrapple;
		public bool m_bActiveSwing;
		public bool m_bFreeze;
		public bool m_bSliding;
		public bool m_bDashing;
		public bool m_bIsGrounded;
		public bool m_bIsWallRunning;

		[SerializeField] Transform orientation;

		float horizontalInput;
		float verticalInput;

		Vector3 moveDir;
		Rigidbody rb;

		Collider m_cPlayerCollider;

		// Jump buffer + cooldown state
		float jumpBufferTimer;
		bool readyToJump = true;

		Sliding slidingComp;

		SlopeMomentum m_momentumScript;

		public bool IsGrounded => m_bIsGrounded;
		public Vector3 SlopeNormal => slopeHit.normal;

		private void Awake()
		{
			inputManager = GetComponent<InputManager>();

			rb = GetComponent<Rigidbody>();
			rb.freezeRotation = true;

			startYScale = transform.localScale.y;
			crouchYScale = startYScale / 2f;

			m_cPlayerCollider = GetComponentInChildren<Collider>();
			slidingComp = GetComponent<Sliding>();
			m_momentumScript = GetComponent<SlopeMomentum>();

			//Vector2 moveInput = inputManager.MoveInput;
			//GetInput(moveInput);
		}

			private void OnEnable()
		{
			inputManager.OnJumpPressed += Jump;
		}

		private void OnDisable()
		{
			inputManager.OnJumpPressed -= Jump;
		}

		private void FixedUpdate()
		{
			// Ground check
			m_bIsGrounded = Physics.CheckSphere(m_tGroundCheck.position, m_fGroundDistance, m_lGround);

			GetInput(inputManager.MoveInput);

			// Drag only when grounded AND not sliding (sliding should keep momentum)

			if (m_bIsGrounded && !m_bActiveGrapple)
			{
				if (state == MovementState.walking || state == MovementState.sprinting || state == MovementState.crouching)
					rb.linearDamping = groundDrag;
				else
					rb.linearDamping = 0;
			}
			else
				rb.linearDamping = 0;


			// Cache slope check once per FixedUpdate (updates slopeHit)
			bool onSlope = OnSlope();

			// Only disable gravity for your custom "stick-to-slope" movement WHEN NOT sliding
			rb.useGravity = !(onSlope && !exitingSlope && !m_bSliding);

			// State/speed first (so movement uses correct moveSpeed this frame)
			StateHandler(onSlope);

			// Jump buffer countdown
			if (jumpBufferTimer > 0f)
				jumpBufferTimer -= Time.fixedDeltaTime;

			TryConsumeJumpBuffer();

            moveSpeed += m_momentumScript.m_momentum;

            // Sliding movement is handled by Sliding.cs
            if (!m_bSliding)
			{
				MovePlayer(onSlope);
				SpeedControl(onSlope);
			}
        }

        private MovementState lastState;
		private bool keepMomentum;

		void StateHandler(bool onSlope)
		{
			if (m_bFreeze)
			{
				state = MovementState.freeze;
				moveSpeed = 0;
				rb.linearVelocity = Vector3.zero;
			}

			else if (m_bDashing)
			{
				state = MovementState.dashing;
				desiredMoveSpeed = dashSpeed;
				speedChangeFactor = dashSpeedChangeFactor;
			}

			else if (m_bActiveSwing)
			{
				state = MovementState.swinging;
				moveSpeed = swingSpeed;
			}

			else if(m_bIsWallRunning)
			{
				state = MovementState.wallRunning;
				desiredMoveSpeed = wallRunSpeed;
			}

			// Priority order matters: sliding > crouch > sprint > walk > air
			else if (m_bSliding)
			{
				state = MovementState.sliding;
				desiredMoveSpeed = slideSpeed;
			}
			else if (m_bIsGrounded && inputManager.isCrouching)
			{
				state = MovementState.crouching;
				desiredMoveSpeed = crouchSpeed;
			}
			else if (m_bIsGrounded && inputManager.isSprinting)
			{
				state = MovementState.sprinting;
				desiredMoveSpeed = sprintSpeed;
			}
			else if (m_bIsGrounded)
			{
				state = MovementState.walking;
				desiredMoveSpeed = walkSpeed;
			}
			else
			{
				state = MovementState.air;

				// This code breaks momentum after exiting a wallrun, but keeping for now just in case
				// if (desiredMoveSpeed < sprintSpeed)
				// 	desiredMoveSpeed = walkSpeed;
				// else
				// 	desiredMoveSpeed = sprintSpeed;
			}

			if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4f && moveSpeed != 0f)
			{
				StopAllCoroutines();
				StartCoroutine(SmoothlyLerpMoveSpeed());
			}
			else
			{
				StopAllCoroutines();
				moveSpeed = desiredMoveSpeed;
			}

			bool desiredMoveSpeedHasChanged = desiredMoveSpeed != lastDesiredMoveSpeed;
			if (lastState == MovementState.dashing) keepMomentum = true;

			if (desiredMoveSpeedHasChanged)
			{
				if (keepMomentum)
				{
					StopAllCoroutines();
					StartCoroutine(SmoothlyLerpMoveSpeed());
				}
				else
				{
					StopAllCoroutines();
					moveSpeed = desiredMoveSpeed;
				}
			}

			lastDesiredMoveSpeed = desiredMoveSpeed;
			lastState = state;
		}

		private float speedChangeFactor;
		private IEnumerator SmoothlyLerpMoveSpeed()
		{
			float time = 0f;
            float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
			float startValue = moveSpeed;

			float boostFactor = speedChangeFactor;

            while (time < difference)
			{
                moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

				time += Time.deltaTime;

				if (OnSlope())
				{
					float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
					float slopeAngleIncrease = 1f + (slopeAngle / 90f);

					time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
				}
				else
				{
					time += Time.deltaTime * speedIncreaseMultiplier;
				}

				yield return null;
			}

            moveSpeed = desiredMoveSpeed;
            speedChangeFactor = 1f;
			keepMomentum = false;
		}

		public Vector3 CalculateJumpVelocity(Vector3 StartPoint, Vector3 EndPoint, float tracjectoryHeight)
		{
			float gravity = Physics.gravity.y;
			float displacementY = EndPoint.y - StartPoint.y;
			Vector3 displacementXZ = new Vector3(EndPoint.x - StartPoint.x, 0f, EndPoint.z - StartPoint.z);

			Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * tracjectoryHeight);
			Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * tracjectoryHeight / gravity)
				+ Mathf.Sqrt(2 * (displacementY - tracjectoryHeight) / gravity));

			return velocityXZ + velocityY;
		}

		private void GetInput(Vector2 input)
		{
			horizontalInput = input.x;
			verticalInput = input.y;
		}

		private void MovePlayer(bool onSlope)
		{
			if (m_bActiveGrapple) return;
			if (m_bActiveSwing) return;
			if (m_bDashing) return;

			moveDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

			if (onSlope && !exitingSlope)
			{
				rb.AddForce(GetSlopeMoveDirection(moveDir) * moveSpeed * 20f, ForceMode.Force);

				// small stick force so you don't "float" off slopes
				if (rb.linearVelocity.y > 0f)
					rb.AddForce(Vector3.down * 40f, ForceMode.Force);

				return; // IMPORTANT: don't also apply normal grounded force
			}

			if (m_bIsGrounded)
				rb.AddForce(moveDir * moveSpeed * 10f, ForceMode.Force);
			else
				rb.AddForce(moveDir * moveSpeed * 10f * airMultiplier, ForceMode.Force);

			if(!m_bIsWallRunning) rb.useGravity = !OnSlope();
        }

        private void SpeedControl(bool onSlope)
		{
			if (m_bActiveGrapple) return;

			// Don't clamp during sliding; Sliding.cs handles momentum/friction
			if (m_bSliding) return;

			if (onSlope && !exitingSlope)
			{
				if (rb.linearVelocity.magnitude > moveSpeed)
					rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
			}
			else
			{
				Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

				if (flatVel.magnitude > moveSpeed)
				{
					Vector3 limitedVel = flatVel.normalized * moveSpeed;
					rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
				}
			}

			// Limit Y velocity

			if (maxYSpeed != 0 && rb.linearVelocity.y > maxYSpeed)
				rb.linearVelocity = new Vector3(rb.linearVelocity.x, maxYSpeed, rb.linearVelocity.z);
		}

		public void Sprint(bool state) => inputManager.isSprinting = state;

		public void Crouch(bool state)
		{
			inputManager.isCrouching = state;

			if (state)
			{
				transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
				rb.AddForce(Vector3.down * 2f, ForceMode.Impulse);
			}
			else
			{
				transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
			}
		}

		public void Jump()
		{
			// buffer the press
			jumpBufferTimer = jumpBufferTime;
			TryConsumeJumpBuffer();
		}

		private void TryConsumeJumpBuffer()
		{
			if (!readyToJump) return;
			if (jumpBufferTimer <= 0f) return;
			if (!m_bIsGrounded) return;

			ExecuteJump();

			jumpBufferTimer = 0f;
			readyToJump = false;
			Invoke(nameof(ResetJump), jumpCooldown);
		}

		private void ExecuteJump()
		{
			// Jumping cancels slide cleanly
			if (m_bSliding && slidingComp != null)
				slidingComp.ForceEndSlide();

			exitingSlope = true;

			rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
			rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
		}

		private void ResetJump()
		{
			readyToJump = true;
			exitingSlope = false;
		}

		private bool enableMovementOnNextTouch;

		public void JumpToPosition(Vector3 targetPosition, float trajectoryHeight)
		{
			m_bActiveGrapple = true;

			VelocityToSet = CalculateJumpVelocity(transform.position, targetPosition, trajectoryHeight);
			Invoke(nameof(SetVelocity), 0.1f);
		}

		private Vector3 VelocityToSet;
		private void SetVelocity()
		{
			enableMovementOnNextTouch = true;
			rb.linearVelocity = VelocityToSet;
		}

		public void ResetRestrictions()
		{
			m_bActiveGrapple = false;
		}

		private void OnCollisionEnter(Collision collision)
		{
			if (enableMovementOnNextTouch)
			{
				enableMovementOnNextTouch = false;
				ResetRestrictions();

				GetComponent<GrappleGun>().ForceStopGrapple();
			}
		}

		public bool OnSlope()
		{
			if (m_cPlayerCollider == null) return false;

			float halfHeight = m_cPlayerCollider.bounds.extents.y; // updates if collider/scale changes

			if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, halfHeight + 0.35f, m_lGround))
			{
				float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
				return angle < MaxSlopeAngle && angle > 0f;
			}
			return false;
		}

		public Vector3 GetSlopeMoveDirection(Vector3 direction)
		{
			return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
		}

		public Vector3 GetDirection() { return moveDir; }
	}
}