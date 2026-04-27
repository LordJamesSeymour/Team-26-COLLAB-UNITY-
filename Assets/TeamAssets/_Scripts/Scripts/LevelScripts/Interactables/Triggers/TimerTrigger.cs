using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerTrigger : TriggerParent
{
    [Header("Properties")]
    [SerializeField] private float m_timerInterval = 1.0f;

    public void StartTimer()
    {
        StartCoroutine(Timer());
    }

    public void StopTimer()
    {
        StopAllCoroutines();
    }

    private IEnumerator Timer()
    {
        while (true)
        {
            yield return new WaitForSeconds(m_timerInterval);
            TriggerInteractables();
        }
    }

}
