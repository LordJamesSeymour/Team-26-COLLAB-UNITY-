using System.Collections;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] private bool m_bautoStart = true;
    [SerializeField] private TextMeshProUGUI m_timerDisplay;
    [SerializeField] private bool m_bcompressTime = true;

    [Header("Timer Info")]
    public int m_elapsedTimeSecs = 0;
    /// <summary>
    /// The elapsed time compressed into minutes or hours
    /// </summary>
    public float m_elapsedTimeCompressed = 0;

    private void Start()
    {
        if(m_bautoStart)
        {
            StartCoroutine(StartTimer());
        }
    }

    public IEnumerator StartTimer()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            m_elapsedTimeSecs++;

            if (m_timerDisplay != null && !m_bcompressTime)
            {
                m_timerDisplay.text = m_elapsedTimeSecs.ToString();
            }
            else if (m_timerDisplay != null && m_bcompressTime)
            {
                CompressTime();
                m_timerDisplay.text = m_elapsedTimeCompressed.ToString();
            }
        }
    }

    public void ResetTimer()
    {
        m_elapsedTimeSecs = 0;
    }

    public void ResumeTimer()
    {
        StartCoroutine(StartTimer());
    }

    public void StopTimer()
    {
        StopCoroutine(StartTimer());
    }

    private void CompressTime()
    {
        int elapsedmins = 0;

        if (m_elapsedTimeSecs > 60)
        {
            elapsedmins = m_elapsedTimeSecs / 60;
        }

        if(elapsedmins > 60)
        {
            m_elapsedTimeCompressed = elapsedmins / 60;
        }
        else if (elapsedmins > 0 && elapsedmins < 60)
        {
            m_elapsedTimeCompressed = elapsedmins;
        }
        else
        {
            m_elapsedTimeCompressed = m_elapsedTimeSecs;
        }
        m_elapsedTimeCompressed = Mathf.Floor(m_elapsedTimeCompressed * 100) / 100;
    }

}
