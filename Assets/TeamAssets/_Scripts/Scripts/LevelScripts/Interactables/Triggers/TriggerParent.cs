using UnityEngine;

public abstract class TriggerParent : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject[] m_linkedInteractables;

    /// <summary>
    /// Method to trigger the interactables linked to this trigger. Do not override this method as it will break functionality.
    /// Add code in another function and call this here to create different trigger behaviour.
    /// </summary>
    public void TriggerInteractables()
    {
        foreach (GameObject interact in m_linkedInteractables)
        {
            Interactable_Parent interactableScript = interact.GetComponent<Interactable_Parent>();
            if (interactableScript != null)
            {
                interactableScript.Interact();
            }
        }
    }
}
