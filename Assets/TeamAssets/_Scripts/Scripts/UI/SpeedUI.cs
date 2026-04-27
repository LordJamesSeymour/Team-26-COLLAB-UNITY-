using UnityEngine;
using TMPro;

public class SpeedUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI m_speedDisplay;
    [SerializeField] private GameObject m_playerObject;
    private Rigidbody m_rb;

    private void Awake()
    {
        if(m_speedDisplay == null)
        {
            m_speedDisplay = GetComponent<TextMeshProUGUI>();
            if(m_speedDisplay == null)
            {
                Debug.LogError(this.name + " cannot find a text component to display the speed");
            }
        }

        if(m_playerObject == null)
        {
            Debug.LogError("No player reference in " + this.name + ", cannot display speed");
        }
        else
        {
            m_rb = m_playerObject.GetComponent<Rigidbody>();
            if(m_rb == null)
            {
                Debug.LogError(m_playerObject.name + " does not have a rigidbody, cannot get speed");
            }
        }

    }

    private void Update()
    {
        if(m_rb != null && m_speedDisplay != null)
        {
            m_speedDisplay.text = "Speed:" + Mathf.Round(m_rb.linearVelocity.magnitude).ToString();
        }
    }
}
