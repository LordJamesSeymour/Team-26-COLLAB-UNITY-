using UnityEngine;

public class GrapplePrototype : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool m_bgrappleDebugRay = false;

    [Header("Grapple properties")]
    [SerializeField] private Transform m_grappleOrigin;

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
        //Physics.Raycast(m_grappleOrigin.position);
    }
}
