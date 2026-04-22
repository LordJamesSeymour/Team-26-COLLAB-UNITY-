using Unity.VisualScripting;
using UnityEngine;
public class ButtonScript : TriggerParent
{
    [Header("Parameters")]
    [SerializeField] private bool m_bCheckForPlayer = true;
    private void OnTriggerEnter(Collider other)
    {
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
