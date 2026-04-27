using UnityEngine;

public class LevelResetVolume : MonoBehaviour
{
    [SerializeField] Transform StartPos;

    private void OnTriggerEnter(Collider other)
    {
        Transform root = other.transform.root;
        root.position = StartPos.position;

        Rigidbody rb = root.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}