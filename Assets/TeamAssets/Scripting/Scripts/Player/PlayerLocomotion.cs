using UnityEngine;

public class PlayerLocomotion : MonoBehaviour
{
	[SerializeField] bool m_bIsGrounded;
	[SerializeField] Transform m_tPlayerOrientation;
	[SerializeField] PlayerStats_SO playerStats_SO;
	[SerializeField] LayerMask m_lGround;

	Rigidbody m_rigidbody;

	// Store raw input, then build direction every FixedUpdate using CURRENT orientation.
	Vector2 m_v2MoveInput;
	Vector3 moveDirection;
	Vector3 slopeMoveDir;

	float m_fGroundDistance = 0.4f;

	RaycastHit m_rSlopeHit;

	Collider m_cPlayerCollider;
	float halfHeight;

	private void Awake()
	{
		if (m_rigidbody == null)
			m_rigidbody = GetComponent<Rigidbody>();

		m_rigidbody.freezeRotation = true;

		m_cPlayerCollider = GetComponentInChildren<Collider>();
		halfHeight = m_cPlayerCollider.bounds.extents.y; // world half-height
	}

	private void FixedUpdate()
	{
		m_bIsGrounded = Physics.CheckSphere(transform.position - new Vector3(0,1,0), m_fGroundDistance, m_lGround);

		// Recompute direction using the latest orientation every physics tick.
		moveDirection = (m_tPlayerOrientation.forward * m_v2MoveInput.y) +
						(m_tPlayerOrientation.right * m_v2MoveInput.x);

		PlayerMove();
		PlayerDrag();
	}

	// Called by InputManager
	public void SetMoveInput(Vector2 input)
	{
		m_v2MoveInput = input;
	}

	public void PlayerJump()
	{
		if (m_bIsGrounded)
		{
			m_rigidbody.AddForce(transform.up * playerStats_SO.m_fPlayerJumpForce, ForceMode.Impulse);
		}
	}

	void PlayerMove()
	{
		slopeMoveDir = Vector3.ProjectOnPlane(moveDirection, m_rSlopeHit.normal);

		if (m_bIsGrounded && !OnSlope())
		{
			m_rigidbody.AddForce(moveDirection.normalized * playerStats_SO.m_fPlayerWalkSpeed * playerStats_SO.m_fGroundMultiplier, ForceMode.Acceleration);
		}
		else if (m_bIsGrounded && OnSlope())
		{
			m_rigidbody.AddForce(slopeMoveDir.normalized * playerStats_SO.m_fPlayerWalkSpeed * playerStats_SO.m_fGroundMultiplier, ForceMode.Acceleration);
		}
		else if(!m_bIsGrounded)
		{
			m_rigidbody.AddForce(moveDirection.normalized * playerStats_SO.m_fPlayerWalkSpeed * playerStats_SO.m_fAirMultiplier, ForceMode.Acceleration);
		}
	}

	void PlayerDrag()
	{
		if (m_bIsGrounded) m_rigidbody.linearDamping = playerStats_SO.m_fGroundDrag;
		else m_rigidbody.linearDamping = playerStats_SO.m_fAirDrag;
	}

	private bool OnSlope()
	{
		if (Physics.Raycast(transform.position, Vector3.down, out m_rSlopeHit, halfHeight + 0.35f))
		{
			if (m_rSlopeHit.normal != Vector3.up)
			{
				return true;
			}
			else
			{
				return false; 
			}	
		}
		return false;
	}
}