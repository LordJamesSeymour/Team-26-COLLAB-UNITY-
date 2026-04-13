using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class settingsmenuscript : menuscreenscript
{
    [SerializeField] Toggle m_checkpointToggle;
    [SerializeField] Slider m_backgroundMusicSlider;
    [SerializeField] Slider m_soundEffectsSlider;
    //[SerializeField] AudioSource m_backgroundMusic;

    private Coroutine m_toggleMenu;
    private Coroutine m_toggleCheckpoint;
    private Coroutine m_changeValue;
    private datamanager m_manager;
    private bool m_onBackgroundSlider;
    private bool m_onSoundEffectsSlider;
    private bool m_onCheckBox;
    private static bool m_run;
    private int m_index = -1;
    //private Vector2 m_direction;

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
        m_backgroundMusicSlider.value = m_manager.GetGameData().settings.backgroundMusicVolume;
        m_soundEffectsSlider.value = m_manager.GetGameData().settings.soundEffectsVolume;
        m_onExitButton = false;
        m_eventSystem = GameObject.Find("EventSystem");
    }

    private IEnumerator ToggleSettingsMenuOff()
    {
        m_buttonScript.m_settingsPanel.SetActive(false);
        m_buttonScript.m_mainMenuPanel.SetActive(true);
        m_onExitButton = false;
        m_onBackgroundSlider = false;
        m_onSoundEffectsSlider = false;
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
        if (m_toggleMenu == null)
            m_toggleMenu = StartCoroutine(ToggleSettingsMenuOff());

        m_toggleMenu = null;
        StopCoroutine(ToggleSettingsMenuOff());
    }

    public IEnumerator ToggleCheckpointsEnabled()
    {
        /*Checkpoint.m_checkpointsEnabled = !Checkpoint.m_checkpointsEnabled*/
        ;
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

    public void UpdateBackgroundVolume(AudioSource source)
    {
        source.volume = m_backgroundMusicSlider.value / 100;
        m_manager.SetBackgroundVolume(m_backgroundMusicSlider.value);
        m_manager.SaveGameData();
        Debug.Log(m_manager.GetGameData().settings.backgroundMusicVolume);
    }

    public void UpdateSoundEffectsVolume(AudioSource source)
    {
        source.volume = m_soundEffectsSlider.value / 100;
        m_manager.SetSoundEffectsVolume(m_soundEffectsSlider.value);
        m_manager.SaveGameData();
        Debug.Log(m_manager.GetGameData().settings.soundEffectsVolume);
    }

    public void OnBackgroundSliderPressed(Slider slider)
    {
        m_onExitButton = false;
        m_onCheckBox = false;
        m_onSoundEffectsSlider = false;
        m_onBackgroundSlider = false;
        m_exitButton.image.sprite = m_buttonSprites[0];
        //Debug.Log("volume slider pressed");

        if (slider == m_backgroundMusicSlider)
        {
            m_onBackgroundSlider = true;
            m_index = 0;
        }
        else if (slider == m_soundEffectsSlider)
        {
            m_onSoundEffectsSlider = true;
            m_index = 1;
        }

        Debug.Log("index: " + m_index);
        m_eventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);
    }

    public void OnSliderDeselected()
    {
        m_eventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);

    }

    public void OnBoxPressed()
    {
        m_onExitButton = false;
        m_onBackgroundSlider = false;
        m_onSoundEffectsSlider = false;
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

            if (m_navInputs.ReadValue<Vector2>() == Vector2.down && m_index < 3)
            {
                //m_direction = Vector2.down;
                m_index++;
            }
            else if (m_navInputs.ReadValue<Vector2>() == Vector2.up && m_index > 0)
            {
                //m_direction = Vector2.up;
                m_index--;
            }

            //inputs for volume slider
            if (m_onBackgroundSlider)
            {
                if(m_navInputs.ReadValue<Vector2>() == Vector2.right && m_backgroundMusicSlider.value < m_backgroundMusicSlider.maxValue)
                {
                    m_backgroundMusicSlider.value += 1.0f;
                }
                else if(m_navInputs.ReadValue<Vector2>() == Vector2.left && m_backgroundMusicSlider.value > m_backgroundMusicSlider.minValue)
                {
                    m_backgroundMusicSlider.value -= 1.0f;
                }
            }
            else if (m_onSoundEffectsSlider)
            {
                if (m_navInputs.ReadValue<Vector2>() == Vector2.right && m_soundEffectsSlider.value < m_soundEffectsSlider.maxValue)
                {
                    m_soundEffectsSlider.value += 1.0f;
                }
                else if (m_navInputs.ReadValue<Vector2>() == Vector2.left && m_soundEffectsSlider.value > m_soundEffectsSlider.minValue)
                {
                    m_soundEffectsSlider.value -= 1.0f;
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

        }

        if(m_enabled && m_navInputs.WasReleasedThisDynamicUpdate())
        {
            //Debug.Log(m_navInputs.ReadValue<Vector2>());

            //if (m_direction == Vector2.down && m_index < 3)
            //    m_index++;
            //else if(m_direction == Vector2.up && m_index > 0)
            //    m_index--;

                switch (m_index)
                {
                    case 0:
                        m_onBackgroundSlider = true;
                        m_onSoundEffectsSlider = false;
                        m_onExitButton = false;
                        m_onCheckBox = false;
                        m_backgroundMusicSlider.image.sprite = m_buttonSprites[3];
                        m_soundEffectsSlider.image.sprite = m_buttonSprites[2];
                        m_checkpointToggle.image.sprite = m_buttonSprites[0];
                        m_exitButton.image.sprite = m_buttonSprites[0];
                        //m_backgroundMusicSlider.Select();
                        break;
                    case 1:
                        m_onBackgroundSlider = false;
                        m_onSoundEffectsSlider = true;
                        m_onCheckBox = false;
                        m_onExitButton = false;
                        m_backgroundMusicSlider.image.sprite = m_buttonSprites[2];
                        m_soundEffectsSlider.image.sprite = m_buttonSprites[3];
                        m_checkpointToggle.image.sprite = m_buttonSprites[0];
                        m_exitButton.image.sprite = m_buttonSprites[0];
                        //m_soundEffectsSlider.Select();
                        break;
                    case 2:
                        m_onBackgroundSlider = false;
                        m_onSoundEffectsSlider = false;
                        m_onExitButton = false;
                        m_onCheckBox = true;
                        m_backgroundMusicSlider.image.sprite = m_buttonSprites[2];
                        m_soundEffectsSlider.image.sprite = m_buttonSprites[2];
                        m_checkpointToggle.image.sprite = m_buttonSprites[1];
                        m_exitButton.image.sprite = m_buttonSprites[0];
                        //m_checkpointToggle.Select();
                        break;
                    case 3:
                        m_onBackgroundSlider = false;
                        m_onSoundEffectsSlider = false;
                        m_onExitButton = true;
                        m_onCheckBox = false;
                        m_backgroundMusicSlider.image.sprite = m_buttonSprites[2];
                        m_soundEffectsSlider.image.sprite = m_buttonSprites[2];
                        m_checkpointToggle.image.sprite = m_buttonSprites[0];
                        m_exitButton.image.sprite = m_buttonSprites[1];
                        break;
                }

            Debug.Log("index: " + m_index);

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

        if (m_enabled && m_selectInput.WasReleasedThisDynamicUpdate())
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
                StopCoroutine(ChangeCheckboxValue());
            }
        }

        //Debug.Log(m_onExitButton);
    }
}
