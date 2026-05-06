using UnityEngine;

public class DetachEmitter : MonoBehaviour
{
    private void OnDestroy()
    {
        ReturnToPool();
    }

    private void OnDisable()
    {
        Invoke("ReturnToPool", 0.01f);
    }

    void ReturnToPool()
    {
        foreach (Transform child in transform)
        {
            if (child.GetComponent<AudioSource>())
            {
                child.SetParent(AudioManager.instance.transform);
                child.GetComponent<AudioSource>().Pause();
                // can also have the sound cut or continue or fade on end instantly from here
            }
        }
    }
}
