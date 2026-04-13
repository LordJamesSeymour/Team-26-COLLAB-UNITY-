using System;
using UnityEngine;

public class FanForceHandler : MonoBehaviour
{
    private Rigidbody m_rb;
    private void Awake()
    {
        m_rb = GetComponent<Rigidbody>();
        if (m_rb == null)
        {
            Debug.LogError("FanForceHandler requires a Rigidbody component.");
        }
    }

    public void HandleFanForce(Vector3 forcetoapply)
    {
        if (m_rb != null)
        {
            m_rb.AddForce(forcetoapply, ForceMode.Impulse);
        }
    }   
}
