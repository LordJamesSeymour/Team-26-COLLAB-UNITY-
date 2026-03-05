using UnityEngine;

public class GrapplePrototype : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool m_bgrappleDebugRay = false;

    [Header("Grapple properties")]
    [SerializeField] private Transform m_grappleOrigin;
    [SerializeField] private float m_grappleRange = 50.0f;

    private InputManager m_inputManager;

    private void Awake()
    {
        m_inputManager = GetComponent<InputManager>();
    }

    private void OnEnable()
    {
        m_inputManager.GrappleAction += GrappleShoot;
    }

    private void GrappleShoot()
    {
        Debug.Log("Started grappling");
        RaycastHit hit;
        Physics.Raycast(m_grappleOrigin.position,transform.forward,out hit,m_grappleRange);
        Vector3 rayduration = hit.point - m_grappleOrigin.position;
        Debug.DrawRay(m_grappleOrigin.position, transform.forward, Color.red,rayduration.magnitude); 

    }
}
