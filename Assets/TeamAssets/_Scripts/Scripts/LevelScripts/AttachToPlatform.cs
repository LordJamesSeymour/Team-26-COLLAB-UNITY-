using System.Collections;
using UnityEngine;

public class AttachToPlatform : MonoBehaviour
{
    [Header("Parameters")]
    /// <summary>
    /// The time between adding the platform force to the player
    /// </summary>
    [SerializeField] private float m_matchforcetime = 0.1f;
    [SerializeField] private float m_speedMultiplier = 1.25f;

    [Header("References")]
    [SerializeField] private LayerMask m_platformLayer;
    private Rigidbody m_platformRb;
    private Rigidbody m_rb;

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
                StartCoroutine(MatchPlatformForce());
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
            StopAllCoroutines();
        }
    }

    private IEnumerator MatchPlatformForce()
    {
        while (true)
        {
            //Stopping the coroutine if the platform or object has no rigidbody
            if (m_platformRb == null || m_rb == null)
                StopAllCoroutines();

            Vector3 forceToAdd = m_platformRb.linearVelocity * m_speedMultiplier;
            m_rb.AddForce(forceToAdd,ForceMode.Impulse);

            yield return new WaitForSeconds(m_matchforcetime);
        }
    }

}
