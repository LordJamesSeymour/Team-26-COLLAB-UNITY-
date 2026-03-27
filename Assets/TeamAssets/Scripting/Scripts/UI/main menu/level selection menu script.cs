using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class levelselectionmenuscript : menuscreenscript
{
    [SerializeField] Image m_playerIcon;
    [SerializeField] Button[] m_levelButtons;

    private RectTransform m_iconTransform;
    private int m_index = 0;
    private static bool m_run = false;
    private buttonnavscript m_buttonScript;

    datamanager m_manager;

    protected override void Awake()
    {
        base.Awake();

        m_manager = new datamanager(3);

        if(m_run == false)
        {
            m_run = true;
        }
        else
        {
            m_manager.LoadGameData();
        }

        if (m_manager.GetGameData().levels[m_index].levelNum == 0)
        {
            m_manager.SetLevelNum(m_index, m_index + 1);
            m_manager.SaveGameData();
        }

        m_currentButton = m_levelButtons[m_index];
        m_currentButton.image.sprite = m_buttonSprites[1];
        m_onExitButton = false;

        m_buttonScript = GetComponent<buttonnavscript>();
        if (!m_buttonScript)
            Debug.LogError("no buttonnavscript attached");

        m_iconTransform = m_playerIcon.GetComponent<RectTransform>();
        if (!m_iconTransform)
        {
            Debug.LogError("no rect transform attached");
        }
        else
        {
            m_iconTransform.position = m_levelButtons[m_index].GetComponent<RectTransform>().position;
            m_iconTransform.position += new Vector3(0, 60f, 0);
        }
    }

    private IEnumerator ToggleLevelsScreenOff()
    {
        m_buttonScript.m_levelsPanel.SetActive(false);
        m_buttonScript.m_mainMenuPanel.SetActive(true);

        if (m_currentButton != m_exitButton)
            m_currentButton.image.sprite = m_buttonSprites[0];
        else
            m_currentButton.image.sprite = m_buttonSprites[2];

        m_onExitButton = false;
        m_index = 0;
        m_currentButton = m_levelButtons[m_index];
        m_currentButton.image.sprite = m_buttonSprites[1];
        m_manager.SetLevelNum(m_index, m_index + 1);
        m_manager.SaveGameData();
        m_iconTransform.position = m_levelButtons[m_index].GetComponent<RectTransform>().position;
        m_iconTransform.position += new Vector3(0, 60f, 0);
        m_enabled = false;

        yield return new WaitUntil(() => m_buttonScript.m_mainMenuPanel.activeSelf == true);
        m_buttonScript.m_mainMenuPanelEnabled = true;
    }

    public void RunToggleLevelsOff()
    {
        StartCoroutine(ToggleLevelsScreenOff());
    }

    public void LoadLevel(int i)
    {
        SceneManager.LoadScene(i);
    }

    public void OnPointerPressed(int i)
    {
        if (m_currentButton == m_exitButton)
            m_currentButton.image.sprite = m_buttonSprites[2];
        else
            m_currentButton.image.sprite = m_buttonSprites[0];

        m_index = i;
        m_currentButton = m_levelButtons[m_index];
        m_currentButton.image.sprite = m_buttonSprites[1];
        m_manager.SetLevelNum(m_index, m_index + 1);
        m_manager.SaveGameData();
        m_onExitButton = false;
        m_iconTransform.position = m_currentButton.GetComponent<RectTransform>().position;
        m_iconTransform.position += new Vector3(0, 60f, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if(m_navInputs.WasPressedThisDynamicUpdate() && m_enabled)
        {
            if (m_navInputs.ReadValue<Vector2>() == Vector2.right && m_onExitButton == false && m_index < m_levelButtons.Length - 1)
            {
                m_index++;
            }
            else if (m_navInputs.ReadValue<Vector2>() == Vector2.left && m_onExitButton == false && m_index > 0)
            {
                m_index--;
            }
            else if (m_navInputs.ReadValue<Vector2>() == Vector2.down && m_onExitButton)
            {
                m_currentButton.image.sprite = m_buttonSprites[2];
                m_index = 0;
                m_currentButton = m_levelButtons[m_index];
                m_currentButton.image.sprite = m_buttonSprites[1];
                m_onExitButton = false;
            }
            else if (m_navInputs.ReadValue<Vector2>() == Vector2.up && m_onExitButton == false)
            {
                m_currentButton.image.sprite = m_buttonSprites[0];
                m_currentButton = m_exitButton;
                m_currentButton.image.sprite = m_buttonSprites[3];
                m_onExitButton = true;
            }
        }

        if (m_navInputs.WasReleasedThisDynamicUpdate() && m_enabled)
        {
            if (m_currentButton != null && m_onExitButton == false)
            {
                m_currentButton.image.sprite = m_buttonSprites[0];
                m_currentButton = m_levelButtons[m_index];
                m_currentButton.image.sprite = m_buttonSprites[1];
                m_manager.SetLevelNum(m_index, m_index + 1);
                m_manager.SaveGameData();
                m_iconTransform.position = m_currentButton.GetComponent<RectTransform>().position;
                m_iconTransform.position += new Vector3(0, 60f, 0);
            }
        }

        if(m_selectInput.WasReleasedThisDynamicUpdate() && m_enabled)
        {
            if (m_onExitButton)
            {
                m_exitButton.onClick.Invoke();
            }
            else
            {
                m_currentButton.onClick.Invoke();
            }
        }

        if (m_enabled)
        {
            m_manager.LoadGameData();
            //Debug.Log("JSON file level num: " + m_manager.GetGameData().levels[m_index].levelNum);
            Debug.Log("index: " + m_index);
            Debug.Log("completed: " + m_manager.GetGameData().levels[m_index].completed);
        }
    }
}
