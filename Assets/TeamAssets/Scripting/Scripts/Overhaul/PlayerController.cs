using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	[Header("References")]
	private InputManager2 inputManager;

	[Header("Movement")]
	[SerializeField] float walkSpeed;
	[SerializeField] float sprintSpeed;
	[SerializeField] float slideSpeed;
	[SerializeField] float wallRunSpeed;
	[SerializeField] float groundDrag;

	float moveSpeed;
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
	public enum MovementState { walking, sprinting, crouching, sliding, air, wallRunning }

	public bool sliding;
	public bool m_bIsGrounded;
	public bool m_bSprinting;
	public bool m_bCrouching;
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

	public bool IsGrounded => m_bIsGrounded;
	public Vector3 SlopeNormal => slopeHit.normal;

    private void Awake()
	{
		inputManager = GetComponent<InputManager2>();

		rb = GetComponent<Rigidbody>();
		rb.freezeRotation = true;

		startYScale = transform.localScale.y;
		crouchYScale = startYScale / 2f;

		m_cPlayerCollider = GetComponentInChildren<Collider>();
		slidingComp = GetComponent<Sliding>();
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

		// Drag only when grounded AND not sliding (sliding should keep momentum)
		rb.linearDamping = (m_bIsGrounded && !sliding) ? groundDrag : 0f;

		// Cache slope check once per FixedUpdate (updates slopeHit)
		bool onSlope = OnSlope();

		// Only disable gravity for your custom "stick-to-slope" movement WHEN NOT sliding
		rb.useGravity = !(onSlope && !exitingSlope && !sliding);

		// State/speed first (so movement uses correct moveSpeed this frame)
		StateHandler(onSlope);

		// Jump buffer countdown
		if (jumpBufferTimer > 0f)
			jumpBufferTimer -= Time.fixedDeltaTime;

		TryConsumeJumpBuffer();

		// Sliding movement is handled by Sliding.cs
		if (!sliding)
		{
			MovePlayer(onSlope);
			SpeedControl(onSlope);
		}
	}

	void StateHandler(bool onSlope)
	{
		if(m_bIsWallRunning)
		{
			state = MovementState.wallRunning;
			desiredMoveSpeed = wallRunSpeed;
		}

		// Priority order matters: sliding > crouch > sprint > walk > air
		if (sliding)
		{
			state = MovementState.sliding;
			desiredMoveSpeed = slideSpeed;
		}
		else if (m_bIsGrounded && m_bCrouching)
		{
			state = MovementState.crouching;
			desiredMoveSpeed = crouchSpeed;
		}
		else if (m_bIsGrounded && m_bSprinting)
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
			// keep last desiredMoveSpeed in air so you don't snap speeds weirdly
		}

		if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4f && moveSpeed != 0f)
		{
			StopAllCoroutines();
			StartCoroutine(SmoothlyLerpMovementSpeed());
		}
		else
		{
			moveSpeed = desiredMoveSpeed;
		}

		lastDesiredMoveSpeed = desiredMoveSpeed;
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

	public void GetInput(Vector2 input)
	{
		horizontalInput = input.x;
		verticalInput = input.y;
	}

	private IEnumerator SmoothlyLerpMovementSpeed()
	{
		float time = 0f;
		float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
		float startValue = moveSpeed;

		while (time < difference)
		{
			moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

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
	}

	private void MovePlayer(bool onSlope)
	{
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
		// Don't clamp during sliding; Sliding.cs handles momentum/friction
		if (sliding) return;

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
	}

	public void Sprint(bool state) => m_bSprinting = state;

	public void Crouch(bool state)
	{
		m_bCrouching = state;

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
		if (sliding && slidingComp != null)
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
}