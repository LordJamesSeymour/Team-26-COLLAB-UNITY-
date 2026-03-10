using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] Death m_playerDeathScript;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            m_playerDeathScript.m_respawnPoint = transform.position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
