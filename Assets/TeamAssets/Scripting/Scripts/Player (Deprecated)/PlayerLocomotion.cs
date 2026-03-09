using System;
using UnityEngine;

public class PlayerLocomotion : MonoBehaviour
{
	[SerializeField] bool m_bIsGrounded;
	[SerializeField] Transform m_tPlayerOrientation;
	[SerializeField] PlayerStats_SO playerStats_SO;
	[SerializeField] LayerMask m_lGround;
	[SerializeField] Transform m_tGroundCheck;

	Rigidbody m_rigidbody;

	float moveSpeed;

	// Store raw input, then build direction every FixedUpdate using CURRENT orientation.
	Vector2 m_v2MoveInput;
	Vector3 moveDirection;
	Vector3 slopeMoveDir;

	float m_fGroundDistance = 0.4f;

	RaycastHit m_rSlopeHit;

	Collider m_cPlayerCollider;
	float halfHeight;

	// sprint state (set by input, used every tick)
	bool m_isSprinting;

	// Jump buffer runtime counter (time remaining)
	float m_fJumpBufferCounter = 0f;

	// Wallrun movement mode (set by Wallrun)
	bool m_isWallRunning = false;
	Vector3 m_wallNormal = Vector3.zero;
	int m_wallSideSign = 0; // Right wall = +1, Left wall = -1

	private void Awake()
	{
		moveSpeed = playerStats_SO.m_fPlayerWalkSpeed;

		if (m_rigidbody == null)
			m_rigidbody = GetComponent<Rigidbody>();

		m_rigidbody.freezeRotation = true;

		m_cPlayerCollider = GetComponentInChildren<Collider>();
		halfHeight = m_cPlayerCollider.bounds.extents.y; // world half-height
	}

	private void FixedUpdate()
	{
		float dt = Time.fixedDeltaTime;

		// Ground check
		m_bIsGrounded = Physics.CheckSphere(m_tGroundCheck.position, m_fGroundDistance, m_lGround);

		// Buffered jump triggers on landing
		if (m_bIsGrounded && m_fJumpBufferCounter > 0f)
			ConsumeBufferedJump();

		// Tick buffer down (clamped)
		if (m_fJumpBufferCounter > 0f)
			m_fJumpBufferCounter = Mathf.Max(m_fJumpBufferCounter - dt, 0f);

		// Cache slope result ONCE per tick (also updates m_rSlopeHit if true)
		bool isOnSlope = OnSlope();

		UpdateMoveSpeed(dt);

		if (m_isWallRunning)
		{
			BuildWallrunMoveDirection();
			PlayerMoveWallrun();
		}
		else
		{
			// Recompute direction using the latest orientation every physics tick.
			moveDirection = (m_tPlayerOrientation.forward * m_v2MoveInput.y) +
							(m_tPlayerOrientation.right * m_v2MoveInput.x);

			PlayerMove(isOnSlope);
		}

		PlayerDrag();
	}

	// Called by InputManager
	public void SetMoveInput(Vector2 input) => m_v2MoveInput = input;

	// Called by InputManager when jump is PRESSED (performed)
	public void PlayerJump()
	{
		m_fJumpBufferCounter = playerStats_SO.m_fJumpBufferTime;

		bool groundedNow = Physics.CheckSphere(m_tGroundCheck.position, m_fGroundDistance, m_lGround);
		if (groundedNow)
		{
			m_bIsGrounded = true;
			ConsumeBufferedJump();
		}
	}

	void ConsumeBufferedJump()
	{
		m_fJumpBufferCounter = 0f;

		m_rigidbody.linearVelocity = new Vector3(m_rigidbody.linearVelocity.x, 0f, m_rigidbody.linearVelocity.z);
		m_rigidbody.AddForce(transform.up * playerStats_SO.m_fPlayerJumpForce, ForceMode.Impulse);

		m_bIsGrounded = false;
	}

	void PlayerMove(bool isOnSlope)
	{
		if (moveDirection.sqrMagnitude < 0.0001f)
			return;

		if (m_bIsGrounded && !isOnSlope)
		{
			m_rigidbody.AddForce(moveDirection.normalized * moveSpeed * playerStats_SO.m_fGroundMultiplier,
				ForceMode.Acceleration);
		}
		else if (m_bIsGrounded && isOnSlope)
		{
			slopeMoveDir = Vector3.ProjectOnPlane(moveDirection, m_rSlopeHit.normal);
			m_rigidbody.AddForce(slopeMoveDir.normalized * moveSpeed * playerStats_SO.m_fGroundMultiplier,
				ForceMode.Acceleration);
		}
		else
		{
			m_rigidbody.AddForce(moveDirection.normalized * moveSpeed * playerStats_SO.m_fAirMultiplier,
				ForceMode.Acceleration);
		}
	}

	// --- Wallrun movement (subtle mapping) ---
	void BuildWallrunMoveDirection()
	{
		Vector3 wallForward = Vector3.ProjectOnPlane(m_tPlayerOrientation.forward, m_wallNormal);
		if (wallForward.sqrMagnitude < 0.0001f)
			wallForward = Vector3.Cross(Vector3.up, m_wallNormal);

		wallForward.Normalize();

		// A/D becomes up/down (subtle scale), inverted per wall side
		float verticalInput = m_v2MoveInput.x * m_wallSideSign * playerStats_SO.m_fWallrunVerticalInputScale;

		// IMPORTANT: no normalization => subtle diagonals (no "full power" snap)
		moveDirection = (wallForward * m_v2MoveInput.y) + (Vector3.up * verticalInput);
	}

	void PlayerMoveWallrun()
	{
		if (moveDirection.sqrMagnitude < 0.0001f)
			return;

		// Uses moveSpeed, so sprinting affects wallrun too
		m_rigidbody.AddForce(moveDirection * moveSpeed * playerStats_SO.m_fWallrunMultiplier,
			ForceMode.Acceleration);
	}

	void PlayerDrag()
	{
		if (m_isWallRunning)
		{
			m_rigidbody.linearDamping = playerStats_SO.m_fWallrunDrag;
			return;
		}

		m_rigidbody.linearDamping = m_bIsGrounded ? playerStats_SO.m_fGroundDrag : playerStats_SO.m_fAirDrag;
	}

	public void PlayerSprint(bool sprinting) => m_isSprinting = sprinting;

	// Wallrun tells locomotion when to switch movement plane
	public void SetWallrunState(bool wallRunning, Vector3 wallNormal, int wallSideSign)
	{
		m_isWallRunning = wallRunning;

		if (!wallRunning)
		{
			m_wallNormal = Vector3.zero;
			m_wallSideSign = 0;
			return;
		}

		m_wallNormal = wallNormal.normalized;
		m_wallSideSign = Mathf.Clamp(wallSideSign, -1, 1);
	}

	void UpdateMoveSpeed(float dt)
	{
		float targetSpeed = m_isWallRunning
			? (m_isSprinting ? playerStats_SO.m_fPlayerRunSpeed : playerStats_SO.m_fPlayerWalkSpeed)
			: ((m_isSprinting && m_bIsGrounded) ? playerStats_SO.m_fPlayerRunSpeed : playerStats_SO.m_fPlayerWalkSpeed);

		moveSpeed = Mathf.Lerp(moveSpeed, targetSpeed, playerStats_SO.m_fPlayerAcceleration * dt);
	}

	private bool OnSlope()
	{
		if (Physics.Raycast(transform.position, Vector3.down, out m_rSlopeHit, halfHeight + 0.35f))
			return m_rSlopeHit.normal != Vector3.up;

		return false;
	}
}