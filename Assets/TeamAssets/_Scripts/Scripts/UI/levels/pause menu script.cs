using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class pausemenuscript : buttonnavscript
{
    [SerializeField] private GameObject m_darkenedBackground;
    [SerializeField] GameObject m_player;
    [SerializeField] private Timer m_timer;

    private Coroutine m_toggleMenuOff;
    private Coroutine m_toggleMenuOn;
    private Coroutine m_restart;
    private InputAction m_pauseInput;
    private Death m_playerDeathScript;
    private Rigidbody m_playerRigidbody;
    private Transform m_playerTransform;
    private GameObject[] m_checkpoints;

    void Awake()
    {
        m_navInputs = InputSystem.actions.FindAction("Navigate");
        m_selectInput = InputSystem.actions.FindAction("Select");
        m_pauseInput = InputSystem.actions.FindAction("Pause");

        m_controlsScreenScript = GetComponent<controlsmenuscript>();
        if (!m_controlsScreenScript)
            Debug.LogError("no controls screen script attached");

        m_settingsScreenScript = GetComponent<settingsmenuscript>();
        if (!m_settingsScreenScript)
            Debug.LogError("no settings screen script attached");

        m_currentButton = m_buttons[m_index];
        m_currentButton.image.sprite = m_buttonSprites[1];
        m_enabled = false;
        m_playerDeathScript = m_player.GetComponent<Death>();
        m_playerTransform = m_player.transform;
        m_playerRigidbody = m_player.GetComponent<Rigidbody>();
        m_checkpoints = GameObject.FindGameObjectsWithTag("checkpoint");
    }

    private IEnumerator TogglePauseMenuOff()
    {
        m_darkenedBackground.SetActive(false);
        m_menuPanel.SetActive(false);
        m_currentButton.image.sprite = m_buttonSprites[0];
        m_index = 0;
        m_currentButton = m_buttons[m_index];
        m_currentButton.image.sprite = m_buttonSprites[1];
        m_enabled = false;
        yield return new WaitUntil(() => m_menuPanel.activeSelf == false);
    }

    private IEnumerator Pause()
    {
        m_darkenedBackground.SetActive(true);
        m_menuPanel.SetActive(true);
        m_enabled = true;
        m_timer.m_paused = true;
        yield return new WaitUntil(() => m_menuPanel.activeSelf == true);
    }

    private IEnumerator RestartLevel()
    {
        m_playerTransform.position = m_playerDeathScript.m_startPoint;
        m_playerDeathScript.m_respawnPoint = m_playerDeathScript.m_startPoint;
        m_timer.ResetTimer();

        if (m_checkpoints != null && Checkpoint.m_checkpointsEnabled)
        {
            foreach (GameObject checkpoint in m_checkpoints)
            {
                checkpoint.GetComponent<Checkpoint>().m_used = false;
            }
        }

        m_playerRigidbody.isKinematic = false;

        yield return new WaitForSeconds(0.9f);
        if (m_toggleMenuOff == null)
            m_toggleMenuOff = StartCoroutine(TogglePauseMenuOff());

        m_toggleMenuOff = null;
        StopCoroutine(TogglePauseMenuOff());
        yield return new WaitForSeconds(0.3f);
        m_timer.UpdateTimerText("00:00");
        m_timer.m_paused = false;
    }

    public void Resume()
    {
        if(m_toggleMenuOff == null) 
            m_toggleMenuOff = StartCoroutine(TogglePauseMenuOff());

        m_toggleMenuOff = null;
        StopCoroutine(TogglePauseMenuOff());
        m_timer.m_paused = false;
    }

    public void Restart()
    {
        m_playerRigidbody.linearVelocity = Vector3.zero;
        m_playerRigidbody.angularVelocity = Vector3.zero;
        m_playerRigidbody.isKinematic = true;
        m_timer.m_paused = true;
        if (m_restart == null)
            m_restart = StartCoroutine(RestartLevel());

        m_restart = null;
        StopCoroutine(RestartLevel());
        Debug.Log(m_toggleMenuOff == null);
        //m_playerDeathScript.RestartLevel();
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(m_enabled);
        if(m_enabled == false && m_pauseInput.WasReleasedThisDynamicUpdate())
        {
            Debug.Log(m_toggleMenuOn == null);
            if(m_toggleMenuOn == null)
                m_toggleMenuOn = StartCoroutine(Pause());

            m_toggleMenuOn = null;
            StopCoroutine(Pause());
            Debug.Log("pausing");
        }

        if (m_navInputs.WasPressedThisDynamicUpdate() && m_enabled)
        {
            Vector2 direction = m_navInputs.ReadValue<Vector2>();
            //Debug.Log(direction);

            if (direction == Vector2.up && m_index > 0)
            {
                m_index--;
            }
            else if (direction == Vector2.down && m_index < m_buttons.Length - 1)
            {
                m_index++;
            }
            //Debug.Log("Index: " + m_index);
        }

        if (m_navInputs.WasReleasedThisDynamicUpdate() && m_enabled)
        {
            if (m_currentButton != null)
            {
                m_currentButton.image.sprite = m_buttonSprites[0];
                m_currentButton = m_buttons[m_index];
                m_currentButton.image.sprite = m_buttonSprites[1];
            }
        }

        if (m_selectInput.WasReleasedThisDynamicUpdate() && m_currentButton != null && m_enabled)
        {
            m_currentButton.onClick.Invoke();
        }
    }
}
