using Group26.Player.Inputs;
using Group26.Player.Movement;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.InputSystem;

public class SlopeMomentum : MonoBehaviour
{
    private PlayerController m_movementScript;
    private float m_startSpeed;
    private int m_stopCheck;
    //private Rigidbody m_rigidbody;
    //private InputManager m_inputManager;
    //private bool m_onSlope;

    public float m_momentum;

    [SerializeField] float m_maxMomentum;
    [SerializeField] float m_minMomentum;
    [SerializeField] float m_speedIncreaseFactor;
    [SerializeField] float m_slowDownFactor;
    [SerializeField] PlayerStats_SO m_playerStats_SO;
    

    private void Awake()
    {
        //m_rigidbody = GetComponent<Rigidbody>();
        //if (!m_rigidbody)
        //    Debug.LogError("No rigidbody attached to this object");

        m_movementScript = GetComponent<PlayerController>();
        if (!m_movementScript)
            Debug.LogError("no player locomotion script attached to this object");
        else
            m_startSpeed = m_movementScript.moveSpeed;
    }

    private IEnumerator SlowDown()
    {
        switch (m_stopCheck)
        {
            case 0:
                m_movementScript.moveSpeed = m_startSpeed;
                m_momentum -= m_slowDownFactor * m_playerStats_SO.m_fGroundDrag;
                m_movementScript.moveSpeed += m_momentum;
                m_stopCheck = 1;
                break;
            default:
                m_momentum -= m_slowDownFactor * m_playerStats_SO.m_fGroundDrag;
                break;
        }

        yield return new WaitForEndOfFrame();
    }

    //Not currently using these methods but keeping them for now just in case
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
        Debug.Log(m_movementScript.moveSpeed);
        Debug.Log(m_momentum);

        if (m_movementScript.OnSlope() &&  m_momentum < m_maxMomentum && m_movementScript.GetDirection().z >= 0.95)
        {
            m_stopCheck = 0;
            m_momentum += m_speedIncreaseFactor;
        }
        else if(m_movementScript.GetDirection() == Vector3.zero)
        {
            if(m_movementScript.moveSpeed > m_startSpeed && m_momentum > m_minMomentum)
            {
                StartCoroutine(SlowDown());
            }
            else
            {
                m_momentum = 0;
            }
            //reset momentum and speed
            //m_momentum = 0.0f;
            
            //m_locomotionScript.moveSpeed = m_startSpeed;
        }
    }
}
