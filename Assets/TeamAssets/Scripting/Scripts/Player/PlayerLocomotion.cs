using UnityEngine;

public class PlayerLocomotion : MonoBehaviour
{
    [SerializeField] bool m_bIsGrounded;
    [SerializeField] Transform m_tPlayerOrientation;
    [SerializeField] PlayerStats_SO playerStats_SO;

    Rigidbody m_rigidbody;

    Vector3 moveDirection;

	Collider m_cPlayerCollider;
	float halfHeight;

    private void Awake()
    {
		// Grab the Rigid body and assign it
		if (m_rigidbody == null)
		{
			m_rigidbody = GetComponent<Rigidbody>();
		}
		m_rigidbody.freezeRotation = true;

		m_cPlayerCollider = GetComponentInChildren<Collider>();
		halfHeight = m_cPlayerCollider.bounds.extents.y;           // world half-height
	}

	private void FixedUpdate()
	{
		m_bIsGrounded = Physics.Raycast(m_cPlayerCollider.bounds.center, Vector3.down, halfHeight + 0.1f);

		Debug.Log(transform.localScale.y);

		Debug.Log(m_bIsGrounded);

		PlayerMove();
		PlayerDrag();

	}

	public void PlayerInput(Vector2 InputV2)
    {
        moveDirection = m_tPlayerOrientation.forward * InputV2.y + m_tPlayerOrientation.right * InputV2.x;
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
		if (m_bIsGrounded) m_rigidbody.AddForce(moveDirection.normalized * playerStats_SO.m_fPlayerWalkSpeed * playerStats_SO.m_fGroundMultiplier, ForceMode.Acceleration);
		else m_rigidbody.AddForce(moveDirection.normalized * playerStats_SO.m_fPlayerWalkSpeed * playerStats_SO.m_fAirMultiplier, ForceMode.Acceleration);
	}

	void PlayerDrag()
	{
		if (m_bIsGrounded) m_rigidbody.linearDamping = playerStats_SO.m_fGroundDrag;
		else m_rigidbody.linearDamping = playerStats_SO.m_fAirDrag;
	}
}