using System.Collections;
using UnityEngine;

public class Death : MonoBehaviour
{
    [SerializeField] int m_numLives;

    [HideInInspector] public Vector3 m_respawnPoint;
    //[HideInInspector] public bool m_isDead;

    private Rigidbody m_rigidbody;

    private void Awake()
    {
        m_respawnPoint = transform.position;

        m_rigidbody = GetComponent<Rigidbody>();
        if (!m_rigidbody)
        {
            Debug.Log("No rigidbody attached to this object");
        }
        //TEST_DeathZone.OnPlayerDead += PlayerDeath;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(1);
        m_rigidbody.linearVelocity = Vector3.zero;
        m_rigidbody.angularVelocity = Vector3.zero;
        switch (m_numLives)
        {
            case 0:
                
                break;
            default:
                m_numLives -= 1;
                transform.position = m_respawnPoint;
                break;
        }
        yield return new WaitForSeconds(0.5f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "death zone")
        {
            Debug.Log("Player Dead");
            StartCoroutine(Respawn());
        }
    }

    //private void PlayerDeath()
    //{
    //    StartCoroutine(Respawn());
    //}

    // Update is called once per frame
    void Update()
    {
        
    }
}
