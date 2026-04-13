using System.Collections;
using UnityEngine;

public class FanScript : MonoBehaviour
{
    [Header("Fan parameters")]
    [SerializeField] private float m_forcedelay = 0.25f;
    [SerializeField] private float m_forceamount = 10f;

    [Header("Debug")]
    [SerializeField] private bool m_logforce = false;

    private FanForceHandler m_fanforcehandler;

    private void Awake()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        m_fanforcehandler = other.gameObject.transform.root.GetComponent<FanForceHandler>();
        if(m_fanforcehandler != null)
        {
            StartCoroutine(ApplyForce());
        }
        
    }

    private void OnTriggerExit(Collider other)
    {
        StopAllCoroutines();
    }

    private IEnumerator ApplyForce()
    {
        while (true)
        {
            Vector3 force = Vector3.up * m_forceamount;

            if (m_logforce)
            {
                Debug.Log("Applying force of " + force);
            }

            if (m_fanforcehandler != null)
            {
                m_fanforcehandler.HandleFanForce(force);
            }
            yield return new WaitForSeconds(m_forcedelay);
        }
    }
}
