using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class settingsmenuscript : menuscreenscript
{
    [SerializeField] Toggle m_checkpointToggle;
    [SerializeField] Toggle m_fullscreenToggle;
    [SerializeField] Slider m_backgroundMusicSlider;
    [SerializeField] Slider m_soundEffectsSlider;
    //[SerializeField] AudioSource m_backgroundMusic;

    private Coroutine m_toggleMenu;
    private Coroutine m_toggleCheckpoint;
    private Coroutine m_changeCheckboxValue;
    //private Coroutine m_changeFullscreenValue;
    private datamanager m_manager;
    private bool m_onBackgroundSlider;
    private bool m_onSoundEffectsSlider;
    private bool m_onCheckpointBox;
    private bool m_onFullscreenBox;
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
        m_onCheckpointBox = false;
        m_onFullscreenBox = false;
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
        //Debug.Log(m_toggleMenu == null);
        if (m_toggleMenu == null)
            m_toggleMenu = StartCoroutine(ToggleSettingsMenuOff());

        m_toggleMenu = null;
        StopCoroutine(ToggleSettingsMenuOff());
    }

    public IEnumerator ToggleCheckpointsEnabled()
    {
        /*Checkpoint.m_checkpointsEnabled = !Checkpoint.m_checkpointsEnabled*/
        m_manager.LoadGameData();
        m_manager.SetCheckpointsEnabled(m_checkpointToggle.isOn);
        m_manager.SaveGameData();
        yield return new WaitForSeconds(0.1f);
        //Debug.Log(m_manager.GetGameData().settings.checkpointsEnabled);
    }

    public IEnumerator ChangeCheckboxValue(Toggle box)
    {
        box.isOn = !box.isOn;
        yield return new WaitForSeconds(0.1f);
    }

    public void RunToggleCheckpoint()
    {
        if (m_toggleCheckpoint == null)
            m_toggleCheckpoint = StartCoroutine(ToggleCheckpointsEnabled());

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

    public void OnSliderPressed(Slider slider)
    {
        m_onExitButton = false;
        m_onCheckpointBox = false;
        m_onSoundEffectsSlider = false;
        m_onBackgroundSlider = false;
        m_onFullscreenBox = false;
        m_backgroundMusicSlider.image.sprite = m_buttonSprites[2];
        m_soundEffectsSlider.image.sprite = m_buttonSprites[2];
        m_checkpointToggle.image.sprite = m_buttonSprites[0];
        m_fullscreenToggle.image.sprite = m_buttonSprites[0];
        m_exitButton.image.sprite = m_buttonSprites[0];

        if (slider == m_backgroundMusicSlider)
        {
            m_onBackgroundSlider = true;
            m_backgroundMusicSlider.image.sprite = m_buttonSprites[3];
            m_index = 0;
        }
        else if (slider == m_soundEffectsSlider)
        {
            m_onSoundEffectsSlider = true;
            m_soundEffectsSlider.image.sprite = m_buttonSprites[3];
            m_index = 1;
        }

        Debug.Log("index: " + m_index);
        m_eventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);
    }

    public void OnBoxPressed(Toggle box)
    {
        m_onExitButton = false;
        m_onBackgroundSlider = false;
        m_onSoundEffectsSlider = false;
        m_onCheckpointBox = false;
        m_onFullscreenBox = false;
        m_backgroundMusicSlider.image.sprite = m_buttonSprites[2];
        m_soundEffectsSlider.image.sprite = m_buttonSprites[2];
        m_checkpointToggle.image.sprite = m_buttonSprites[0];
        m_fullscreenToggle.image.sprite = m_buttonSprites[0];
        m_exitButton.image.sprite = m_buttonSprites[0];

        if(box == m_checkpointToggle)
        {
            m_onCheckpointBox = true;
            m_checkpointToggle.image.sprite = m_buttonSprites[1];
            m_index = 2;
        }
        else if(box == m_fullscreenToggle)
        {
            m_onFullscreenBox = true;
            m_fullscreenToggle.image.sprite = m_buttonSprites[1];
            m_index = 3;
        }

        Debug.Log("index: " + m_index);
        m_eventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);
    }

    // Update is called once per frame
    void Update()
    {
        if(m_enabled && m_navInputs.WasPressedThisDynamicUpdate())
        {
            //Debug.Log(m_navInputs.ReadValue<Vector2>());

            if (m_navInputs.ReadValue<Vector2>() == Vector2.down && m_index < 4)
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
                    m_backgroundMusicSlider.Select();
                    m_backgroundMusicSlider.value += 1.0f;
                }
                else if(m_navInputs.ReadValue<Vector2>() == Vector2.left && m_backgroundMusicSlider.value > m_backgroundMusicSlider.minValue)
                {
                    m_backgroundMusicSlider.Select();
                    m_backgroundMusicSlider.value -= 1.0f;
                }
            }
            else if (m_onSoundEffectsSlider)
            {
                if (m_navInputs.ReadValue<Vector2>() == Vector2.right && m_soundEffectsSlider.value < m_soundEffectsSlider.maxValue)
                {
                    m_soundEffectsSlider.Select();
                    m_soundEffectsSlider.value += 1.0f;
                }
                else if (m_navInputs.ReadValue<Vector2>() == Vector2.left && m_soundEffectsSlider.value > m_soundEffectsSlider.minValue)
                {
                    m_soundEffectsSlider.Select();
                    m_soundEffectsSlider.value -= 1.0f;
                }
            }
        }

        if(m_enabled && m_navInputs.WasReleasedThisDynamicUpdate())
        {
            m_eventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);
            switch (m_index)
                {
                    case 0:
                        m_onBackgroundSlider = true;
                        m_onSoundEffectsSlider = false;
                        //m_onExitButton = false;
                        //m_onCheckpointBox = false;
                        //m_onFullscreenBox = false;
                        m_backgroundMusicSlider.image.sprite = m_buttonSprites[3];
                        m_soundEffectsSlider.image.sprite = m_buttonSprites[2];
                        //m_checkpointToggle.image.sprite = m_buttonSprites[0];
                        //m_fullscreenToggle.image.sprite = m_buttonSprites[0];
                        m_exitButton.image.sprite = m_buttonSprites[0];
                        //m_backgroundMusicSlider.Select();
                        break;
                    case 1:
                        m_onBackgroundSlider = false;
                        m_onSoundEffectsSlider = true;
                        m_onCheckpointBox = false;
                        //m_onFullscreenBox = false;
                        //m_onExitButton = false;
                        m_backgroundMusicSlider.image.sprite = m_buttonSprites[2];
                        m_soundEffectsSlider.image.sprite = m_buttonSprites[3];
                        m_checkpointToggle.image.sprite = m_buttonSprites[0];
                        //m_fullscreenToggle.image.sprite = m_buttonSprites[0];
                        //m_exitButton.image.sprite = m_buttonSprites[0];
                        //m_soundEffectsSlider.Select();
                        break;
                    case 2:
                        //m_onBackgroundSlider = false;
                        m_onSoundEffectsSlider = false;
                        //m_onExitButton = false;
                        m_onCheckpointBox = true;
                        m_onFullscreenBox = false;
                        //m_backgroundMusicSlider.image.sprite = m_buttonSprites[2];
                        m_soundEffectsSlider.image.sprite = m_buttonSprites[2];
                        m_checkpointToggle.image.sprite = m_buttonSprites[1];
                        m_fullscreenToggle.image.sprite = m_buttonSprites[0];
                        //m_exitButton.image.sprite = m_buttonSprites[0];
                        //m_checkpointToggle.Select();
                        break;
                    case 3:
                        //m_onBackgroundSlider = false;
                        //m_onSoundEffectsSlider = false;
                        m_onExitButton = false;
                        m_onCheckpointBox = false;
                        m_onFullscreenBox = true;
                        //m_backgroundMusicSlider.image.sprite = m_buttonSprites[2];
                        //m_soundEffectsSlider.image.sprite = m_buttonSprites[2];
                        m_checkpointToggle.image.sprite = m_buttonSprites[0];
                        m_fullscreenToggle.image.sprite= m_buttonSprites[1];
                        m_exitButton.image.sprite = m_buttonSprites[0];
                        break;
                    case 4:
                        //m_onBackgroundSlider = false;
                        //m_onSoundEffectsSlider = false;
                        m_onExitButton = true;
                        //m_onCheckpointBox = false;
                        m_onFullscreenBox = false;
                        //m_backgroundMusicSlider.image.sprite = m_buttonSprites[2];
                        //m_soundEffectsSlider.image.sprite = m_buttonSprites[2];
                        //m_checkpointToggle.image.sprite = m_buttonSprites[0];
                        m_fullscreenToggle.image.sprite = m_buttonSprites[0];
                        m_exitButton.image.sprite = m_buttonSprites[1];
                        break;
                }

            Debug.Log("index: " + m_index);
        }

        if (m_enabled && m_selectInput.WasReleasedThisDynamicUpdate())
        {
            //Debug.Log(m_onExitButton);

            if (m_onExitButton)
            {
                RunToggleSettingsOff();
            }
            else if (m_onCheckpointBox)
            {
                if (m_changeCheckboxValue == null)
                    m_changeCheckboxValue = StartCoroutine(ChangeCheckboxValue(m_checkpointToggle));

                m_changeCheckboxValue = null;
                StopCoroutine(ChangeCheckboxValue(m_checkpointToggle));
            }
            else if (m_onFullscreenBox)
            {
                if (m_changeCheckboxValue == null)
                    m_changeCheckboxValue = StartCoroutine(ChangeCheckboxValue(m_fullscreenToggle));

                m_changeCheckboxValue = null;
                StopCoroutine(ChangeCheckboxValue(m_fullscreenToggle));
            }
        }
    }
}