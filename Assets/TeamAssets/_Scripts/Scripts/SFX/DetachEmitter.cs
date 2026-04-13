using UnityEngine;

public class DetachEmitter : MonoBehaviour
{
    private void OnDestroy()
    {
        foreach (Transform child in transform)
        {
            if (child.GetComponent<AudioSource>())
            {
                child.SetParent(null);
                // can also have the sound cut or continue or fade on end instantly from here
            }
        }
    }
}
