using System.Collections;
using UnityEngine;

namespace Group26.Utils
{
    public class Collectable_USB : MonoBehaviour
    {
        [SerializeField, Range(100, 200)] private float rotateSpeed = 50f;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                //Collectable_Tracker.m_pickupsCollected += 1;

                StartCoroutine(DebounceDisable());
            }
        }

        private void Update()
        {
           gameObject.transform.Rotate(new Vector3(0,0,1) * rotateSpeed * Time.deltaTime);
        }

        private IEnumerator DebounceDisable()
        {
            yield return new WaitForSeconds(0.05f);
            gameObject.SetActive(false);
        }
    }
}