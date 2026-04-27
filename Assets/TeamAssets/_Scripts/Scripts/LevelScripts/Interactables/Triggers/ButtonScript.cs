using Unity.VisualScripting;
using UnityEngine;
public class ButtonScript : TriggerParent
{
    [Header("Parameters")]
    [SerializeField] private bool m_bCheckForPlayer = true;

    [Header("Button debug")]
    [SerializeField] private bool m_bLogCollisionName = false;
    private void OnTriggerEnter(Collider other)
    {
        if (m_bLogCollisionName)
            Debug.Log("Collision is named: " + other.gameObject.name);

        if (m_bCheckForPlayer)
        {
            if (other.transform.root.CompareTag("Player"))
            {
                TriggerInteractables();
            }
        }
        else
        {
            TriggerInteractables();
        }
    }

}
