using System.Collections;
using UnityEngine;

public class AttachToPlatform : MonoBehaviour
{  
    [Header("References")]
    [SerializeField] private LayerMask m_platformLayer;
    private Rigidbody m_platformRb = null;
    private Rigidbody m_rb = null;
    private MovingPlatform m_movingPlatformScript = null;
    [SerializeField] private float m_forceMultiplier = 2.0f;
    /// <summary>
    /// The force that will be added when an attached platform moves. 
    /// This will be a zero vector if the player is not attached to a platform.
    /// </summary>
    private Vector3 m_forceToAdd = Vector3.zero;
    private void Awake()
    {
        m_rb= GetComponent<Rigidbody>();
        if (m_rb == null)
            Debug.Log(this.name + "does not have an attached rigidbody, so will not move along with the platform");
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((1 << other.gameObject.layer) == m_platformLayer.value)
        {
            Debug.Log("Touching a platform");
            if (other.transform.root.GetComponent<Rigidbody>() != null)
            {
                m_platformRb = other.transform.root.GetComponent<Rigidbody>();

                m_movingPlatformScript = other.transform.root.GetComponent<MovingPlatform>();
                m_movingPlatformScript.m_movingPlatformTick += MatchPlatformForce;
            }
            else
                Debug.LogWarning("The platform " + other.transform.root.name + " does not have a rigidbody, so the player cannot be attached to it.");

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if ((1 << other.gameObject.layer) == m_platformLayer.value)
        {
            Debug.Log("Stopped touching a platform");
            m_platformRb = null;

            if(m_movingPlatformScript != null)
            {
                m_movingPlatformScript.m_movingPlatformTick -= MatchPlatformForce;
                m_movingPlatformScript = null;
            }
        }
    }

    private void MatchPlatformForce()
    {
        if(m_platformRb == null || m_rb == null)
        {
            Debug.LogWarning("Cannot match platform force because either the player or the platform does not have a rigidbody");
            return;
        }
        
        m_rb.AddForce(m_platformRb.linearVelocity, ForceMode.Impulse);
        
    }

}
