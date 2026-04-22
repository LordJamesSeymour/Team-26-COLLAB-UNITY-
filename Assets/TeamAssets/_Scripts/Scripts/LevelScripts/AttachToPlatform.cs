using UnityEngine;

public class AttachToPlatform : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LayerMask m_platformLayer;
    private Rigidbody m_rb;

    private void OnTriggerEnter(Collider other)
    {
        if ((1 << other.gameObject.layer) == m_platformLayer.value)
        {
            Debug.Log("Touching a platform");
            if(other.transform.root.GetComponent<Rigidbody>() != null)
                m_rb = other.transform.root.GetComponent<Rigidbody>();
            else
                Debug.LogWarning("The platform " + other.transform.root.name + " does not have a rigidbody, so the player cannot be attached to it.");

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if ((1 << other.gameObject.layer) == m_platformLayer.value)
        {
            Debug.Log("Stopped touching a platform");
            m_rb = null;
        }
    }

    private void Update()
    {
        if(m_rb != null)
        {
            Debug.Log("Speed: " + m_rb.linearVelocity.magnitude);
        }
    }

}
