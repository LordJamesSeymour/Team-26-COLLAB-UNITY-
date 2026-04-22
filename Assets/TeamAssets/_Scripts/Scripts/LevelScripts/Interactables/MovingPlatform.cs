using System.Collections;
using UnityEditor.Rendering;
using UnityEngine;

public class MovingPlatform : Interactable_Parent
{
    [Header("Properties")]
    /// <summary>
    /// The time between when the move code is ran. This will affect the speed and smoothness of the platform movement.
    /// This value should only be changed to increase the smoothness of movement. m_lerpAmmountPerIter should be used for this instead.
    /// </summary>
    [SerializeField] private float m_moveIterDelay = 0.01f;
    /// <summary>
    /// The ammount that the platform will be moved towards the target position after the interval defined by m_moveIterDelay.
    /// This will affect the speed of the movement, but not the smoothness.
    /// This should be the primary way of manipulating the speed of the platform.
    /// </summary>
    [SerializeField] private float m_lerpAmmountPerIter = 0.1f;
    [SerializeField] private Vector3 m_targetPosition = Vector3.zero;
    private Vector3 m_startPosition = Vector3.zero;
    /// <summary>
    /// Controls whether the platform will move back to the start position when being toggled again.
    /// </summary>
    [SerializeField] private bool m_bToggleDirectionOfMovement = true;

    [Header("Debug")]
    [SerializeField] private bool m_bLogEndOfMove = false;
    [SerializeField] private bool m_bLogIterations = false;
 
    private bool m_bMovingToTarget = true;

    private void Start()
    {
        m_startPosition = transform.position;
    }
    
    public override void InteractImplementation()
    {
        StartCoroutine(MovePlatform());
    }

    private IEnumerator MovePlatform()
    {

        float lerpAmmount = 0;
        while (true)
        {
            lerpAmmount = Mathf.Abs(lerpAmmount);
            lerpAmmount = lerpAmmount + m_lerpAmmountPerIter;
            
            if(!m_bMovingToTarget)
                lerpAmmount = lerpAmmount * -1.0f;

            if (m_bLogIterations)
                Debug.Log(this.name + " is currently moving. Lerp ammount: " + lerpAmmount);
            
            if (lerpAmmount > 1)
            {
                if(m_bLogEndOfMove)
                    Debug.Log(this.name + " has finished it's movement");

                if(m_bToggleDirectionOfMovement)
                    m_bMovingToTarget = !m_bMovingToTarget;

                transform.position = m_targetPosition;
                StopAllCoroutines();
                break;
            }
            transform.position = Vector3.Lerp(m_startPosition,m_targetPosition,lerpAmmount);
            yield return new WaitForSeconds(m_moveIterDelay);
        }
    }

}
