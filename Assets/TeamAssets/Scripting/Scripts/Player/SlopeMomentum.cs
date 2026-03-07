using Group26.Player.Locomotion;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

public class SlopeMomentum : MonoBehaviour
{
    private PlayerLocomotion m_locomotionScript;
    private float m_startSpeed;
    //private bool m_onSlope;

    public float m_momentum;

    [SerializeField] float m_maxMomentum;
    [SerializeField] float m_speedIncreaseFactor;

    private void Awake()
    {
        m_locomotionScript = GetComponent<PlayerLocomotion>();
        if (!m_locomotionScript)
            Debug.LogError("no player locomotion script attached to this object");
        else
            m_startSpeed = m_locomotionScript.moveSpeed;
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    if (collision.gameObject.tag == "slope")
    //    {
    //        m_onSlope = true;
    //    }
    //}

    //private void OnCollisionExit(Collision collision)
    //{
    //    if (collision.gameObject.tag == "slope")
    //    {
    //        m_onSlope = false;
    //    }
    //}

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(m_onSlope);
        //Debug.Log(m_locomotionScript.GetDirection());
        //Debug.Log(m_locomotionScript.moveSpeed);
        //Debug.Log(m_momentum);
        if(m_locomotionScript.OnSlope() &&  m_momentum < m_maxMomentum && m_locomotionScript.GetDirection().z >= 0.95)
        {
            m_momentum += m_speedIncreaseFactor;
        }
        else if(m_locomotionScript.GetDirection() == Vector3.zero)
        {
            m_momentum = 0.0f;
            m_locomotionScript.moveSpeed = m_startSpeed;
        }
    }
}
