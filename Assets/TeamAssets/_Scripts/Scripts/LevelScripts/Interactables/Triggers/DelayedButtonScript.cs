using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class DelayedButtonScript : TriggerParent
{
    [Header("Properties")]
    [SerializeField] private float m_delayTime = 2.5f;
    [SerializeField] private bool m_bCheckForPlayer = true;

    [Header("Button debug")]
    [SerializeField] private bool m_bLogCollisionName = false;

    private void OnTriggerEnter(Collider other)
    {
        if (m_bLogCollisionName)
            Debug.Log("Collision is named: " + other.gameObject.name);

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
        TriggerInteractables();
    }

}
