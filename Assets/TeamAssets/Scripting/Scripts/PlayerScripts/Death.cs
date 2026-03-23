using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Death : MonoBehaviour
{
    //[SerializeField] private GameObject m_respawnMenuPanel;
    [SerializeField] private Timer m_timerScript;

    [HideInInspector] public Vector3 m_respawnPoint;
    //[HideInInspector] public bool m_isDead;

    private Rigidbody m_rigidbody;
    private Vector3 m_startPoint;
    private GameObject[] m_checkpoints;
    private int m_totalTime;
    //private bool m_buttonPressed = false;

    //private InputAction m_respawnInput;
    //private InputAction m_restartInput;

    private Coroutine m_respawn;
    private Coroutine m_restart;

    private void Awake()
    {
        m_respawnPoint = transform.position;
        m_startPoint = transform.position;

        m_rigidbody = GetComponent<Rigidbody>();
        if (!m_rigidbody)
        {
            Debug.Log("No rigidbody attached to this object");
        }

        //m_respawnInput = InputSystem.actions.FindAction("TEST_RESPAWN");
        //m_restartInput = InputSystem.actions.FindAction("TEST_RESTART");

        m_checkpoints = GameObject.FindGameObjectsWithTag("checkpoint");
    }

    private IEnumerator Respawn()
    {
        //respawns the player at their last checkpoint
        //player points don't need to change

        yield return new WaitForSeconds(0.5f);

        Debug.Log("Respawning");

        transform.position = m_respawnPoint;
        //m_respawnMenuPanel.SetActive(false);
        m_rigidbody.isKinematic = false;

        //if(m_respawnMenuPanel.activeSelf == false)
        //{
        //    m_timerScript.m_timerDisplay.gameObject.SetActive(true);
        //}

        yield return new WaitForSeconds(0.5f);
        m_respawn = null;

        //yield return new WaitUntil(() => m_respawnMenuPanel.activeSelf == false);

        //Time.timeScale = 1.0f;
        //m_timerScript.ResumeTimer();
        //m_timerScript.m_timerDisplay.gameObject.SetActive(true);
        m_timerScript.m_paused = false;
    }

    private IEnumerator Restart()
    {
        //restarts the player from the start of the level
        //player points will need to be reset in this case
        //possibly just reload the level using SceneManager.LoadScene(numOfThisScene)

        yield return new WaitForSeconds(1f);

        Debug.Log("Restarting");

        transform.position = m_startPoint;
        m_respawnPoint = m_startPoint;
        m_timerScript.ResetTimer();
        m_timerScript.UpdateTimerText("00:00");

        Debug.Log(m_checkpoints.Length);

        //NOTE: since the decision to remove the choice to respawn or restart, this may not be needed but is being kept here 
        //      for now in case the way changing settings mid run requires it. If it doesn't this can be removed
        if(m_checkpoints != null)
        {
            foreach(GameObject checkpoint in m_checkpoints)
            {
                checkpoint.GetComponent<Checkpoint>().m_used = false;
            }
        }

        //m_respawnMenuPanel.SetActive(false);
        m_rigidbody.isKinematic = false;

        yield return new WaitForSeconds(0.5f);
        m_restart = null;

        //yield return new WaitUntil(() => m_respawnMenuPanel.activeSelf == false);
        //m_timerScript.ResumeTimer();
        //Time.timeScale = 1.0f;
        //m_timerScript.ResetTimer();
        //m_timerScript.m_timerDisplay.gameObject.SetActive(true);
        m_timerScript.m_paused = false;
        //yield return new WaitForSeconds(0.5f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "death zone")
        {       
            Debug.Log("Player Dead");
            m_rigidbody.linearVelocity = Vector3.zero;
            m_rigidbody.angularVelocity = Vector3.zero;
            m_rigidbody.isKinematic = true;
            //m_respawnMenuPanel.SetActive(true);
            //m_timerScript.m_timerDisplay.gameObject.SetActive(false);
            m_timerScript.m_paused = true;

            if (Checkpoint.m_checkpointsEnabled)
            {
                StopCoroutine(Restart());
                m_restart = null;
                m_respawn = StartCoroutine(Respawn());
            }
            else
            {
                StopCoroutine(Respawn());
                m_respawn = null;
                m_restart = StartCoroutine(Restart());
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        //if(m_respawnInput.WasReleasedThisDynamicUpdate() && m_respawnMenuPanel.activeSelf && m_respawn == null)
        //{
        //    StopCoroutine(Restart());
        //    m_restart = null;
        //    m_respawn = StartCoroutine(Respawn());
        //}

        //if(m_restartInput.WasReleasedThisDynamicUpdate() && m_respawnMenuPanel.activeSelf && m_restart == null)
        //{
        //    StopCoroutine(Respawn());
        //    m_respawn = null;
        //    m_restart = StartCoroutine(Restart());
        //}
    }
}
