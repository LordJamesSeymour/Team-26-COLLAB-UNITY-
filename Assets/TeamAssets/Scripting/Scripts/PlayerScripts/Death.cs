using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Death : MonoBehaviour
{
    [SerializeField] private GameObject m_respawnMenuPanel;

    [HideInInspector] public Vector3 m_respawnPoint;
    //[HideInInspector] public bool m_isDead;

    private Rigidbody m_rigidbody;
    private Vector3 m_startPoint;
    private bool m_buttonPressed = false;

    private InputAction m_respawnInput;
    private InputAction m_restartInput;

    private void Awake()
    {
        m_respawnPoint = transform.position;
        m_startPoint = transform.position;

        m_rigidbody = GetComponent<Rigidbody>();
        if (!m_rigidbody)
        {
            Debug.Log("No rigidbody attached to this object");
        }

        m_respawnInput = InputSystem.actions.FindAction("TEST_RESPAWN");
        m_restartInput = InputSystem.actions.FindAction("TEST_RESTART");
        //TEST_DeathZone.OnPlayerDead += PlayerDeath;
    }

    private IEnumerator Respawn()
    {
        //respawns the player at their last checkpoint
        //player points don't need to change

        yield return new WaitForSeconds(1f);

        Debug.Log("Respawning");

        transform.position = m_respawnPoint;
        m_respawnMenuPanel.SetActive(false);
        m_rigidbody.isKinematic = false;

        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator Restart()
    {
        //restarts the player from the start of the level
        //player points will need to be reset in this case
        //possibly just reload the level using SceneManager.LoadScene(numOfThisScene)

        yield return new WaitForSeconds(1f);

        Debug.Log("Restarting");

        transform.position = m_startPoint;
        m_respawnMenuPanel.SetActive(false);
        m_rigidbody.isKinematic = false;

        yield return new WaitForSeconds(0.5f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "death zone")
        {       
            Debug.Log("Player Dead");
            m_rigidbody.linearVelocity = Vector3.zero;
            m_rigidbody.angularVelocity = Vector3.zero;
            m_rigidbody.isKinematic = true;
            m_respawnMenuPanel.SetActive(true);
            //StartCoroutine(Respawn());
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(m_respawnInput.WasReleasedThisDynamicUpdate() && m_respawnMenuPanel.activeSelf)
        {
            StartCoroutine(Respawn());
        }

        if(m_restartInput.WasReleasedThisDynamicUpdate() && m_respawnMenuPanel.activeSelf)
        {
            StartCoroutine(Restart());
        }
    }
}
