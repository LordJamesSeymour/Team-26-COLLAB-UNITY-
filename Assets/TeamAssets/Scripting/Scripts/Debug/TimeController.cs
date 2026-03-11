using UnityEngine;

public class TimeController : MonoBehaviour
{
    [Header("Time data")]
    [SerializeField,Range(0.0f,10.0f)] private float m_currentTimeScale = 1f;
    private void Update()
    {
        Time.timeScale = m_currentTimeScale;
    }
}
