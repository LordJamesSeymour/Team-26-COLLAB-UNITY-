using UnityEngine;

public class GrapplePointCode : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] bool m_doBoost = true;
    [SerializeField] float m_boostAmmount = 50.0f;
    /// <summary>
    /// Whether a boost will be given regardless of whether the player is grappling or not.
    /// </summary>
    [SerializeField] bool m_requireGrappleForBoost = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && m_doBoost)
        {
            //checking whether a boost can occur
            bool canboost = false;

            if (m_requireGrappleForBoost)
            {
                Grapple grappleScript = other.GetComponentInParent<Grapple>();
                if (grappleScript != null)
                {
                    canboost = grappleScript.m_isGrappling;
                }
            }
            else
            {
                canboost = true;
            }

            //performing the boost
            if(other.attachedRigidbody != null && canboost)
            {
                Debug.Log("Applying boost");
                other.attachedRigidbody.AddForce(other.transform.TransformDirection(Vector3.forward) * m_boostAmmount,ForceMode.Impulse);
            }
        }
    }
}
