using System.Collections;
using UnityEngine;

public class DelayedButtonScript : TriggerParent
{
    [Header("Properties")]
    [SerializeField] private float m_delayTime = 2.5f;
    [SerializeField] private bool m_bCheckForPlayer = true;

    [Header("Debug")]
    [SerializeField] private bool m_bLogTrigger = false;

    private void OnTriggerEnter(Collider other)
    {
        if (m_bCheckForPlayer)
        {
            if(other.transform.root.CompareTag("Player"))
                StartCoroutine(TriggerAfterDelay());
        }
        else
        {
            StartCoroutine(TriggerAfterDelay());
        }
    }

    private IEnumerator TriggerAfterDelay()
    {
        yield return new WaitForSeconds(m_delayTime);
        if(m_bLogTrigger)
            Debug.Log("Delayed button " + this.name + " has triggered after a delay of " + m_delayTime + " seconds.");
        TriggerInteractables();
    }

}
