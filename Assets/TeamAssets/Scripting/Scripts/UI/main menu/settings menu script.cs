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
    private int m_index = -1;

    GameObject m_eventSystem;

    protected override void Awake()
    {
        base.Awake();

        m_manager = new datamanager(6);

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
        m_onExitButton = false;
        m_eventSystem = GameObject.Find("EventSystem");
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
        m_index = -1;
        m_eventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);
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
        //Debug.Log(m_manager.GetGameData().settings.checkpointsEnabled);
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

    public void OnSliderPressed()
    {
        m_onExitButton = false;
        m_onCheckBox = false;
        m_exitButton.image.sprite = m_buttonSprites[0];
        //Debug.Log("volume slider pressed");
        m_onSlider = true;
    }

    public void OnBoxPressed()
    {
        m_onExitButton = false;
        m_onSlider = false;
        m_exitButton.image.sprite = m_buttonSprites[0];
        //Debug.Log("checkbox pressed");
        m_onCheckBox = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(m_enabled && m_navInputs.WasPressedThisDynamicUpdate())
        {
            //Debug.Log(m_navInputs.ReadValue<Vector2>());

            if (m_navInputs.ReadValue<Vector2>() == Vector2.down && m_index < 2)
            {
                m_index++;
            }
            else if (m_navInputs.ReadValue<Vector2>() == Vector2.up && m_index > 0)
            {
                m_index--;
            }

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
            }
            //inputs for checkbox
            //else if (m_onCheckBox)
            //{
            //    //if(m_navInputs.ReadValue<Vector2>() == Vector2.up)
            //    //{
                    
            //    //}
            //    //else if(m_navInputs.ReadValue<Vector2>() == Vector2.down)
            //    //{
                    
            //    //    //m_exitButton.image.sprite = m_buttonSprites[1];
            //    //}
            //}
            //exit button inputs
            //else if (m_onExitButton)
            //{
            //    if(m_navInputs.ReadValue<Vector2>() == Vector2.up)
            //    {
                    
            //    }
            //}
            //else
            //{
            //    m_onSlider = true;
            //    //m_index = 0;
            //    m_onCheckBox = false;
            //    m_onExitButton = false;
            //    m_volumeSlider.Select();
            //}

            Debug.Log("index: " + m_index);
        }

        if(m_enabled && m_navInputs.WasReleasedThisDynamicUpdate())
        {
            //Debug.Log(m_navInputs.ReadValue<Vector2>());

            switch (m_index)
            {
                case 0:
                    m_onSlider = true;
                    m_onExitButton = false;
                    m_onCheckBox = false;
                    m_volumeSlider.Select();
                    break;
                case 1:
                    m_onSlider = false;
                    m_onExitButton = false;
                    m_onCheckBox = true;
                    m_checkpointToggle.Select();
                    break;
                case 2:
                    m_onSlider = false;
                    m_onExitButton = true;
                    m_onCheckBox = false;
                    break;
            }
            //if (m_navInputs.ReadValue<Vector2>() == Vector2.down)
            //{
            //    if (m_onSlider)
            //    {
            //        m_onSlider = false;
            //        m_onCheckBox = true;
            //    }
            //    else if (m_onCheckBox)
            //    {
            //        m_onCheckBox = false;
            //        m_onExitButton = true;
            //        m_exitButton.image.sprite = m_buttonSprites[1];
            //    }

            //    Debug.Log("slider: " + m_onSlider);
            //    Debug.Log("checkbox: " + m_onCheckBox);
            //    Debug.Log("exit button: " + m_onExitButton);
            //}
            //else if(m_navInputs.ReadValue<Vector2>() == Vector2.up)
            //{
            //    if (m_onCheckBox)
            //    {
            //        m_onCheckBox = false;
            //        m_onSlider = true;
            //    }
            //    else if (m_onExitButton)
            //    {
            //        m_onExitButton = false;
            //        m_exitButton.image.sprite = m_buttonSprites[0];
            //        m_onCheckBox = true;
            //        m_checkpointToggle.Select();
            //    }

            //    Debug.Log("slider: " + m_onSlider);
            //    Debug.Log("checkbox: " + m_onCheckBox);
            //    Debug.Log("exit button: " + m_onExitButton);
            //}
        }

        if(m_enabled && m_selectInput.WasReleasedThisDynamicUpdate())
        {
            if (m_onExitButton)
            {
                RunToggleSettingsOff();
            }
            else if (m_onCheckBox)
            {
                if (m_changeValue == null)
                    m_changeValue = StartCoroutine(ChangeCheckboxValue());

                m_changeValue = null;
                StartCoroutine(ChangeCheckboxValue());
            }
        }

        //Debug.Log(m_onExitButton);
    }
}
