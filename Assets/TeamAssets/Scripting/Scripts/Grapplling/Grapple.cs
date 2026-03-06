using System.Collections;
using UnityEngine;

public class Grapple : MonoBehaviour
{
    private InputManager m_inputManager;

    [Header("Debug")]
    [SerializeField] private bool m_drawDebugRay = true;
    /// <summary>
    /// The time the grapple debug ray will be visible for. This will do nothing if m_drawDebugRay is disabled.
    /// </summary>
    [SerializeField] private float m_debugRayDuration = 10.0f;

    [Header("Grapple properties")]
    [SerializeField] private Transform m_grappleOrigin;
    [SerializeField] private float m_grappleRange = 100.0f;
    /// <summary>
    /// Controls whether the player needs to look at grapple points to successfully grapple.
    /// </summary>
    [SerializeField] private bool m_grappleAnywhere = false;
    [SerializeField] private float m_grappleForce = 10.0f;
    [SerializeField] private float m_grappleForceDelay = 0.01f;
    /// <summary>
    /// The ammount of times the currently grappling while loop can run before exiting.
    /// </summary>
    [SerializeField] private int m_maxGrappleIterations = 1000;

    private Rigidbody m_rigidBody;
    public bool m_isGrappling = false;
    private bool m_inGrapplePoint = false;

    private void Awake()
    {
        m_inputManager = GetComponent<InputManager>();
        m_rigidBody = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        m_inputManager.m_startGrappleAction += GrappleShoot;
        m_inputManager.m_stopGrappleAction += StopGrappling;
    }

    private void StopGrappling()
    {
        StopAllCoroutines();
        m_isGrappling = false;
    }

    private void GrappleShoot()
    {
        //Raycast, and drawing debug ray
        Debug.Log("Started grapple");
        RaycastHit hit;
        Physics.Raycast(m_grappleOrigin.position, m_grappleOrigin.TransformDirection(Vector3.forward), out hit, m_grappleRange);
        if (m_drawDebugRay)
        {
            ShowGrappleRay(hit);
        }

        //checking validity of hit object for grappling
        if(hit.collider != null)
        {
            bool validgrapple = false;
            if (m_grappleAnywhere)
            {
                validgrapple = true;
            }
            else
            {
                validgrapple = hit.collider.gameObject.CompareTag("GrapplePoint");
            }

            if (validgrapple)
            {
                if (m_isGrappling) { 
                    StopGrappling();
                }
                StartCoroutine(Grappling(hit.transform));
            }
        }
    }

    private IEnumerator Grappling(Transform grappleObject)
    {
        //Safety measure to ensure the while loop canot be infinite
        int iters = 0;
        while (!m_inGrapplePoint || iters < m_maxGrappleIterations)
        {
            m_isGrappling = true;
            iters++;
            //Applying a force towards the grappleObject
            if (m_rigidBody != null)
            {
                Vector3 direction = grappleObject.position - transform.position;
                direction = direction.normalized;
                m_rigidBody.AddForce(direction * m_grappleForce, ForceMode.Impulse);
            }
            yield return new WaitForSeconds(m_grappleForceDelay);
        }
        m_isGrappling = false;
    }

   private void ShowGrappleRay(RaycastHit hit)
   {
        Debug.DrawRay(m_grappleOrigin.position, m_grappleOrigin.TransformDirection(Vector3.forward) * m_grappleRange, Color.red, m_debugRayDuration);
   }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("GrapplePoint")) {
            m_inGrapplePoint = true;
            StopGrappling();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("GrapplePoint"))
        {
            m_inGrapplePoint = false;
        }
    }
}
