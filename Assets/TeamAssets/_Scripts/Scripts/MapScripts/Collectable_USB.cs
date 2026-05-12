using System.Collections;
using UnityEngine;

public class Collectable_USB : MonoBehaviour
{
    [SerializeField, Range(100, 200)] private float rotateSpeed = 140f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var tracker = other.GetComponent<Collectable_Tracker>();
            if (tracker != null)
            {
                tracker.CollectUSB();
            }
            
            StartCoroutine(DebounceDisable());
        }
    }

    private void FixedUpdate()
    {
        gameObject.transform.Rotate(new Vector3(0,0,1) * rotateSpeed * Time.fixedDeltaTime);
    }

    private IEnumerator DebounceDisable()
    {
        yield return new WaitForSeconds(0.1f);
        gameObject.SetActive(false);
    }
}