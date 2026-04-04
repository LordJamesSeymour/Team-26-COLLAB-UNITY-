using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class settingsmenuscript : menuscreenscript
{
    [SerializeField] Toggle m_checkpointToggle;
    [SerializeField] Slider m_volumeSlider;

    private Coroutine m_toggleMenu;
    private Coroutine m_toggleCheckpoint;
    private Coroutine m_changeValue;
    private datamanager m_manager;
    private bool m_onSlider;
    private bool m_onCheckBox;
    private static bool m_run;

    protected override void Awake()
    {
        base.Awake();

        m_manager = new datamanager(3);

        if (m_run == false)
        {
            m_run = true;
        }
        else
        {
            m_manager.LoadGameData();
        }

        //Debug.Log(m_manager.GetGameData().settings.checkpointsEnabled);
        m_checkpointToggle.isOn = m_manager.GetGameData().settings.checkpointsEnabled;
        m_volumeSlider.value = m_manager.GetGameData().settings.volume;
    }

    private IEnumerator ToggleSettingsMenuOff()
    {
        m_buttonScript.m_settingsPanel.SetActive(false);
        m_buttonScript.m_mainMenuPanel.SetActive(true);
        m_onExitButton = false;
        m_onSlider = false;
        m_onCheckBox = false;
        m_exitButton.image.sprite = m_buttonSprites[0];
        m_enabled = false;
        //yield return new WaitForSeconds(2.0f);
        yield return new WaitUntil(() => m_buttonScript.m_settingsPanel.activeSelf == false && m_buttonScript.m_mainMenuPanel.activeSelf == true);
        yield return new WaitForSeconds(0.1f);
        m_buttonScript.m_mainMenuPanelEnabled = true;
        //m_toggle = null;
        //StopCoroutine(ToggleSettingsMenuOff());
    }

    public void RunToggleSettingsOff()
    {
        if(m_toggleMenu == null)
            m_toggleMenu = StartCoroutine(ToggleSettingsMenuOff());

        m_toggleMenu = null;
        StopCoroutine(ToggleSettingsMenuOff());
    }

    public IEnumerator ToggleCheckpointsEnabled()
    {
        /*Checkpoint.m_checkpointsEnabled = !Checkpoint.m_checkpointsEnabled*/;
        m_manager.LoadGameData();
        m_manager.SetCheckpointsEnabled(m_checkpointToggle.isOn);
        m_manager.SaveGameData();
        yield return new WaitForSeconds(0.1f);
        Debug.Log(m_manager.GetGameData().settings.checkpointsEnabled);
    }

    public IEnumerator ChangeCheckboxValue()
    {
        m_checkpointToggle.isOn = !m_checkpointToggle.isOn;
        yield return new WaitForSeconds(0.1f);
    }

    public void RunToggleCheckpoint()
    {
        if (m_toggleCheckpoint == null)
            m_toggleMenu = StartCoroutine(ToggleCheckpointsEnabled());

        m_toggleCheckpoint = null;
        StopCoroutine(ToggleCheckpointsEnabled());
    }

    public void UpdateVolume()
    {
        m_manager.SetVolume(m_volumeSlider.value);
        m_manager.SaveGameData();
        Debug.Log(m_manager.GetGameData().settings.volume);
    }

    // Update is called once per frame
    void Update()
    {
        if(m_enabled && m_navInputs.WasPressedThisDynamicUpdate())
        {
            //inputs for volume slider
            if (m_onSlider)
            {
                if(m_navInputs.ReadValue<Vector2>() == Vector2.right && m_volumeSlider.value < m_volumeSlider.maxValue)
                {
                    m_volumeSlider.value += 1.0f;
                }
                else if(m_navInputs.ReadValue<Vector2>() == Vector2.left && m_volumeSlider.value > m_volumeSlider.minValue)
                {
                    m_volumeSlider.value -= 1.0f;
                }
                else if(m_navInputs.ReadValue<Vector2>() == Vector2.down)
                {
                    m_onSlider = false;
                    m_onCheckBox = true;
                }
            }
            //inputs for checkbox
            else if (m_onCheckBox)
            {
                if(m_navInputs.ReadValue<Vector2>() == Vector2.up)
                {
                    m_onCheckBox = false;
                    m_onSlider = true;
                }
                else if(m_navInputs.ReadValue<Vector2>() == Vector2.down)
                {
                    m_onCheckBox = false;
                    m_onExitButton = true;
                    m_exitButton.image.sprite = m_buttonSprites[1];
                }
            }
            //exit button inputs
            else if (m_onExitButton)
            {
                if(m_navInputs.ReadValue<Vector2>() == Vector2.up)
                {
                    m_onExitButton = false;
                    m_onCheckBox = true;
                    m_checkpointToggle.Select();
                }
            }
            else
            {
                m_onSlider = true;
                m_volumeSlider.Select();
            }
        }

        if(m_enabled && m_selectInput.WasReleasedThisDynamicUpdate())
        {
            if (m_onExitButton)
                RunToggleSettingsOff();
            else if (m_onCheckBox)
            {
                if (m_changeValue == null)
                    m_changeValue = StartCoroutine(ChangeCheckboxValue());

                m_changeValue = null;
                StartCoroutine(ChangeCheckboxValue());
            }
        }
    }
}
