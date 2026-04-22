using UnityEngine;

public class LerpTesting : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] private Vector3 m_targetPosition = Vector3.zero;
    private Vector3 m_startPosition = Vector3.zero;
    [SerializeField][Range(-1.0f,1.0f)] private float m_lerpAmmount = 0.0f;
    private void Start()
    {
        m_startPosition = transform.position;
    }

    private void Update()
    {
            transform.position = Vector3.Lerp(m_startPosition, m_targetPosition, m_lerpAmmount);
    }

}
