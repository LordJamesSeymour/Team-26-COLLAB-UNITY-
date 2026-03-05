using UnityEngine;

public class Wallrun : MonoBehaviour
{
	[SerializeField] Transform m_tPlayerOrientation;

	[Header("Wall Running")]
	[SerializeField] PlayerStats_SO m_player;

	bool wallLeft = false;
	bool wallRight = false;

	RaycastHit leftWallHit;
	RaycastHit rightWallHit;

	Rigidbody m_rigidbody;
	PlayerLocomotion m_locomotion;

	// set by input (pressed)
	public bool m_jumpRequested;

	bool m_isWallRunning;

	// Prevents instantly re-hooking the wall right after a wall jump
	float m_fReattachCooldown = 0f;
	const float kReattachCooldownTime = 0.20f;

	private void Awake()
	{
		m_rigidbody = GetComponent<Rigidbody>();
		m_locomotion = GetComponent<PlayerLocomotion>();
	}

	bool CanWallRun()
	{
		return !Physics.Raycast(transform.position, Vector3.down, m_player.minimumJumpHeight);
	}

	void CheckWall()
	{
		wallLeft = Physics.Raycast(transform.position, -m_tPlayerOrientation.right, out leftWallHit, m_player.wallDistance);
		wallRight = Physics.Raycast(transform.position, m_tPlayerOrientation.right, out rightWallHit, m_player.wallDistance);
	}

	private void FixedUpdate()
	{
		float dt = Time.fixedDeltaTime;
		if (m_fReattachCooldown > 0f)
			m_fReattachCooldown = Mathf.Max(0f, m_fReattachCooldown - dt);

		CheckWall();

		bool canAttach = (m_fReattachCooldown <= 0f);
		bool shouldWallRun = canAttach && CanWallRun() && (wallLeft || wallRight);

		if (shouldWallRun && !m_isWallRunning)
			EnterWallRun();
		else if (!shouldWallRun && m_isWallRunning)
			ExitWallRun();

		if (m_isWallRunning)
			UpdateWallRun();
	}

	void EnterWallRun()
	{
		m_isWallRunning = true;
		m_rigidbody.useGravity = false;

		Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
		int wallSideSign = wallRight ? +1 : -1;
		m_locomotion?.SetWallrunState(true, wallNormal, wallSideSign);

		// Keep whatever momentum you have, but don't let it be pushing away from the wall at attach
		Vector3 v = m_rigidbody.linearVelocity;
		float outward = Vector3.Dot(v, wallNormal);
		if (outward > 0f) v -= wallNormal * outward;
		m_rigidbody.linearVelocity = v;
	}

	void UpdateWallRun()
	{
		Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
		int wallSideSign = wallRight ? +1 : -1;

		m_locomotion?.SetWallrunState(true, wallNormal, wallSideSign);

		// If jump pressed, do the jump FIRST (don’t apply stick/anti-outward on the jump frame)
		if (m_jumpRequested)
		{
			DoWallJump(wallNormal);
			return;
		}

		// Custom gravity while wallrunning
		m_rigidbody.AddForce(Vector3.down * m_player.m_fWallrunGravity, ForceMode.Acceleration);

		// Keep contact: stick into wall
		m_rigidbody.AddForce(-wallNormal * m_player.m_fWallStickForce, ForceMode.Acceleration);

		// Remove only "away from wall" velocity so you don't drift off
		Vector3 v = m_rigidbody.linearVelocity;
		float outward = Vector3.Dot(v, wallNormal);
		if (outward > 0f)
			m_rigidbody.linearVelocity = v - wallNormal * outward;
	}

	void DoWallJump(Vector3 wallNormal)
	{
		// Stop wallrun state BEFORE applying the jump so we don't instantly "stick" again
		ExitWallRun();
		m_fReattachCooldown = kReattachCooldownTime;

		// Build a jump direction that is:
		// - away from the wall (normal)
		// - upward
		// - plus forward (so it feels like a leap away, not a slide)
		Vector3 wallForward = Vector3.ProjectOnPlane(m_tPlayerOrientation.forward, wallNormal);
		if (wallForward.sqrMagnitude < 0.0001f)
			wallForward = Vector3.Cross(Vector3.up, wallNormal);
		wallForward.Normalize();

		Vector3 jumpDir = (wallNormal + Vector3.up + wallForward).normalized;

		// Reset vertical so jump is consistent
		m_rigidbody.linearVelocity = new Vector3(m_rigidbody.linearVelocity.x, 0f, m_rigidbody.linearVelocity.z);

		// Impulse jump
		m_rigidbody.AddForce(jumpDir * m_player.m_fWalljumpForce, ForceMode.Impulse);

		m_jumpRequested = false;
	}

	void ExitWallRun()
	{
		m_isWallRunning = false;

		m_locomotion?.SetWallrunState(false, Vector3.zero, 0);

		m_rigidbody.useGravity = true;
		m_jumpRequested = false;
	}
}