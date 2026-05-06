using Group26.Player.Movement;
using System;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class levelcompletescript : MonoBehaviour
{
    [HideInInspector] public bool m_enabled = false;
    public int m_levelNum;

    [SerializeField] private Sprite[] m_buttonSprites;
    [SerializeField] private TextMeshProUGUI[] m_uiTexts;
    [SerializeField] private Button m_mainMenuButton;
    [SerializeField] private menuscreeneventsmanager m_menuEventsManager;
    [SerializeField] GameObject m_endOfLevelUIPanel;
    [SerializeField] Timer m_timer;
    [SerializeField] GameObject m_player;

    private Rigidbody m_playerRigidbody;
    private TrickSystem m_trickSystem;
    private Death m_playerDeathScript;
    private InputAction m_navInputs;
    private InputAction m_selectInput;
    private InputAction m_completeInput;
    private bool m_onMainMenuButton = false;
    private datamanager m_manager;

    private void Awake()
    {
        m_manager = new datamanager(6);
        try
        {
            m_manager.LoadGameData();
        }
        catch(Exception e)
        {
            Debug.Log(e.Message);
        }

        m_playerRigidbody = m_player.GetComponent<Rigidbody>();
        m_trickSystem = m_player.GetComponent<TrickSystem>();
        m_playerDeathScript = m_player.GetComponent<Death>();

        m_navInputs = InputSystem.actions.FindAction("Navigate");
        m_selectInput = InputSystem.actions.FindAction("Select");
        m_completeInput = InputSystem.actions.FindAction("Complete");
        m_menuEventsManager.IsVisible += OnVisible;
    }

    public void OnMainMenuButtonPressed(int mainMenuNum)
    {
        m_manager.SetCompleted(m_levelNum, true);
        m_manager.SaveGameData();
        SceneManager.LoadScene(mainMenuNum);
    }

    private void OnVisible()
    {
        m_playerRigidbody.linearVelocity = Vector3.zero;
        m_playerRigidbody.angularVelocity = Vector3.zero;
        m_playerRigidbody.isKinematic = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        m_uiTexts[0].text = "Game Score: " + m_trickSystem.TotalScore;
        m_uiTexts[1].text = "Completion Time: " + m_timer.m_timerDisplay.text;

        if (m_playerDeathScript.m_deathless)
            m_uiTexts[3].text = "Deathless: Yes";
        else
            m_uiTexts[3].text = "Deathless: No";
    }

    public void ToggleMenuOn()
    {
        m_endOfLevelUIPanel.SetActive(true);
        m_enabled = true;
        m_timer.m_paused = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(m_navInputs.WasPressedThisDynamicUpdate() && m_enabled)
        {
            m_mainMenuButton.image.sprite = m_buttonSprites[1];
            m_onMainMenuButton = true;
        }

        if(m_selectInput.WasPressedThisDynamicUpdate() && m_enabled && m_onMainMenuButton)
        {
            m_mainMenuButton.onClick.Invoke();
        }
        
        if(m_completeInput.WasPressedThisDynamicUpdate() && m_enabled == false)
        {
            ToggleMenuOn();
        }
    }
}
