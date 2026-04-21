using UnityEngine;

public class PrintInteractable : Interactable_Parent
{
    public override void InteractImplementation()
    {
        //this is mainly a test interactable
        Debug.Log(this.name + " was interacted with");
    }
}
