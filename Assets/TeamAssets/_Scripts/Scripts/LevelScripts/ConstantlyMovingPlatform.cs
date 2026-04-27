using System.Collections;
using UnityEngine;

public class ConstantlyMovingPlatform : MonoBehaviour
{

    [Header("Parameters")]
    [SerializeField] private float m_launchInterval = 1.0f;
    [SerializeField] private float m_moveSpeed = 50.0f;

    private Rigidbody m_rb;

    private void Awake()
    {
        m_rb = GetComponent<Rigidbody>();
        if (m_rb == null)
            Debug.LogWarning(this.name + " has no rigidbody, so will not move");
    }

    private void Start()
    {
        StartCoroutine(LaunchPlatform());
    }

    private IEnumerator LaunchPlatform()
    {
        while (true)
        {
            if (m_rb == null)
            {
                StopAllCoroutines();
            }
            else
            {
                m_rb.AddForce(transform.forward * m_moveSpeed, ForceMode.Impulse);
                Debug.Log(transform.forward * m_moveSpeed);
            }
            yield return new WaitForSeconds(m_launchInterval);
        }
    }
}
