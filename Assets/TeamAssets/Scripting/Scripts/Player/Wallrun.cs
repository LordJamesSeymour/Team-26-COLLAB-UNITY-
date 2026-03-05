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

	// NEW: jump input buffer for FixedUpdate
	public bool m_jumpRequested;

	private void Awake()
	{
		m_rigidbody = GetComponent<Rigidbody>();
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
		CheckWall();

		if (CanWallRun() && (wallLeft || wallRight))
		{
			StartWallRun();

			if (wallLeft) Debug.Log("Wall running on the left");
			if (wallRight) Debug.Log("Wall running on the right");
		}
		else
		{
			StopWallRun();
		}
	}

	void StartWallRun()
	{
		m_rigidbody.useGravity = false;

		m_rigidbody.AddForce(Vector3.down * m_player.m_fWallrunGravity, ForceMode.Force);

		if (m_jumpRequested)
		{
			if (wallLeft)
			{
				Vector3 wallRunJumpDir = transform.up + leftWallHit.normal;
				m_rigidbody.linearVelocity = new Vector3(m_rigidbody.linearVelocity.x, 0, m_rigidbody.linearVelocity.z);
				m_rigidbody.AddForce(wallRunJumpDir * m_player.m_fWalljumpForce * 100f, ForceMode.Force);
				m_jumpRequested = false;
			}
			else if (wallRight)
			{
				Vector3 wallRunJumpDir = transform.up + rightWallHit.normal;
				m_rigidbody.linearVelocity = new Vector3(m_rigidbody.linearVelocity.x, 0, m_rigidbody.linearVelocity.z);
				m_rigidbody.AddForce(wallRunJumpDir * m_player.m_fWalljumpForce * 100f, ForceMode.Force);
				m_jumpRequested = false;
			}
		}
	}

	void StopWallRun()
	{
		m_rigidbody.useGravity = true;
		m_jumpRequested = false; // optional: prevents buffered jump after leaving wall
	}
}