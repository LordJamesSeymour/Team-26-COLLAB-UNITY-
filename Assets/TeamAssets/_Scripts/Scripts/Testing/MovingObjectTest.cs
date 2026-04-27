using UnityEngine;

public class MovingObjectTest : MonoBehaviour
{
    Rigidbody m_rb;
    Vector3 m_targetpos;

    private void Awake()
    {
        m_rb = GetComponent<Rigidbody>();

        m_targetpos = transform.position + new Vector3(100.0f,0.0f,0.0f); 

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (m_rb != null) {

            m_rb.Move(m_targetpos,transform.rotation);
        }
    }

}
