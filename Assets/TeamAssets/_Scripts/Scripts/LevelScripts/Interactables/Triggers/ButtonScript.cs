using Unity.VisualScripting;
using UnityEngine;

public class ButtonScript : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private bool m_bCheckForPlayer = true;

    [Header("References")]
    [SerializeField] private GameObject[] m_linkedInteractables;
    private PrintInteractable[] m_interactables;

    private void Awake()
    {
        if (m_linkedInteractables.Length > 0)
        {
            for (int i = 0; i < m_linkedInteractables.Length; i++)
            {
                PrintInteractable elementToAdd = m_linkedInteractables[i].GetComponent<PrintInteractable>();
                if(elementToAdd != null)
                {
                    //add stuff here
                }
            }
        }
        else
        {
            Debug.LogWarning("No interactables linked to " + this.name + ". This means it will do nothing.");
        }
    }

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

    private void TriggerInteractables()
    {
        /*foreach(PrintInteractable interactable in m_interactables)
        {
            if(interactable != null)
                interactable.Interact();
        }*/
        Debug.Log(m_interactables.Length);
        for(int i = 0; i < m_interactables.Length; i++)
        {
            if(m_interactables[i] != null)
                m_interactables[i].Interact();
        }
    }

}
