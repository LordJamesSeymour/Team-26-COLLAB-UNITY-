using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class PlayerController : MonoBehaviour
{
	[Header("Movement")]
	[SerializeField] float walkSpeed;
	[SerializeField] float sprintSpeed;
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
	public enum MovementState { walking, sprinting, crouching, sliding, air }

	public bool sliding;
	bool m_bIsGrounded;
	bool m_bSprinting;
	bool m_bCrouching;

	[SerializeField] Transform orientation;

	float horizontalInput;
	float verticalInput;

	Vector3 moveDir;
	Rigidbody rb;

	Collider m_cPlayerCollider;
	Sliding slidingComp;

	// Jump buffer + cooldown state
	float jumpBufferTimer;
	bool readyToJump = true;

	public bool IsGrounded => m_bIsGrounded;
	public Vector3 SlopeNormal => slopeHit.normal;

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		rb.freezeRotation = true;

		startYScale = transform.localScale.y;
		crouchYScale = startYScale / 2f;

		m_cPlayerCollider = GetComponentInChildren<Collider>();
		slidingComp = GetComponent<Sliding>();
	}

	private void FixedUpdate()
	{
		// Ground check first
		m_bIsGrounded = Physics.CheckSphere(m_tGroundCheck.position, m_fGroundDistance, m_lGround);

		// Drag: keep momentum while sliding
		rb.linearDamping = (m_bIsGrounded && !sliding) ? groundDrag : 0f;

		// Update slopeHit once per tick
		bool onSlope = OnSlope();

		// Gravity: allow normal gravity during sliding
		rb.useGravity = !(onSlope && !exitingSlope && !sliding);

		// State/speed before forces
		StateHandler();

		// Jump buffer countdown
		if (jumpBufferTimer > 0f)
			jumpBufferTimer -= Time.fixedDeltaTime;

		TryConsumeJumpBuffer();

		// Sliding movement handled in Sliding.cs
		if (!sliding)
		{
			MovePlayer(onSlope);
			SpeedControl(onSlope);
		}
	}

	void StateHandler()
	{
		// Priority: sliding > crouch > sprint > walk > air
		if (sliding)
		{
			state = MovementState.sliding;
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
			// keep desiredMoveSpeed as last grounded setting
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

		// Recompute extents because you scale for crouch/slide visuals
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

			if (rb.linearVelocity.y > 0f)
				rb.AddForce(Vector3.down * 40f, ForceMode.Force);

			return; // don't also apply flat forces
		}

		if (m_bIsGrounded)
			rb.AddForce(moveDir * moveSpeed * 10f, ForceMode.Force);
		else
			rb.AddForce(moveDir * moveSpeed * 10f * airMultiplier, ForceMode.Force);
	}

	private void SpeedControl(bool onSlope)
	{
		// Don't clamp while sliding (momentum owned by Sliding.cs)
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
		// Jump cancels slide immediately
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