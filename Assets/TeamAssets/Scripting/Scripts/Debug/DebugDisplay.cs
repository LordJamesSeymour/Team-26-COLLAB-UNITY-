using TMPro;
using UnityEngine;

public class DebugDisplay : MonoBehaviour
{
    [Header("Debug Display")]
    [SerializeField] private bool m_displayDeltaTime = false;
    [SerializeField] private bool m_displaySpeed = false;
    [SerializeField] private TextMeshProUGUI m_deltaTimeDisplay = null;
    [SerializeField] private TextMeshProUGUI m_speedDisplay = null;
    /// <summary>
    /// The object that's speed will be displayed. It must have a rigidbody component.
    /// </summary>
    [SerializeField] private GameObject m_speedObject = null;
    private void FixedUpdate()
    {
        if(m_displayDeltaTime && m_deltaTimeDisplay != null)
        {
            m_deltaTimeDisplay.enabled = true;
            m_deltaTimeDisplay.text = "DT: " + Time.deltaTime.ToString();
        }else if (!m_displayDeltaTime && m_deltaTimeDisplay != null)
        {
            m_deltaTimeDisplay.enabled = false;
        }

        if(m_displaySpeed && m_speedDisplay != null)
        {
            m_speedDisplay.enabled = true;
            
            if(m_speedObject != null)
            {
                Rigidbody rb = m_speedObject.GetComponent<Rigidbody>();
                if(rb != null)
                {
                    m_speedDisplay.text = "Speed: " + rb.linearVelocity.magnitude.ToString() + "   Velocity: " + rb.linearVelocity;
                }
            }
            

        } else if(!m_displaySpeed && m_speedDisplay != null)
        {
            m_speedDisplay.enabled = false;
        }
    }
}
