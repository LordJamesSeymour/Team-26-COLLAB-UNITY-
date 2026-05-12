using System;
using System.Collections;
using TMPro;
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

    [SerializeField] private AudioMixer audioMixer;

    string m_effects = "EffectsVolume";
    string m_music = "MusicVolume";

    private Coroutine m_toggleMenu;
    private Coroutine m_toggleCheckpoint;
    private Coroutine m_changeCheckboxValue;

    private ScrollRect m_scrollRect;
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

    private int m_index = -1;
    private int m_maxWidth;
    private int m_maxHeight;

    private const int MinWindowWidth = 750;
    private const int MinWindowHeight = 750;

    GameObject m_eventSystem;

    protected override void Awake()
    {
        base.Awake();

        CacheDesktopResolution();

        m_manager = new datamanager(6);

        m_scrollRect = m_scrollArea.GetComponent<ScrollRect>();
        if (!m_scrollRect)
            Debug.LogError("no scroll rect on object");

        ApplyBorderlessFullscreen();

        m_fullscreenToggle.isOn = true;
        m_widthInput.interactable = false;
        m_heightInput.interactable = false;

        UpdateResolutionInputFields(m_maxWidth, m_maxHeight);

        m_onExitButton = false;
        m_eventSystem = GameObject.Find("EventSystem");

        m_buttonScript.m_settingsPanel.GetComponent<menuscreeneventsmanager>().IsVisible += SettingsVisible;
    }

    private void CacheDesktopResolution()
    {
        Resolution current = Screen.currentResolution;

        m_maxWidth = Mathf.Max(current.width, MinWindowWidth);
        m_maxHeight = Mathf.Max(current.height, MinWindowHeight);
    }

    private void ApplyBorderlessFullscreen()
    {
        Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        Screen.fullScreen = true;
    }

    private void ApplyWindowedResolution(int width, int height)
    {
        CacheDesktopResolution();

        width = Mathf.Clamp(width, MinWindowWidth, m_maxWidth);
        height = Mathf.Clamp(height, MinWindowHeight, m_maxHeight);

        Screen.fullScreenMode = FullScreenMode.Windowed;
        Screen.SetResolution(width, height, FullScreenMode.Windowed);

        UpdateResolutionInputFields(width, height);
    }

    private void UpdateResolutionInputFields(int width, int height)
    {
        m_widthInputText = width.ToString();
        m_heightInputText = height.ToString();

        if (m_widthInput != null)
            m_widthInput.text = m_widthInputText;

        if (m_heightInput != null)
            m_heightInput.text = m_heightInputText;
    }

    private int ReadInputFieldAsResolutionValue(TMP_InputField input, int fallback, int min, int max)
    {
        if (input == null)
            return fallback;

        if (string.IsNullOrWhiteSpace(input.text))
            return fallback;

        if (!int.TryParse(input.text, out int value))
            return fallback;

        return Mathf.Clamp(value, min, max);
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
        m_sensitivitySlider.value = m_manager.GetGameData().settings.sensitivity;

        float musicDB;
        audioMixer.GetFloat(m_music, out musicDB);

        float sfxDB;
        audioMixer.GetFloat(m_effects, out sfxDB);

        float musicVolume = 100 * Mathf.Pow(10, musicDB / 20);
        m_backgroundMusicSlider.value = musicVolume;

        float SFXVolume = 100 * Mathf.Pow(10, sfxDB / 20);
        m_soundEffectsSlider.value = SFXVolume;

        Debug.Log(m_manager.GetGameData().settings.backgroundMusicVolume);
    }

    private IEnumerator ToggleSettingsMenuOff()
    {
        if (m_buttonScript.m_settingsBackground != null)
            m_buttonScript.m_settingsBackground.SetActive(false);

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

        if (m_index != 6)
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

        yield return new WaitUntil(() => m_buttonScript.m_settingsPanel.activeSelf == false && m_buttonScript.m_menuPanel.activeSelf == true);
        yield return new WaitForSeconds(0.1f);

        m_buttonScript.m_enabled = true;
        m_toggleMenu = null;
    }

    public void RunToggleSettingsOff()
    {
        if (m_toggleMenu == null)
            m_toggleMenu = StartCoroutine(ToggleSettingsMenuOff());
    }

    public IEnumerator ToggleCheckpointsEnabled()
    {
        m_manager.SetCheckpointsEnabled(m_checkpointToggle.isOn);
        m_manager.SaveGameData();

        yield return new WaitForSeconds(0.1f);

        m_toggleCheckpoint = null;
    }

    public IEnumerator ChangeCheckboxValue(Toggle box)
    {
        box.isOn = !box.isOn;

        if (box == m_fullscreenToggle)
            MakeFullscreen();

        if (box == m_checkpointToggle)
            RunToggleCheckpoint();

        yield return new WaitForSeconds(0.1f);

        m_changeCheckboxValue = null;
    }

    public void RunToggleCheckpoint()
    {
        if (m_toggleCheckpoint == null)
            m_toggleCheckpoint = StartCoroutine(ToggleCheckpointsEnabled());
    }

    public void UpdateBackgroundVolume(AudioSource source)
    {
        float db = Mathf.Clamp(Mathf.Log10(Mathf.Max(0.0001f, m_backgroundMusicSlider.value / 100)) * 20, -80, 0);

        audioMixer.SetFloat(m_music, db);

        m_manager.SetBackgroundVolume(m_backgroundMusicSlider.value);
        m_manager.SaveGameData();

        Debug.Log(m_manager.GetGameData().settings.backgroundMusicVolume);

        source.Play();
    }

    public void UpdateSoundEffectsVolume(AudioSource source)
    {
        float db = Mathf.Clamp(Mathf.Log10(Mathf.Max(0.0001f, m_soundEffectsSlider.value / 100)) * 20, -80, 0);

        audioMixer.SetFloat(m_effects, db);

        m_manager.SetSoundEffectsVolume(m_soundEffectsSlider.value);
        m_manager.SaveGameData();

        Debug.Log(m_manager.GetGameData().settings.soundEffectsVolume);

        source.Play();
    }

    public void UpdateSensitivitySlider()
    {
        m_manager.SetSensitivity(m_sensitivitySlider.value);
        m_manager.SaveGameData();

        Debug.Log(m_manager.GetGameData().settings.sensitivity);
    }

    public void MakeFullscreen()
    {
        CacheDesktopResolution();

        if (m_fullscreenToggle.isOn)
        {
            m_widthInput.interactable = false;
            m_heightInput.interactable = false;

            ApplyBorderlessFullscreen();
            UpdateResolutionInputFields(m_maxWidth, m_maxHeight);
        }
        else
        {
            m_widthInput.interactable = true;
            m_heightInput.interactable = true;

            int targetWidth = ReadInputFieldAsResolutionValue(m_widthInput, Mathf.Min(1280, m_maxWidth), MinWindowWidth, m_maxWidth);
            int targetHeight = ReadInputFieldAsResolutionValue(m_heightInput, Mathf.Min(720, m_maxHeight), MinWindowHeight, m_maxHeight);

            ApplyWindowedResolution(targetWidth, targetHeight);
        }
    }

    public void ChangeScreenSize(TMP_InputField input)
    {
        if (m_fullscreenToggle.isOn)
            return;

        CacheDesktopResolution();

        int width = ReadInputFieldAsResolutionValue(m_widthInput, Screen.width, MinWindowWidth, m_maxWidth);
        int height = ReadInputFieldAsResolutionValue(m_heightInput, Screen.height, MinWindowHeight, m_maxHeight);

        if (input == m_widthInput)
        {
            width = ReadInputFieldAsResolutionValue(m_widthInput, width, MinWindowWidth, m_maxWidth);
        }
        else if (input == m_heightInput)
        {
            height = ReadInputFieldAsResolutionValue(m_heightInput, height, MinWindowHeight, m_maxHeight);
        }

        ApplyWindowedResolution(width, height);
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
        else if (slider == m_sensitivitySlider)
        {
            m_onSensitivitySlider = true;
            m_sensitivitySlider.image.sprite = m_buttonSprites[3];
            m_index = 3;
        }

        m_scrollRect.verticalNormalizedPosition = 1.0f;

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

        if (box == m_checkpointToggle)
        {
            m_onCheckpointBox = true;
            m_checkpointToggle.image.sprite = m_buttonSprites[1];
            m_scrollRect.verticalNormalizedPosition = 1.0f;
            m_index = 2;
        }
        else if (box == m_fullscreenToggle)
        {
            m_onFullscreenBox = true;
            m_fullscreenToggle.image.sprite = m_buttonSprites[1];
            m_scrollRect.verticalNormalizedPosition = 0.0f;
            m_index = 4;
        }

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

        if (input == m_widthInput && m_fullscreenToggle.isOn == false)
        {
            m_widthInput.image.sprite = m_buttonSprites[1];
            m_heightInput.image.sprite = m_buttonSprites[0];
            m_onHeight = false;
        }
        else if (input == m_heightInput && m_fullscreenToggle.isOn == false)
        {
            m_widthInput.image.sprite = m_buttonSprites[0];
            m_heightInput.image.sprite = m_buttonSprites[1];
            m_onHeight = true;
        }

        m_index = 5;

        m_eventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);
    }

    private bool CheckIfNum(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        for (int i = 0; i < text.Length; i++)
        {
            if (Char.IsNumber(text[i]) == false)
                return false;
        }

        return true;
    }

    public void OnInputEntered(TMP_InputField input)
    {
        CacheDesktopResolution();

        if (input == m_widthInput)
        {
            if (CheckIfNum(input.text) == false)
            {
                input.text = m_widthInputText;
            }
            else
            {
                int width = Mathf.Clamp(Convert.ToInt32(input.text), MinWindowWidth, m_maxWidth);
                input.text = width.ToString();
                m_widthInputText = input.text;
            }
        }
        else if (input == m_heightInput)
        {
            if (CheckIfNum(input.text) == false)
            {
                input.text = m_heightInputText;
            }
            else
            {
                int height = Mathf.Clamp(Convert.ToInt32(input.text), MinWindowHeight, m_maxHeight);
                input.text = height.ToString();
                m_heightInputText = input.text;
            }
        }
    }

    void Update()
    {
        if (m_sensitivityValueText != null)
            m_sensitivityValueText.text = m_sensitivitySlider.value.ToString();

        if (m_enabled && m_navInputs.WasPressedThisDynamicUpdate())
        {
            if (m_navInputs.ReadValue<Vector2>() == Vector2.down && m_index < 6)
            {
                m_index++;
            }
            else if (m_navInputs.ReadValue<Vector2>() == Vector2.up && m_index > 0)
            {
                m_index--;
            }

            if (m_onBackgroundSlider)
            {
                if (m_navInputs.ReadValue<Vector2>() == Vector2.right && m_backgroundMusicSlider.value < m_backgroundMusicSlider.maxValue)
                {
                    m_backgroundMusicSlider.Select();
                    m_backgroundMusicSlider.value += 1.0f;
                }
                else if (m_navInputs.ReadValue<Vector2>() == Vector2.left && m_backgroundMusicSlider.value > m_backgroundMusicSlider.minValue)
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
                if (m_navInputs.ReadValue<Vector2>() == Vector2.right && m_sensitivitySlider.value < m_sensitivitySlider.maxValue)
                {
                    m_sensitivitySlider.Select();
                    m_sensitivitySlider.value += 1.0f;
                }
                else if (m_navInputs.ReadValue<Vector2>() == Vector2.left && m_sensitivitySlider.value > m_sensitivitySlider.minValue)
                {
                    m_sensitivitySlider.Select();
                    m_sensitivitySlider.value -= 1.0f;
                }
            }
            else if (m_onResizeInputs)
            {
                if (m_navInputs.ReadValue<Vector2>() == Vector2.right)
                {
                    if (m_fullscreenToggle.isOn == false)
                        m_heightInput.image.sprite = m_buttonSprites[1];

                    m_widthInput.image.sprite = m_buttonSprites[0];
                    m_onHeight = true;
                }
                else if (m_navInputs.ReadValue<Vector2>() == Vector2.left)
                {
                    if (m_fullscreenToggle.isOn == false)
                        m_widthInput.image.sprite = m_buttonSprites[1];

                    m_heightInput.image.sprite = m_buttonSprites[0];
                    m_onHeight = false;
                }
            }
        }

        if (m_enabled && m_navInputs.WasReleasedThisDynamicUpdate())
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
                        if (m_fullscreenToggle.isOn == false)
                            m_heightInput.image.sprite = m_buttonSprites[1];

                        m_heightInput.Select();
                    }
                    else
                    {
                        if (m_fullscreenToggle.isOn == false)
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
        }

        if (m_enabled && m_selectInput.WasReleasedThisDynamicUpdate())
        {
            if (m_onExitButton)
            {
                RunToggleSettingsOff();
            }
            else if (m_onCheckpointBox)
            {
                if (m_changeCheckboxValue == null)
                    m_changeCheckboxValue = StartCoroutine(ChangeCheckboxValue(m_checkpointToggle));
            }
            else if (m_onFullscreenBox)
            {
                if (m_changeCheckboxValue == null)
                    m_changeCheckboxValue = StartCoroutine(ChangeCheckboxValue(m_fullscreenToggle));
            }
        }
    }
}