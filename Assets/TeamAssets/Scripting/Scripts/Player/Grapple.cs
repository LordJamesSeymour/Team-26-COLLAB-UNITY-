using UnityEngine;

public class Grapple : MonoBehaviour
{
    private InputManager m_inputManager;

    [Header("Debug")]
    [SerializeField] private bool m_drawDebugRay = true;

    [Header("Grapple properties")]
    [SerializeField] private Transform m_grappleOrigin;
    [SerializeField] private float m_grappleRange = 100.0f;

    private void Awake()
    {
        m_inputManager = GetComponent<InputManager>();
    }

    private void OnEnable()
    {
        m_inputManager.m_grappleAction += GrappleShoot;
    }

    private void GrappleShoot()
    {
        RaycastHit hit;
        Physics.Raycast(m_grappleOrigin.position, transform.forward, out hit, m_grappleRange);
        if (hit.collider != null && m_drawDebugRay)
        {
            Vector3 rayduration = hit.point - m_grappleOrigin.position;
            Debug.DrawRay(m_grappleOrigin.position, transform.forward, Color.red, 50.0f);
        }
    }

}
