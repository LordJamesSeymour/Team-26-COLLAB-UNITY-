using UnityEngine;
using System.Collections;
using Group26.Player.Inputs;
using Unity.Mathematics;
using UnityEngine.Splines;

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
		public float moveSpeed;
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

		[Header("Rail System")]
		[SerializeField] float railJumpUpForce = 6f;
		[SerializeField] float railExitForwardBoost = 2f;

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
			dashing,
			rail
		}

		public bool m_bActiveGrapple;
		public bool m_bActiveSwing;
		public bool m_bFreeze;
		public bool m_bSliding;
		public bool m_bDashing;
		public bool m_bIsGrounded;
		public bool m_bIsWallRunning;
		public bool m_bOnRail;

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

		// Rail runtime state
		RailSpline currentRail;
		float currentRailT;
		float currentRailSpeed;

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
			if (m_momentumScript == null)
				Debug.LogWarning("No SlopeMomentum script found on player.");
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

			// Rail overrides all other movement
			if (m_bOnRail)
			{
				UpdateRailMovement(Time.fixedDeltaTime);
				return;
			}

			// Drag only when grounded AND not sliding
			if (m_bIsGrounded && !m_bActiveGrapple)
			{
				if (state == MovementState.walking || state == MovementState.sprinting || state == MovementState.crouching)
					rb.linearDamping = groundDrag;
				else
					rb.linearDamping = 0;
			}
			else
			{
				rb.linearDamping = 0;
			}

			// Cache slope check once per FixedUpdate
			bool onSlope = OnSlope();

			// Only disable gravity for stick-to-slope movement when not sliding
			rb.useGravity = !(onSlope && !exitingSlope && !m_bSliding);

			// State/speed first
			StateHandler(onSlope);

			// Jump buffer countdown
			if (jumpBufferTimer > 0f)
				jumpBufferTimer -= Time.fixedDeltaTime;

			TryConsumeJumpBuffer();

			if (m_momentumScript != null)
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
		private float speedChangeFactor;

		void StateHandler(bool onSlope)
		{
			if (m_bOnRail)
			{
				state = MovementState.rail;
				desiredMoveSpeed = 0f;
				moveSpeed = 0f;
				return;
			}

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
			else if (m_bIsWallRunning)
			{
				state = MovementState.wallRunning;
				desiredMoveSpeed = wallRunSpeed;
			}
			else if (m_bSliding)
			{
				state = MovementState.sliding;
				desiredMoveSpeed = slideSpeed;
			}
			else if (m_bIsGrounded && inputManager.isCrouching)
			{
				Debug.Log("Crouching");
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

		private IEnumerator SmoothlyLerpMoveSpeed()
		{
			float time = 0f;
			float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
			float startValue = moveSpeed;

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
			if (m_bOnRail) return;

			moveDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

			if (onSlope && !exitingSlope)
			{
				rb.AddForce(GetSlopeMoveDirection(moveDir) * moveSpeed * 20f, ForceMode.Force);

				// small stick force so you don't float off slopes
				if (rb.linearVelocity.y > 0f)
					rb.AddForce(Vector3.down * 40f, ForceMode.Force);

				return;
			}

			if (m_bIsGrounded)
				rb.AddForce(moveDir * moveSpeed * 10f, ForceMode.Force);
			else
				rb.AddForce(moveDir * moveSpeed * 10f * airMultiplier, ForceMode.Force);

			if (!m_bIsWallRunning)
				rb.useGravity = !OnSlope();
		}

		private void SpeedControl(bool onSlope)
		{
			if (m_bActiveGrapple) return;
			if (m_bSliding) return;
			if (m_bOnRail) return;

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

			if (maxYSpeed != 0 && rb.linearVelocity.y > maxYSpeed)
				rb.linearVelocity = new Vector3(rb.linearVelocity.x, maxYSpeed, rb.linearVelocity.z);
		}

		// Leaving this method here so other scripts don't break if they reference it.
		// Your InputManager owns sprint state, so this no longer writes into isSprinting directly.
		public void Sprint(bool state)
		{
			Debug.LogWarning("Sprint(bool) is deprecated. Sprint state is now owned by InputManager.");
		}

		public void Jump()
		{
			// Rail jump = hop off the rail
			if (m_bOnRail)
			{
				ForceExitRail(true);
				return;
			}

			// Normal buffered jump
			jumpBufferTimer = jumpBufferTime;
			TryConsumeJumpBuffer();
		}

		private void TryConsumeJumpBuffer()
		{
			if (!readyToJump) return;
			if (jumpBufferTimer <= 0f) return;
			if (!m_bIsGrounded) return;
			if (m_bOnRail) return;

			ExecuteJump();

			jumpBufferTimer = 0f;
			readyToJump = false;
			Invoke(nameof(ResetJump), jumpCooldown);
		}

		private void ExecuteJump()
		{
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
		private Vector3 VelocityToSet;

		public void JumpToPosition(Vector3 targetPosition, float trajectoryHeight)
		{
			m_bActiveGrapple = true;

			VelocityToSet = CalculateJumpVelocity(transform.position, targetPosition, trajectoryHeight);
			Invoke(nameof(SetVelocity), 0.1f);
		}

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

			float halfHeight = m_cPlayerCollider.bounds.extents.y;

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

		public Vector3 GetDirection()
		{
			return moveDir;
		}

		// =========================
		// RAIL SYSTEM
		// =========================

		public void EnterRail(RailSpline rail)
		{
			if (rail == null || rail.SplineContainer == null)
				return;

			if (m_bOnRail && currentRail == rail)
				return;

			Vector3 incomingVelocity = rb.linearVelocity;

			// Cancel other modes that should not coexist with rails
			if (m_bSliding && slidingComp != null)
				slidingComp.ForceEndSlide();

			m_bActiveGrapple = false;
			m_bActiveSwing = false;
			m_bDashing = false;
			m_bSliding = false;
			m_bIsWallRunning = false;
			m_bFreeze = false;
			exitingSlope = false;

			currentRail = rail;
			m_bOnRail = true;
			state = MovementState.rail;

			rb.useGravity = false;
			rb.linearDamping = 0f;
			rb.angularVelocity = Vector3.zero;
			rb.linearVelocity = Vector3.zero;

			Vector3 localPlayerPos = rail.SplineContainer.transform.InverseTransformPoint(transform.position);
			Spline spline = rail.SplineContainer.Spline;

			SplineUtility.GetNearestPoint(
				spline,
				(float3)localPlayerPos,
				out float3 nearestLocal,
				out float nearestT,
				rail.NearestPointResolution,
				rail.NearestPointIterations);

			currentRailT = nearestT;
			currentRailSpeed = Mathf.Clamp(
				Mathf.Max(rail.EntrySpeed, incomingVelocity.magnitude),
				rail.MinSpeed,
				rail.MaxSpeed);

			SnapToRail();
		}

		private void SnapToRail()
		{
			if (!m_bOnRail || currentRail == null)
				return;

			Vector3 worldPos = currentRail.SplineContainer.EvaluatePosition(currentRailT);
			Vector3 tangent = ((Vector3)currentRail.SplineContainer.EvaluateTangent(currentRailT)).normalized;
			Vector3 up = currentRail.UseSplineUp
				? ((Vector3)currentRail.SplineContainer.EvaluateUpVector(currentRailT)).normalized
				: Vector3.up;

			rb.MovePosition(worldPos + up * currentRail.RideOffset);

			if (currentRail.RotateToRail && tangent.sqrMagnitude > 0.0001f)
			{
				Quaternion targetRotation = Quaternion.LookRotation(tangent, up);
				rb.MoveRotation(targetRotation);

				if (orientation != null)
					orientation.rotation = targetRotation;
			}
		}

		private void UpdateRailMovement(float deltaTime)
		{
			if (!m_bOnRail || currentRail == null)
				return;

			state = MovementState.rail;
			rb.useGravity = false;
			rb.linearDamping = 0f;

			float railInput = Mathf.Clamp(inputManager.MoveInput.y, -1f, 1f);

			// W = accelerate, S = brake
			if (railInput > 0f)
			{
				currentRailSpeed += railInput * currentRail.Acceleration * deltaTime;
			}
			else if (railInput < 0f)
			{
				currentRailSpeed -= (-railInput) * currentRail.BrakeDeceleration * deltaTime;
			}
			else
			{
				currentRailSpeed -= currentRail.PassiveDeceleration * deltaTime;
			}

			currentRailSpeed = Mathf.Clamp(currentRailSpeed, currentRail.MinSpeed, currentRail.MaxSpeed);

			Spline spline = currentRail.SplineContainer.Spline;

			float3 localPoint = SplineUtility.GetPointAtLinearDistance(
				spline,
				currentRailT,
				currentRailSpeed * deltaTime,
				out float newT);

			currentRailT = Mathf.Clamp01(newT);

			Vector3 worldPos = currentRail.SplineContainer.transform.TransformPoint((Vector3)localPoint);
			Vector3 tangent = ((Vector3)currentRail.SplineContainer.EvaluateTangent(currentRailT)).normalized;
			Vector3 up = currentRail.UseSplineUp
				? ((Vector3)currentRail.SplineContainer.EvaluateUpVector(currentRailT)).normalized
				: Vector3.up;

			rb.MovePosition(worldPos + up * currentRail.RideOffset);

			if (currentRail.RotateToRail && tangent.sqrMagnitude > 0.0001f)
			{
				Quaternion targetRotation = Quaternion.LookRotation(tangent, up);
				rb.MoveRotation(targetRotation);

				if (orientation != null)
					orientation.rotation = targetRotation;
			}

			if (currentRail.AutoExitAtEnd && currentRailT >= 0.999f)
			{
				ForceExitRail(false);
			}
		}

		public void ForceExitRail(bool jumpedOff)
		{
			if (!m_bOnRail)
				return;

			Vector3 tangent = transform.forward;
			Vector3 up = Vector3.up;

			if (currentRail != null && currentRail.SplineContainer != null)
			{
				tangent = ((Vector3)currentRail.SplineContainer.EvaluateTangent(currentRailT)).normalized;
				up = currentRail.UseSplineUp
					? ((Vector3)currentRail.SplineContainer.EvaluateUpVector(currentRailT)).normalized
					: Vector3.up;
			}

			m_bOnRail = false;
			currentRail = null;

			rb.useGravity = true;
			rb.linearDamping = 0f;

			Vector3 exitVelocity = tangent * (currentRailSpeed + railExitForwardBoost);

			if (jumpedOff)
				exitVelocity += up * railJumpUpForce;

			rb.linearVelocity = exitVelocity;
			state = MovementState.air;
		}
	}
}