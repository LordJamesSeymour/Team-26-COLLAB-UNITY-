using UnityEngine;

public class TimerManager : Interactable_Parent
{
    [Header("References")]
    [SerializeField] private GameObject m_linkedTimer;
    private TimerTrigger m_timerTrigger;

    [Header("Properties")]
    /// <summary>
    /// Sets whether multiple interactions with the timer manager toggles between an active and inactive timer
    /// </summary>
    [SerializeField] private bool m_bToggleTimer = true;
    [SerializeField] private bool m_bState = false;
    [SerializeField] private bool m_bAutoStart = false;

    private void Awake()
    {
        if(m_linkedTimer == null)
        {
            Debug.LogWarning("No timer linked to " + this.name);
        }
        else
        {
            m_timerTrigger = m_linkedTimer.GetComponent<TimerTrigger>();
            if (m_timerTrigger == null)
                Debug.LogWarning(m_linkedTimer.name + " does not have an attached TimerTrigger script");
        }
    }

    private void Start()
    {
        if (m_bAutoStart)
            InteractImplementation();
    }

    public override void InteractImplementation()
    {
        if (m_timerTrigger != null) {

            if (m_bState)
            { 
               m_timerTrigger.StopTimer();
            }
            else
            {
               m_timerTrigger.StartTimer();
            }
            
            if(m_bToggleTimer)
                m_bState = !m_bState;

        }
    }
}
