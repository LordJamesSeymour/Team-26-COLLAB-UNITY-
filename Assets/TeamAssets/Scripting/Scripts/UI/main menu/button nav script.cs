using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class buttonnavscript : MonoBehaviour
{
    private InputAction m_navInputs;
    private InputAction m_selectInput;
    private int m_index = 0;
    private Button m_currentButton;
    private Color m_normalColour;
    private Color m_highlightedColour;
    private ColorBlock m_buttonColorBlock;
    private controlsmenuscript m_controlsScreenScript;
    private levelselectionmenuscript m_levelScreenScript;
    private settingsmenuscript m_settingsScreenScript;
    [HideInInspector] public bool m_mainMenuPanelEnabled = true;
    //private Vector2 direction;

    [SerializeField] public GameObject m_mainMenuPanel;
    [SerializeField] public GameObject m_controlsPanel;
    [SerializeField] public GameObject m_levelsPanel;
    [SerializeField] public GameObject m_settingsPanel;
    [SerializeField] Button[] m_buttons;
    [SerializeField] Sprite[] m_buttonSprites;

    private Coroutine m_toggleControlsOn;
    private Coroutine m_toggleLevelsOn;
    private Coroutine m_toggleSettingsOn;
    
    private void Awake()
    {
        m_navInputs = InputSystem.actions.FindAction("Navigate");
        m_selectInput = InputSystem.actions.FindAction("Select");

        m_controlsScreenScript = GetComponent<controlsmenuscript>();
        if (!m_controlsScreenScript)
            Debug.LogError("no controls screen script attached");

        m_levelScreenScript = GetComponent<levelselectionmenuscript>();
        if ((!m_levelScreenScript))
            Debug.LogError("no level select screen script attached");

        m_settingsScreenScript = GetComponent<settingsmenuscript>();
        if (!m_settingsScreenScript)
            Debug.LogError("no settings screen script attached");
        //m_normalColour = m_buttons[0].colors.normalColor;
        //m_highlightedColour = m_buttons[0].colors.highlightedColor;
        //Debug.Log(m_highlightedColour);
        //m_buttonColorBlock.normalColor = m_highlightedColour;
        //Debug.Log(m_buttonColorBlock.normalColor);
        //m_currentButton = m_buttons[m_index];
        //m_currentButton.colors = m_buttonColorBlock;

        m_currentButton = m_buttons[m_index];
        m_currentButton.image.sprite = m_buttonSprites[1];
    }

    //function for when the mouse hovers over a button
    public void OnPointerEnter(int i)
    {
        //m_buttonColorBlock.normalColor = m_normalColour;
        //m_currentButton.colors = m_buttonColorBlock;
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
        //m_buttonColorBlock.normalColor = m_highlightedColour;
        //m_currentButton.colors = m_buttonColorBlock;
    }

    private IEnumerator ToggleControlsMenuOn(GameObject menu)
    {
        //if (m_mainMenuPanelEnabled)
        //{
        //    m_mainMenuPanel.SetActive(false);
        //    m_controlsPanel.SetActive(true);
        //    m_controlsScreenScript.m_enabled = true;
        //}
        //else
        //{
        //    m_mainMenuPanel.SetActive(true);
        //    m_controlsPanel.SetActive(false);
        //    m_controlsScreenScript.m_enabled = false;
        //}

        //m_mainMenuPanelEnabled = !m_mainMenuPanelEnabled;

        menu.SetActive(true);
        m_mainMenuPanel.SetActive(false);
        m_mainMenuPanelEnabled = false;
        yield return new WaitUntil(() => menu.activeSelf == true && m_mainMenuPanel.activeSelf == false);
        m_controlsScreenScript.m_enabled = true;
        m_toggleControlsOn = null;
        StopCoroutine(ToggleControlsMenuOn(menu));
    }

    private IEnumerator ToggleLevelMenuOn(GameObject menu)
    {
        menu.SetActive(true);
        m_mainMenuPanel.SetActive(false);
        m_mainMenuPanelEnabled = false;
        yield return new WaitUntil(() => menu.activeSelf == true && m_mainMenuPanel.activeSelf == false);
        m_levelScreenScript.m_enabled = true;
        m_toggleLevelsOn = null;
        StopCoroutine(ToggleLevelMenuOn(menu));
    }

    private IEnumerator ToggleSettingsMenuOn(GameObject menu)
    {
        menu.SetActive(true);
        m_mainMenuPanel.SetActive(false);
        m_mainMenuPanelEnabled = false;
        yield return new WaitUntil(() => menu.activeSelf == true && m_mainMenuPanel.activeSelf == false);
        m_settingsScreenScript.m_enabled = true;
        m_toggleSettingsOn = null;
        StopCoroutine(ToggleSettingsMenuOn(menu));
    }

    public void RunControlsMenuToggle(GameObject menu)
    {
        if(m_toggleControlsOn == null)
            m_toggleControlsOn = StartCoroutine(ToggleControlsMenuOn(menu));
    }

    public void RunLevelMenuToggle(GameObject menu)
    {
        if(m_toggleLevelsOn == null)
            m_toggleLevelsOn = StartCoroutine(ToggleLevelMenuOn(menu));
    }

    public void RunSettingsMenuToggle(GameObject menu)
    {
        if(m_toggleSettingsOn == null)
            m_toggleSettingsOn = StartCoroutine(ToggleSettingsMenuOn(menu));

        m_toggleSettingsOn = null;
        StopCoroutine(ToggleSettingsMenuOn(menu));
    }

    // Update is called once per frame
    void Update()
    {
        if (m_navInputs.WasPressedThisDynamicUpdate() && m_mainMenuPanelEnabled)
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

        if (m_navInputs.WasReleasedThisDynamicUpdate() && m_mainMenuPanelEnabled)
        {
            if(m_currentButton != null)
            {
                m_currentButton.image.sprite = m_buttonSprites[0];
                m_currentButton = m_buttons[m_index];
                m_currentButton.image.sprite = m_buttonSprites[1];

                //m_buttonColorBlock.normalColor = m_normalColour;
                //m_currentButton.colors = m_buttonColorBlock;
                //m_buttonColorBlock.normalColor = m_highlightedColour;
                //m_currentButton = m_buttons[m_index];
                //m_currentButton.colors = m_buttonColorBlock;
            }      
        }

        if(m_selectInput.WasReleasedThisDynamicUpdate() && m_currentButton != null && m_mainMenuPanelEnabled)
        {
            m_currentButton.onClick.Invoke();
        }
    }
}
