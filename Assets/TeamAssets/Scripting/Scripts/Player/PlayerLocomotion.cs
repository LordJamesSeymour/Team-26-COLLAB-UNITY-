using Group26.Player.Input;
using UnityEngine;

namespace Group26.Player.Locomotion
{
	public class PlayerLocomotion : MonoBehaviour
{
	private InputManager playerInput;

	[SerializeField] bool m_bIsGrounded;
	[SerializeField] Transform m_tPlayerOrientation;
	[SerializeField] PlayerStats_SO playerStats_SO;
	[SerializeField] LayerMask m_lGround;
	[SerializeField] Transform m_tGroundCheck;

	Rigidbody m_rigidbody;
	public float moveSpeed;

	// Store raw input, then build direction every FixedUpdate using CURRENT orientation.
	Vector2 m_v2MoveInput;
	Vector3 moveDirection;
	Vector3 slopeMoveDir;
	float m_fGroundDistance = 0.4f;
	RaycastHit m_rSlopeHit;
	Collider m_cPlayerCollider;
	float halfHeight;

	// NEW: sprint state (set by input, used every tick)
	bool m_isSprinting;

	//NEW: momentum script
	private SlopeMomentum m_momentumController;

        private void OnEnable()
        {
            playerInput.OnJumpPressed += PlayerJump;
        }

        private void OnDisable()
        {
            playerInput.OnJumpPressed -= PlayerJump;
        }

        private void Awake()
	{
		playerInput = GetComponent<InputManager>();
		if(playerInput == null) Debug.LogError("No input manager found");

		moveSpeed = playerStats_SO.m_fPlayerWalkSpeed;

		if (m_rigidbody == null)
			m_rigidbody = GetComponent<Rigidbody>();

		m_rigidbody.freezeRotation = true;

		//CHANGED: get reference to momentum script
		m_momentumController = GetComponent<SlopeMomentum>();
		if (!m_momentumController)
			Debug.LogError("no momentum script found");

		m_cPlayerCollider = GetComponentInChildren<Collider>();
		halfHeight = m_cPlayerCollider.bounds.extents.y; // world half-height
	}

	private void FixedUpdate()
	{
		float dt = Time.fixedDeltaTime;

		m_bIsGrounded = Physics.CheckSphere(m_tGroundCheck.position, m_fGroundDistance, m_lGround);

		// Cache slope result ONCE per tick (also updates m_rSlopeHit if true)
		bool isOnSlope = OnSlope();

		// Recompute direction using the latest orientation every physics tick.
		moveDirection = (m_tPlayerOrientation.forward * m_v2MoveInput.y) +
						(m_tPlayerOrientation.right * m_v2MoveInput.x);

		UpdateMoveSpeed(dt);

		SetMoveInput(playerInput.MoveInput);
		PlayerMove(isOnSlope);
		PlayerSprint(playerInput.isSprinting);
		PlayerDrag();
	}

	// Trying to pass in the MoveInput vector from InputManager?
	public void SetMoveInput(Vector2 moveInput)
	{
		m_v2MoveInput = moveInput;
	}

	public void PlayerJump()
	{
		if (m_bIsGrounded)
		{
			m_rigidbody.linearVelocity = new Vector3(m_rigidbody.linearVelocity.x, 0, m_rigidbody.linearVelocity.z);
			m_rigidbody.AddForce(transform.up * playerStats_SO.m_fPlayerJumpForce, ForceMode.Impulse);
		}
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
		else if (!m_bIsGrounded)
		{
			m_rigidbody.AddForce(moveDirection.normalized * moveSpeed * playerStats_SO.m_fAirMultiplier,
				ForceMode.Acceleration);
		}
	}

	void PlayerDrag()
	{
		if (m_bIsGrounded) m_rigidbody.linearDamping = playerStats_SO.m_fGroundDrag;
		else m_rigidbody.linearDamping = playerStats_SO.m_fAirDrag;
	}

	// CHANGED: this now sets sprint state only (no lerp here)
	public void PlayerSprint(bool sprinting)
	{
		m_isSprinting = sprinting;
	}

	// NEW: speed smoothing runs every physics tick
	void UpdateMoveSpeed(float dt)
	{
		float targetSpeed =
			(m_isSprinting && m_bIsGrounded)
				? playerStats_SO.m_fPlayerRunSpeed
				: playerStats_SO.m_fPlayerWalkSpeed;

		// Option A: Keep your Lerp-style smoothing
		moveSpeed = Mathf.Lerp(moveSpeed, targetSpeed, playerStats_SO.m_fPlayerAcceleration * dt);
		//CHANGED: add momentum to speed
		moveSpeed += m_momentumController.m_momentum;

		// Option B (often nicer): true "units per second" acceleration
		// moveSpeed = Mathf.MoveTowards(moveSpeed, targetSpeed, playerStats_SO.m_fPlayerAcceleration * dt);
	}

	//CHANGED: Made this public so it can be accessed by slope momentum script
	public bool OnSlope()
	{
		if (Physics.Raycast(transform.position, Vector3.down, out m_rSlopeHit, halfHeight + 0.35f))
		{
			return m_rSlopeHit.normal != Vector3.up;
		}
		return false;
	}

	public Vector3 GetDirection() { return moveDirection; }
	}
}