using UnityEngine;

public class ObjectSpinner : MonoBehaviour
{
    [SerializeField, Range(100, 200)] private float rotateSpeed = 140f;

    private void FixedUpdate()
    {
        gameObject.transform.Rotate(Vector3.up * rotateSpeed * Time.fixedDeltaTime);
    }
}
