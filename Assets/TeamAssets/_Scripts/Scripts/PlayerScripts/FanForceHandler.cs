using System;
using UnityEngine;

public class FanForceHandler : MonoBehaviour
{
    private Rigidbody m_rb;

    //These are variables to be set in engine, so should not be changed at runtime
    [Header("Force limitations")]
    /// <summary>
    /// Toggles limiting force. Setting this to true will make m_maxupwardsforce do nothing
    /// </summary>
    [SerializeField] private bool m_bLimitForce = true;
    /// <summary>
    /// The maximum force that can be added by a fan. This is a float as fans only add upwards force
    /// </summary>
    [SerializeField] private float m_maxUpwardsForce = 25.0f;

    [Header("In fan check paramters")]
    [SerializeField] private LayerMask m_fanLayer;
    [SerializeField] private float m_fanCheckRadius = 2.5f;

    [Header("Debug")]
    ///<summary>
    ///Testing variable that prints the upwards speed of the object when handling fan force
    ///</summary>
    [SerializeField] private bool m_bLogUpwardsSpeed = false;
    [SerializeField] private bool m_bDrawInFanCheck = false;
    [SerializeField] private Color m_drawColor = Color.red;

    private void Awake()
    {
        m_rb = GetComponent<Rigidbody>();
        if (m_rb == null)
        {
            Debug.LogError("FanForceHandler requires a Rigidbody component");
        }
    }

    public void HandleFanForce(Vector3 forcetoapply)
    {
        if (m_rb != null)
        {

            if (m_bLogUpwardsSpeed)
            {
                Debug.Log(this.name + " is moving at a speed of: " + m_rb.linearVelocity.magnitude + ". The max speed is: " + m_maxUpwardsForce);
            }

            //if the force is not being limited, the force is added and the function returns / exits early
            if (!m_bLimitForce)
            {
                m_rb.AddForce(forcetoapply, ForceMode.Impulse);
                return;
            }

            //if the force is being limited, the force is only added if the linear velocity is less than the max force
            if (m_rb.linearVelocity.magnitude < m_maxUpwardsForce)
            {
                m_rb.AddForce(forcetoapply, ForceMode.Impulse);
            }

        }
        else
        {
            Debug.LogError(this.name + " does not have an attached rigidbody, so the fan will not affect it");
        }
    }

    public bool IsInFan()
    {
        return Physics.CheckSphere(transform.position, m_fanCheckRadius, m_fanLayer);
    }
    private void OnDrawGizmos()
    {
        if (m_bDrawInFanCheck)
            Gizmos.color = m_drawColor;
            Gizmos.DrawWireSphere(transform.position, m_fanCheckRadius);
    }
}
