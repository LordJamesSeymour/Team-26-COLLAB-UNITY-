using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class settingsmenuscript : menuscreenscript
{
    [SerializeField] Toggle m_checkpointToggle;
    [SerializeField] Toggle m_fullscreenToggle;
    [SerializeField] Slider m_backgroundMusicSlider;
    [SerializeField] Slider m_soundEffectsSlider;
    [SerializeField] Slider m_sensitivitySlider;
    [SerializeField] TMP_InputField m_widthInput;
    [SerializeField] TMP_InputField m_heightInput;
    [SerializeField] GameObject m_scrollArea;
    [SerializeField] TextMeshProUGUI m_sensitivityValueText;
    //[SerializeField] AudioSource m_backgroundMusic;

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string effectsVolumeName = "EffectsVolume";
    [SerializeField] private string musicVolumeName = "MusicVolume";

    private Coroutine m_toggleMenu;
    private Coroutine m_toggleCheckpoint;
    private Coroutine m_changeCheckboxValue;
    //private Coroutine m_updateSensitivityVal;
    private ScrollRect m_scrollRect;
    //private Coroutine m_changeFullscreenValue;
    private datamanager m_manager;
    private bool m_onBackgroundSlider;
    private bool m_onSoundEffectsSlider;
    private bool m_onCheckpointBox;
    private bool m_onSensitivitySlider;
    private bool m_onFullscreenBox;
    private bool m_onResizeInputs;
    private bool m_onHeight;
    private string m_widthInputText;
    private string m_heightInputText;
    //private static bool m_run;
    private static bool m_started;
    private int m_index = -1;
    private int m_maxWidth;
    private int m_maxHeight;
    //private float m_sensitivityStep = 0.01f;
    //private float m_sensitivity;
    //private Vector2 m_direction;

    GameObject m_eventSystem;

    protected override void Awake()
    {
        base.Awake();

        //Screen.fullScreen = true;

        //Screen.SetResolution(1920, 888, true);
        Resolution maxResolution = FindHighestRes();
        m_maxWidth = maxResolution.width;
        m_maxHeight = maxResolution.height;

        if (m_started == false)
        {
            m_started = true;
            Screen.SetResolution(maxResolution.width, maxResolution.height, true);
        }

        m_manager = new datamanager(6);

        //if (m_run == false)
        //{
        //    m_run = true;
        //}
        //else
        //{
        //    m_manager.LoadGameData();
        //}

        m_scrollRect = m_scrollArea.GetComponent<ScrollRect>();
        if (!m_scrollRect)
            Debug.LogError("no scroll rect on object");

        //Debug.Log(m_manager.GetGameData().settings.checkpointsEnabled);

        m_fullscreenToggle.isOn = true;
        m_widthInput.interactable = false;
        m_heightInput.interactable = false;
        m_onExitButton = false;
        m_eventSystem = GameObject.Find("EventSystem");
        m_buttonScript.m_settingsPanel.GetComponent<menuscreeneventsmanager>().IsVisible += SettingsVisible;
    }


    private void SettingsVisible()
    {
        Debug.Log(m_run);
        if (m_run)
            m_manager.LoadGameData();
        else
            m_run = true;

        Debug.Log("settings visible");
        m_checkpointToggle.isOn = m_manager.GetGameData().settings.checkpointsEnabled;
        //m_backgroundMusicSlider.value = m_manager.GetGameData().settings.backgroundMusicVolume;
        //m_soundEffectsSlider.value = m_manager.GetGameData().settings.soundEffectsVolume;
        m_sensitivitySlider.value = m_manager.GetGameData().settings.sensitivity;

        float musicDB;
        audioMixer.GetFloat(musicVolumeName, out musicDB);
        float sfxDB;
        audioMixer.GetFloat(effectsVolumeName, out sfxDB);

        float musicVolume = 100 * Mathf.Pow(10, musicDB / 20);
        m_backgroundMusicSlider.value = musicVolume;
        float SFXVolume = 100 * Mathf.Pow(10, sfxDB / 20);
        m_soundEffectsSlider.value = SFXVolume;
    }

    private Resolution FindHighestRes()
    {
        Resolution[] resolutions = Screen.resolutions;
        Resolution maxRes = resolutions[0];
        for(int i = 1; i < resolutions.Length; i++)
        {
            if (resolutions[i].width > maxRes.width && resolutions[i].height > maxRes.height)
            {
                maxRes = resolutions[i];
            }
        }

        return maxRes;
    }

    private IEnumerator ToggleSettingsMenuOff()
    {
        m_buttonScript.m_settingsPanel.SetActive(false);
        m_buttonScript.m_menuPanel.SetActive(true);
        m_onExitButton = false;
        m_onBackgroundSlider = false;
        m_onSoundEffectsSlider = false;
        m_onCheckpointBox = false;
        m_onSensitivitySlider = false;
        m_onFullscreenBox = false;
        m_onResizeInputs = false;
        m_onHeight = false;

        if(m_index != 6)
        {
            switch (m_index)
            {
                default:
                    break;
                case 0:
                    m_backgroundMusicSlider.image.sprite = m_buttonSprites[2];
                    break;
                case 1:
                    m_soundEffectsSlider.image.sprite = m_buttonSprites[2];
                    break;
                case 2:
                    m_checkpointToggle.image.sprite = m_buttonSprites[0];
                    break;
                case 3:
                    m_sensitivitySlider.image.sprite = m_buttonSprites[2];
                    break;
                case 4:
                    m_fullscreenToggle.image.sprite = m_buttonSprites[0];
                    break;
                case 5:
                    m_widthInput.image.sprite = m_buttonSprites[0];
                    m_heightInput.image.sprite = m_buttonSprites[0];
                    break;
            }
        }

        m_exitButton.image.sprite = m_buttonSprites[0];
        m_enabled = false;
        m_index = -1;
        m_scrollRect.verticalNormalizedPosition = 1.0f;
        m_eventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);
        //yield return new WaitForSeconds(2.0f);
        yield return new WaitUntil(() => m_buttonScript.m_settingsPanel.activeSelf == false && m_buttonScript.m_menuPanel.activeSelf == true);
        yield return new WaitForSeconds(0.1f);
        m_buttonScript.m_enabled = true;
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
        //m_manager.LoadGameData();
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
        //float volume = m_backgroundMusicSlider.value * .8f - 80;
        float db = Mathf.Log10(Mathf.Max(0.0001f, m_backgroundMusicSlider.value / 100)) * 20;
        audioMixer.SetFloat(musicVolumeName, db);
        m_manager.SetBackgroundVolume(db);
        m_manager.SaveGameData();
        Debug.Log(m_manager.GetGameData().settings.backgroundMusicVolume);

        source.Play();
    }

    public void UpdateSoundEffectsVolume(AudioSource source)
    {
        //float volume = m_soundEffectsSlider.value * .9f - 80;
        float db = Mathf.Log10(Mathf.Max(0.0001f, m_soundEffectsSlider.value / 100)) * 20;
        audioMixer.SetFloat(effectsVolumeName, db);
        m_manager.SetSoundEffectsVolume(db);
        m_manager.SaveGameData();
        Debug.Log(m_manager.GetGameData().settings.soundEffectsVolume);

        source.Play();
    }

    public void UpdateSensitivitySlider()
    {
        //m_sensitivitySlider.value = (float)Math.Round(m_sensitivitySlider.value, 2);
        ////m_sensitivitySlider.value = Mathf.Lerp(m_sensitivitySlider.value, m_sensitivity, 6 * Time.deltaTime);
        //m_sensitivity = m_sensitivitySlider.value;
        m_manager.SetSensitivity(m_sensitivitySlider.value);
        m_manager.SaveGameData();
        Debug.Log(m_manager.GetGameData().settings.sensitivity);
    }

    public void MakeFullscreen()
    {
        if (m_fullscreenToggle.isOn)
        {
            m_widthInput.interactable = false;
            m_heightInput.interactable = false;
            Screen.SetResolution(FindHighestRes().width, FindHighestRes().height, true);
        }
        else
        {
            m_widthInput.interactable = true;
            m_heightInput.interactable = true;
        }
        Screen.fullScreen = m_fullscreenToggle.isOn;
        //Debug.Log(m_fullscreenToggle.isOn);
        //Debug.Log(Screen.fullScreen);
    }

    public void ChangeScreenSize(TMP_InputField input)
    {
        if(input == m_widthInput)
        {
            int width = Convert.ToInt32(m_widthInputText);
            if (width <= 750)
            {
                Screen.SetResolution(750, Screen.height, false);
                m_widthInputText = "750";
            }
            else
            {          
                Screen.SetResolution(width, Screen.height, false);
            }
            m_widthInput.text = m_widthInputText;

        }
        else if(input == m_heightInput)
        {
            int height = Convert.ToInt32(m_heightInputText);
            if (height <= 750)
            {
                Screen.SetResolution(Screen.width, 750, false);
                m_heightInputText = "750";
            }
            else
            {
                Screen.SetResolution(Screen.width, height, false);
            }
            m_heightInput.text = m_heightInputText;
        }
    }

    public void OnSliderPressed(Slider slider)
    {
        m_onExitButton = false;
        m_onCheckpointBox = false;
        m_onSoundEffectsSlider = false;
        m_onBackgroundSlider = false;
        m_onFullscreenBox = false;
        m_onResizeInputs = false;
        m_onSensitivitySlider = false;
        m_onHeight = false;
        m_backgroundMusicSlider.image.sprite = m_buttonSprites[2];
        m_soundEffectsSlider.image.sprite = m_buttonSprites[2];
        m_checkpointToggle.image.sprite = m_buttonSprites[0];
        m_sensitivitySlider.image.sprite = m_buttonSprites[2];
        m_fullscreenToggle.image.sprite = m_buttonSprites[0];
        m_widthInput.image.sprite = m_buttonSprites[0];
        m_heightInput.image.sprite = m_buttonSprites[0];
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
        else if(slider == m_sensitivitySlider)
        {
            m_onSensitivitySlider = true;
            m_sensitivitySlider.image.sprite = m_buttonSprites[3];
            m_index = 3;
        }

            m_scrollRect.verticalNormalizedPosition = 1.0f;

        //Debug.Log("index: " + m_index);
        m_eventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);
    }

    public void OnBoxPressed(Toggle box)
    {
        m_onExitButton = false;
        m_onBackgroundSlider = false;
        m_onSoundEffectsSlider = false;
        m_onCheckpointBox = false;
        m_onSensitivitySlider = false;
        m_onFullscreenBox = false;
        m_onResizeInputs = false;
        m_onHeight = false;
        m_backgroundMusicSlider.image.sprite = m_buttonSprites[2];
        m_soundEffectsSlider.image.sprite = m_buttonSprites[2];
        m_checkpointToggle.image.sprite = m_buttonSprites[0];
        m_sensitivitySlider.image.sprite = m_buttonSprites[2];
        m_fullscreenToggle.image.sprite = m_buttonSprites[0];
        m_widthInput.image.sprite = m_buttonSprites[0];
        m_heightInput.image.sprite = m_buttonSprites[0];
        m_exitButton.image.sprite = m_buttonSprites[0];

        if(box == m_checkpointToggle)
        {
            m_onCheckpointBox = true;
            m_checkpointToggle.image.sprite = m_buttonSprites[1];
            m_scrollRect.verticalNormalizedPosition = 1.0f;
            m_index = 2;
        }
        else if(box == m_fullscreenToggle)
        {
            m_onFullscreenBox = true;
            m_fullscreenToggle.image.sprite = m_buttonSprites[1];
            m_scrollRect.verticalNormalizedPosition = 0.0f;
            m_index = 4;
        }

        //Debug.Log("index: " + m_index);
        m_eventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);
    }

    public void OnInputPressed(TMP_InputField input)
    {
        m_onExitButton = false;
        m_onBackgroundSlider = false;
        m_onSoundEffectsSlider = false;
        m_onCheckpointBox = false;
        m_onSensitivitySlider = false;
        m_onFullscreenBox = false;
        m_onResizeInputs = false;
        m_onHeight = false;
        m_backgroundMusicSlider.image.sprite = m_buttonSprites[2];
        m_soundEffectsSlider.image.sprite = m_buttonSprites[2];
        m_checkpointToggle.image.sprite = m_buttonSprites[0];
        m_sensitivitySlider.image.sprite = m_buttonSprites[2];
        m_fullscreenToggle.image.sprite = m_buttonSprites[0];
        m_exitButton.image.sprite = m_buttonSprites[0];

        if(input == m_widthInput && m_fullscreenToggle.isOn == false)
        {
            m_widthInput.image.sprite = m_buttonSprites[1];
            m_heightInput.image.sprite = m_buttonSprites[0];
        }
        else if(input == m_heightInput && m_fullscreenToggle.isOn == false)
        {
            m_widthInput.image.sprite = m_buttonSprites[0];
            m_heightInput.image.sprite = m_buttonSprites[1];
            m_onHeight = true;
        }

        m_index = 5;

        //Debug.Log("index: " + m_index);
        m_eventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);
    }

    private bool CheckIfNum(string text)
    {
        for(int i = 0; i < text.Length; i++)
        {
            if (Char.IsNumber(text[i]) == false)
            {
                return false;
            }
        }

        return true;
    }

    public void OnInputEntered(TMP_InputField input)
    {
        if(input == m_widthInput)
        {
            if (CheckIfNum(input.text) == false)
            {
                input.text = m_widthInputText;
            }
            else if(Convert.ToInt32(input.text) > m_maxWidth)
            {
                input.text = m_maxWidth.ToString();
            }

            m_widthInputText = input.text;
        }
        else if(input == m_heightInput)
        {
            if (CheckIfNum(input.text) == false)
            {
                input.text = m_heightInputText;
            }
            else if(Convert.ToInt32(input.text) > m_maxHeight)
            {
                input.text = m_maxHeight.ToString();
            }

            m_heightInputText = input.text;
        }
    }

    // Update is called once per frame
    void Update()
    {
        m_sensitivityValueText.text = m_sensitivitySlider.value.ToString();

        if(m_enabled && m_navInputs.WasPressedThisDynamicUpdate())
        {
            //Debug.Log(m_navInputs.ReadValue<Vector2>());

            if (m_navInputs.ReadValue<Vector2>() == Vector2.down && m_index < 6)
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
            else if (m_onSensitivitySlider)
            {
                if (m_navInputs.ReadValue<Vector2>() == Vector2.right && m_sensitivitySlider.value < m_sensitivitySlider.maxValue/* && m_sensitivity < m_sensitivitySlider.maxValue*/)
                {
                    m_sensitivitySlider.Select();
                    //m_sensitivity += m_sensitivityStep;
                    m_sensitivitySlider.value += 1.0f;
                }
                else if (m_navInputs.ReadValue<Vector2>() == Vector2.left && m_sensitivitySlider.value > m_sensitivitySlider.minValue/* && m_sensitivity > m_sensitivitySlider.minValue*/)
                {
                    m_sensitivitySlider.Select();
                    //m_sensitivity -= m_sensitivityStep;
                    m_sensitivitySlider.value -= 1.0f;
                }

                //m_sensitivity = (float)Math.Round(m_sensitivity, 2);
                //m_sensitivitySlider.value = Mathf.Lerp(m_sensitivitySlider.value, m_sensitivity,2 * Time.deltaTime);

                //Debug.Log(m_sensitivity);
            }
            else if (m_onResizeInputs)
            {
                if (m_navInputs.ReadValue<Vector2>() == Vector2.right)
                {
                    if (m_fullscreenToggle.isOn == false)
                    {
                        m_heightInput.image.sprite = m_buttonSprites[1];
                    }
                    m_widthInput.image.sprite = m_buttonSprites[0];
                    m_onHeight = true;
                }
                else if (m_navInputs.ReadValue<Vector2>() == Vector2.left)
                {
                    if (m_fullscreenToggle.isOn == false)
                    {
                        m_widthInput.image.sprite = m_buttonSprites[1];
                    }
                    m_heightInput.image.sprite = m_buttonSprites[0];
                    m_onHeight = false;
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
                    m_backgroundMusicSlider.image.sprite = m_buttonSprites[3];
                    m_soundEffectsSlider.image.sprite = m_buttonSprites[2];
                    m_exitButton.image.sprite = m_buttonSprites[0];
                    break;
                case 1:
                    m_onBackgroundSlider = false;
                    m_onSoundEffectsSlider = true;
                    m_onCheckpointBox = false;
                    m_backgroundMusicSlider.image.sprite = m_buttonSprites[2];
                    m_soundEffectsSlider.image.sprite = m_buttonSprites[3];
                    m_checkpointToggle.image.sprite = m_buttonSprites[0];
                    break;
                case 2:
                    m_onSoundEffectsSlider = false;
                    m_onCheckpointBox = true;
                    m_onSensitivitySlider = false;
                    m_soundEffectsSlider.image.sprite = m_buttonSprites[2];
                    m_checkpointToggle.image.sprite = m_buttonSprites[1];
                    m_sensitivitySlider.image.sprite = m_buttonSprites[2];
                    //m_scrollRect.verticalNormalizedPosition = 1.0f;
                    break;
                case 3:
                    m_onCheckpointBox = false;
                    m_onSensitivitySlider = true;
                    m_onFullscreenBox = false;
                    m_checkpointToggle.image.sprite = m_buttonSprites[0];
                    m_sensitivitySlider.image.sprite = m_buttonSprites[3];
                    m_fullscreenToggle.image.sprite = m_buttonSprites[0];
                    m_scrollRect.verticalNormalizedPosition = 1.0f;
                    break;
                case 4:
                    m_onSensitivitySlider = false;
                    m_onFullscreenBox = true;
                    m_onResizeInputs = false;
                    m_onHeight = false;
                    m_sensitivitySlider.image.sprite = m_buttonSprites[2];
                    m_fullscreenToggle.image.sprite = m_buttonSprites[1];
                    m_widthInput.image.sprite = m_buttonSprites[0];
                    m_heightInput.image.sprite = m_buttonSprites[0];
                    //m_exitButton.image.sprite = m_buttonSprites[0];
                    m_scrollRect.verticalNormalizedPosition = 0.0f;
                    break;
                case 5:
                    m_onFullscreenBox = false;
                    m_onResizeInputs = true;
                    m_onExitButton = false;
                    m_fullscreenToggle.image.sprite = m_buttonSprites[0];
                    m_exitButton.image.sprite = m_buttonSprites[0];
                    if (m_onHeight)
                    {
                        if(m_fullscreenToggle.isOn == false)
                            m_heightInput.image.sprite = m_buttonSprites[1];
                        m_heightInput.Select();
                    }
                    else
                    {
                        if(m_fullscreenToggle.isOn == false)
                            m_widthInput.image.sprite = m_buttonSprites[1];
                        m_widthInput.Select();
                    }
                    break;
                case 6:
                    m_onExitButton = true;
                    m_onResizeInputs = false;
                    m_onHeight = false;
                    m_widthInput.image.sprite = m_buttonSprites[0];
                    m_heightInput.image.sprite = m_buttonSprites[0];
                    m_exitButton.image.sprite = m_buttonSprites[1];
                    break;
            }

            //Debug.Log("index: " + m_index);
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