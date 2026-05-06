using UnityEngine;

public class DetachEmitter : MonoBehaviour
{
    private void OnDestroy()
    {
        ReturnToPool();
    }

    private void OnDisable()
    {
        ReturnToPool();
    }

    void ReturnToPool()
    {
        foreach (Transform child in transform)
        {
            if (child.GetComponent<AudioSource>())
            {
                AudioManager.instance.EndSound(child.GetComponent<AudioSource>());
                // can also have the sound cut or continue or fade on end instantly from here
            }
        }
    }
}
