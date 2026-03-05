using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
	[Header("Movement")]
	[SerializeField] float walkSpeed;
	[SerializeField] float sprintSpeed;
	[SerializeField] float slideSpeed;
	[SerializeField] float groundDrag;

	private float moveSpeed;

	private float desiredMoveSpeed;
	private float lastDesiredMoveSpeed;

	[SerializeField] float speedIncreaseMultiplier;
	[SerializeField] float slopeIncreaseMultiplier;

	[Header("Jumping")]
	[SerializeField] float jumpForce;
	[SerializeField] float jumpCooldown;
	[SerializeField] float airMultiplier;
	[SerializeField] float jumpBufferTime = 0.15f; // how long we remember a jump press
	private bool exitingSlope;

	[Header("Crouching")]
	[SerializeField] float crouchSpeed;
	private float startYScale;
	private float crouchYScale;

	[Header("Ground Check")]
	[SerializeField] float m_fGroundDistance = 0.15f;
	[SerializeField] Transform m_tGroundCheck;
	[SerializeField] LayerMask m_lGround;

	[Header("Slope Handling")]
	[SerializeField] float MaxSlopeAngle;
	RaycastHit slopeHit;

	public MovementState state;
	public enum MovementState
	{
		walking,
		sprinting,
		crouching,
		sliding,
		air
	}

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
	float halfHeight;

	// Jump buffer + cooldown state
	float jumpBufferTimer = 0f;
	bool readyToJump = true;

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		rb.freezeRotation = true;

		startYScale = transform.localScale.y;
		crouchYScale = startYScale / 2f;

		m_cPlayerCollider = GetComponentInChildren<Collider>();
		halfHeight = m_cPlayerCollider.bounds.extents.y; // world half-height
	}

	private void FixedUpdate()
	{
		// Ground check FIRST (so movement/jump uses the current grounded state)
		m_bIsGrounded = Physics.CheckSphere(m_tGroundCheck.position, m_fGroundDistance, m_lGround);

		if (m_bIsGrounded) rb.linearDamping = groundDrag;
		else rb.linearDamping = 0;

		// Tick down jump buffer
		if (jumpBufferTimer > 0f)
			jumpBufferTimer -= Time.fixedDeltaTime;

		// If jump was pressed recently, and we can jump now, consume the buffer
		TryConsumeJumpBuffer();

		MovePlayer();
		SpeedControl();
		StateHandler();
	}

	void StateHandler()
	{
		if (sliding)
		{
			state = MovementState.sliding;

			if (OnSlope() && rb.linearVelocity.y < 0.1f)
			{
				desiredMoveSpeed = slideSpeed;
			} else
				desiredMoveSpeed = sprintSpeed;
		}

		else if (m_bIsGrounded && m_bSprinting)
		{
			state = MovementState.sprinting;
			desiredMoveSpeed = sprintSpeed;
		}
		else if (m_bIsGrounded && !m_bSprinting)
		{
			state = MovementState.walking;
			desiredMoveSpeed = walkSpeed;
		}
		else if (m_bCrouching && m_bIsGrounded)
		{
			state = MovementState.crouching;
			desiredMoveSpeed = crouchSpeed;
		}
		else if (!m_bIsGrounded)
		{
			state = MovementState.air;
		}


		// Check if desiredMoveSpeed has changed drastically

		if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4f && moveSpeed != 0)
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
		if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, halfHeight + 0.25f, m_lGround))
		{
			float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
			return angle < MaxSlopeAngle && angle != 0;
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
		// Smoothly lerp movement speed to desired value
		float time = 0;
		float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
		float startValue = moveSpeed;

		while(time < difference)
		{
			moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

			if (OnSlope())
			{
				float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
				float slopeAngleIncrease = 1 + (slopeAngle / 90f);

				time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
			}
			else
				time += Time.deltaTime * speedIncreaseMultiplier;

			yield return null;
		}

		moveSpeed = desiredMoveSpeed;
	}

	private void MovePlayer()
	{
		moveDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

		if (OnSlope() && !exitingSlope)
		{
			rb.AddForce(GetSlopeMoveDirection(moveDir) * moveSpeed * 20f, ForceMode.Force);

			if (rb.linearVelocity.y > 0) rb.AddForce(Vector3.down * 80f, ForceMode.Force);
		}

		if (m_bIsGrounded)
			rb.AddForce(moveDir * moveSpeed * 10f, ForceMode.Force);
		else
			rb.AddForce(moveDir * moveSpeed * 10f * airMultiplier, ForceMode.Force);


		rb.useGravity = !OnSlope();
	}

	private void SpeedControl()
	{
		// Limiting the speed on slopes
		if (OnSlope() && !exitingSlope)
		{
			if (rb.linearVelocity.magnitude > moveSpeed)
			{
				rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
			}
		}

		// Limiting the speed on the air/ground
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

	public void Sprint(bool state)
	{
		m_bSprinting = state;
	}

	public void Crouch(bool state)
	{
		m_bCrouching = state;

		if (state)
		{
			transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
			rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
		}
		else transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);

	}

	// Call this from your InputSystem: Jump.performed += _ => Jump();
	public void Jump()
	{
		exitingSlope = true;

		// Store the jump press for a short time
		jumpBufferTimer = jumpBufferTime;

		// If we're already in a valid state, jump immediately
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
		rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
		rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
	}

	private void ResetJump()
	{
		readyToJump = true;
		exitingSlope = false;
	}
}