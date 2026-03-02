using UnityEngine;

public class PlayerLocomotion : MonoBehaviour
{
    [SerializeField] bool m_bIsGrounded;
    [SerializeField] Transform m_tPlayerOrientation;
    [SerializeField] PlayerStats_SO playerStats_SO;

    Rigidbody m_rigidbody;

    Vector3 moveDirection;

    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody>();
        m_rigidbody.freezeRotation = true;
    }

    void PlayerInput(Vector2 InputV2)
    {
        moveDirection = m_tPlayerOrientation.forward * InputV2.y + m_tPlayerOrientation.right * InputV2.x;
    }

    void MovePlayer()
    {
        m_rigidbody.AddForce(moveDirection.normalized * playerStats_SO.m_fPlayerWalkSpeed, ForceMode.Acceleration);
    }
}
