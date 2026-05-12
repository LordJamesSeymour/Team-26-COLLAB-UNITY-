using System.Collections;
using UnityEngine;

public class Collectable_USB : MonoBehaviour
{
    [SerializeField, Range(100, 200)] private float rotateSpeed = 140f;
    //private AudioSource pickupSound;

    // void Awake()
    // {
    //     pickupSound = GetComponent<AudioSource>();
    // }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<Collectable_Tracker>().CollectUSB();

            // if(pickupSound != null)
            // {
            //     pickupSound.Play();
            // }

            StartCoroutine(DebounceDisable());
        }
    }

    private void FixedUpdate()
    {
        gameObject.transform.Rotate(new Vector3(0,0,1) * rotateSpeed * Time.fixedDeltaTime);
    }

    private IEnumerator DebounceDisable()
    {
        yield return new WaitForSeconds(0.3f);
        gameObject.SetActive(false);
    }
}