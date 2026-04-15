using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class FanScript : MonoBehaviour
{
    [Header("Fan parameters")]
    [SerializeField] private float m_forceDelay = 0.1f;
    /// <summary>
    /// The ammount of upwards force added, this does not need to be a Vector3 as only one axis is considered here
    /// </summary>
    [SerializeField] private float m_forceAmmount = 10f;
    private Vector3 m_forceToAdd = Vector3.zero;

    [Header("Debug")]
    /// <summary>
    /// Debug variable that toggles printing the force added when the fan adds force
    /// </summary>
    [SerializeField] private bool m_bLogForce = false;

    private FanForceHandler m_fanForceHandler;

    private void Start()
    {
        //calculates the force to add. This is done on start as m_forceammount will not change as runtime. This may change however if we want rotating fans
        m_forceToAdd = transform.up * m_forceAmmount;
    }
    private void OnTriggerEnter(Collider other)
    {
        //gets the FanForceHandler script from the root of the collided gameobject, and begins adding force if it exists
        m_fanForceHandler = other.gameObject.transform.root.GetComponent<FanForceHandler>();
        if (m_fanForceHandler != null)
        {
           StartCoroutine(ApplyForce());
        }
        
    }

    private void OnTriggerExit(Collider other)
    { 
        //Stops the fan from adding force
        //This may need to be improved to check for multiple targets, but I believe the player will be the only object affected by the fan
        StopAllCoroutines();
    }

    private IEnumerator ApplyForce()
    {
        //loops infinitely until stopped through StopAllCoroutines
        while (true)
        {
            if (m_bLogForce)
            {
                Debug.Log("Applying force of " + m_forceToAdd);
            }

            if (m_fanForceHandler != null)
            {
                m_fanForceHandler.HandleFanForce(m_forceToAdd);
            }
            yield return new WaitForSeconds(m_forceDelay);
        }
    }
}
