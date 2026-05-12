using System.Collections;
using UnityEngine;

public class Collectable_USB : MonoBehaviour
{
    [SerializeField, Range(100, 200)] private float rotateSpeed = 140f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<Collectable_Tracker>().CollectUSB();
            StartCoroutine(DebounceDisable());
        }
    }

    private void FixedUpdate()
    {
        gameObject.transform.Rotate(new Vector3(0,0,1) * rotateSpeed * Time.fixedDeltaTime);
    }

    private IEnumerator DebounceDisable()
    {
        yield return new WaitForSeconds(0.025f);
        gameObject.SetActive(false);
    }
}