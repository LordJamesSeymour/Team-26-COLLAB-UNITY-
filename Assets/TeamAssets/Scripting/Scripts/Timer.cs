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
    public int m_totalTimeSecs = 0;
    public int m_elapsedTimeSecs = 0;
    public int m_elapsedTimeMins = 0;

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
            m_totalTimeSecs++;

            if (m_timerDisplay != null && !m_bcompressTime)
            {
                m_timerDisplay.text = m_totalTimeSecs.ToString();
            }
            else if (m_timerDisplay != null && m_bcompressTime)
            {
                CompressTime();
                m_timerDisplay.text = m_elapsedTimeMins.ToString("00") + ":" + m_elapsedTimeSecs.ToString("00");
            }
        }
    }

    public void ResetTimer()
    {
        m_totalTimeSecs = 0;
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
        m_elapsedTimeSecs = m_totalTimeSecs % 60;
        m_elapsedTimeMins = m_totalTimeSecs / 60;
    }

}
