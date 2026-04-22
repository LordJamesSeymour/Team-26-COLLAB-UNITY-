using Unity.VisualScripting;
using UnityEngine;

public class ButtonScript : TriggerParent
{
    [Header("Parameters")]
    [SerializeField] private bool m_bCheckForPlayer = true;

    private void OnCollisionEnter(Collision collision)
    {
        if(m_bCheckForPlayer)
        {
            if(collision.transform.root.CompareTag("Player"))
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
