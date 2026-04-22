using UnityEngine;

public abstract class Interactable_Parent : MonoBehaviour
{
    [Header("Parameters")]
    ///<summary>
    /// This can be used to disable interactables. This should be used in game, however can be used in editor if needed.
    /// For now, there is no in game implementation.
    /// </summary>
    public bool m_bIsActive = true;

    [Header("Debug")]
    public bool m_bLogInteractions = false;

    //Interact and interact implementation are seperate for me to enforce rules on interaction.
    //Mainly preventing interaction when the interactible is not active.
    public void Interact()
    {
        if (m_bIsActive) {
            if (m_bLogInteractions)
                Debug.Log(this.name + " has begun it's interaction");
            InteractImplementation();
        }
            
    }

    /// <summary>
    /// Not to be used directly. Call Interact() instead.
    /// </summary>
    public abstract void InteractImplementation();
}
