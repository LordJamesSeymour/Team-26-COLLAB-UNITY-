using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class buttonnavscript : MonoBehaviour
{
    protected InputAction m_navInputs;
    protected InputAction m_selectInput;
    protected int m_index = 0;
    protected Button m_currentButton;
    protected controlsmenuscript m_controlsScreenScript;
    private levelselectionmenuscript m_levelScreenScript;
    protected settingsmenuscript m_settingsScreenScript;
    [HideInInspector] public bool m_enabled = true;
    //private Vector2 direction;

    [SerializeField] public GameObject m_menuPanel;
    [SerializeField] public GameObject m_controlsPanel;
    [SerializeField] public GameObject m_controlsBackground;
    [SerializeField] private GameObject m_levelsPanel;
    [SerializeField] private GameObject m_levelsBackground;
    [SerializeField] public GameObject m_settingsPanel;
    [SerializeField] public GameObject m_settingsBackground;
    [SerializeField] protected Button[] m_buttons;
    [SerializeField] protected Sprite[] m_buttonSprites;

    protected Coroutine m_toggleControlsOn;
    private Coroutine m_toggleLevelsOn;
    protected Coroutine m_toggleSettingsOn;

    //public bool GetMainMenuEnabled() {  return m_mainMenuPanelEnabled; }
    //public void SetMainMenuEnabled(bool enabled) { m_mainMenuPanelEnabled = enabled; }
    //public GameObject GetMainMenuPanel() { return m_mainMenuPanel; }
    public GameObject GetLevelsPanel() { return m_levelsPanel; }

    public GameObject GetLevelsBackground() { return m_levelsBackground; }
    
    private void Awake()
    {
        m_navInputs = InputSystem.actions.FindAction("Navigate");
        m_selectInput = InputSystem.actions.FindAction("Select");

        m_controlsScreenScript = GetComponent<controlsmenuscript>();
        if (!m_controlsScreenScript)
            Debug.LogError("no controls screen script attached");

        m_levelScreenScript = GetComponent<levelselectionmenuscript>();
        //if ((!m_levelScreenScript))
        //    Debug.LogError("no level select screen script attached");

        m_settingsScreenScript = GetComponent<settingsmenuscript>();
        if (!m_settingsScreenScript)
            Debug.LogError("no settings screen script attached");

        m_currentButton = m_buttons[m_index];
        m_currentButton.image.sprite = m_buttonSprites[1];

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    //function for when the mouse hovers over a button
    public void OnPointerEnter(int i)
    {
        m_currentButton.image.sprite = m_buttonSprites[0];

        if (i > 0 && i < m_buttons.Length)
        {
            m_index = i;
        }
        else
        {
            m_index = 0;
        }

        m_currentButton = m_buttons[m_index];
    }

    //function for when the mosue stops hovering over a button
    public void OnPointerExit()
    {
        m_currentButton.image.sprite = m_buttonSprites[1];
    }

    protected IEnumerator ToggleControlsMenuOn(GameObject menu)
    {
        menu.SetActive(true);
        m_menuPanel.SetActive(false);
        m_enabled = false;
        yield return new WaitUntil(() => menu.activeSelf == true && m_menuPanel.activeSelf == false);
        m_controlsScreenScript.m_enabled = true;
    }

    private IEnumerator ToggleLevelMenuOn(GameObject menu)
    {
        if(m_levelsBackground != null)
            m_levelsBackground.SetActive(true);
        menu.SetActive(true);
        m_menuPanel.SetActive(false);
        m_enabled = false;

        if(menuscreenscript.m_run == false)
            menuscreenscript.m_run = true;

        yield return new WaitUntil(() => menu.activeSelf == true && m_menuPanel.activeSelf == false);
        m_levelScreenScript.m_enabled = true;
    }

    protected IEnumerator ToggleSettingsMenuOn(GameObject menu)
    {
        if(m_settingsBackground != null)
            m_settingsBackground.SetActive(true);
        menu.SetActive(true);
        m_menuPanel.SetActive(false);
        m_enabled = false;

        if(menuscreenscript.m_run == false)
            menuscreenscript.m_run = true;

        yield return new WaitUntil(() => menu.activeSelf == true && m_menuPanel.activeSelf == false);
        m_settingsScreenScript.m_enabled = true;
    }

    public void RunControlsMenuToggle(GameObject menu)
    {
        if(m_controlsBackground != null)
            m_controlsBackground.SetActive(true);

        if(m_toggleControlsOn == null)
            m_toggleControlsOn = StartCoroutine(ToggleControlsMenuOn(menu));

        m_toggleControlsOn = null;
        StopCoroutine(ToggleControlsMenuOn(menu));
    }

    public void RunLevelMenuToggle(GameObject menu)
    {
        if(m_toggleLevelsOn == null)
            m_toggleLevelsOn = StartCoroutine(ToggleLevelMenuOn(menu));

        m_toggleLevelsOn = null;
        StopCoroutine(ToggleLevelMenuOn(menu));
    }

    public void RunSettingsMenuToggle(GameObject menu)
    {
        if(m_toggleSettingsOn == null)
            m_toggleSettingsOn = StartCoroutine(ToggleSettingsMenuOn(menu));

        m_toggleSettingsOn = null;
        StopCoroutine(ToggleSettingsMenuOn(menu));
    }

    public void Quit()
    {
        Application.Quit();
    }

    // Update is called once per frame
    void Update()
    {
        if (m_navInputs.WasPressedThisDynamicUpdate() && m_enabled)
        {
            Vector2 direction = m_navInputs.ReadValue<Vector2>();
            //Debug.Log(direction);

            if(direction == Vector2.up && m_index > 0)
            {
                m_index--;
            }
            else if(direction == Vector2.down && m_index < m_buttons.Length - 1)
            {
                m_index++;
            }
            //Debug.Log("Index: " + m_index);
        }

        if (m_navInputs.WasReleasedThisDynamicUpdate() && m_enabled)
        {
            if(m_currentButton != null)
            {
                m_currentButton.image.sprite = m_buttonSprites[0];
                m_currentButton = m_buttons[m_index];
                m_currentButton.image.sprite = m_buttonSprites[1];
            }      
        }

        if(m_selectInput.WasReleasedThisDynamicUpdate() && m_currentButton != null && m_enabled)
        {
            m_currentButton.onClick.Invoke();
        }
    }
}
