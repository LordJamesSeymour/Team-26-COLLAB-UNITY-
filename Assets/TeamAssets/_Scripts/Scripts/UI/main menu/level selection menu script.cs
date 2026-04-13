using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class levelselectionmenuscript : menuscreenscript
{
    [SerializeField] Image m_playerIcon;
    [SerializeField] Button[] m_levelButtons;
    [SerializeField] GameObject m_contentPanel;
    [SerializeField] GameObject m_scrollObject;
    [SerializeField] float m_scrollAmount;

    private RectTransform m_iconTransform;
    private int m_index = 0;
    private static bool m_run = false;
    private ScrollRect m_scrollRect;
    //private buttonnavscript m_buttonScript;

    datamanager m_manager;
    GameObject m_eventSystem;

    protected override void Awake()
    {
        base.Awake();

        m_manager = new datamanager(6);

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

        m_eventSystem = GameObject.Find("EventSystem");

        m_currentButton = m_levelButtons[m_index];
        m_currentButton.image.sprite = m_buttonSprites[1];
        m_onExitButton = false;

        m_manager.SetLocked(2, true);
        m_manager.SaveGameData();

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

        m_scrollRect = m_scrollObject.GetComponent<ScrollRect>();
        if (!m_scrollRect)
            Debug.LogError("no scroll rect");

        for(int i = 0; i < m_levelButtons.Length; i++)
        {
            if (m_manager.GetGameData().levels[i].completed)
            {
                m_levelButtons[i].image.sprite = m_buttonSprites[2];
            }
            else if (m_manager.GetGameData().levels[i].locked)
            {
                m_levelButtons[i].image.sprite = m_buttonSprites[3];
            }
        }
    }

    private IEnumerator ToggleLevelsScreenOff()
    {
        m_buttonScript.m_levelsPanel.SetActive(false);
        m_buttonScript.m_mainMenuPanel.SetActive(true);

        if (m_currentButton != m_exitButton)
        {
            if (m_manager.GetGameData().levels[m_index].completed == false && m_manager.GetGameData().levels[m_index].locked == false)
            {
                m_currentButton.image.sprite = m_buttonSprites[0];
            }
            //m_currentButton.image.sprite = m_buttonSprites[0];
        }
        else
            m_currentButton.image.sprite = m_buttonSprites[4];

        m_onExitButton = false;
        m_index = 0;
        m_currentButton = m_levelButtons[m_index];

        if (m_manager.GetGameData().levels[m_index].completed == false && m_manager.GetGameData().levels[m_index].locked == false)
        {
            m_currentButton.image.sprite = m_buttonSprites[1];
        }
        //m_currentButton.image.sprite = m_buttonSprites[1];
        m_manager.SetLevelNum(m_index, m_index + 1);
        m_manager.SaveGameData();
        m_iconTransform.position = m_levelButtons[m_index].GetComponent<RectTransform>().position;
        m_iconTransform.position += new Vector3(0, 60f, 0);
        m_enabled = false;

        m_scrollRect.horizontalNormalizedPosition = 0f;

        m_eventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);

        yield return new WaitUntil(() => m_buttonScript.m_mainMenuPanel.activeSelf == true);
        yield return new WaitForSeconds(0.1f);
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
            m_currentButton.image.sprite = m_buttonSprites[4];
        else
        {
            if (m_manager.GetGameData().levels[m_index].completed == false && m_manager.GetGameData().levels[m_index].locked == false)
            {
                m_currentButton.image.sprite = m_buttonSprites[0];
            }
        }

        m_index = i;
        m_currentButton = m_levelButtons[m_index];

        if (m_manager.GetGameData().levels[m_index].completed == false && m_manager.GetGameData().levels[m_index].locked == false)
        {
            m_currentButton.image.sprite = m_buttonSprites[1];
        }

        m_manager.SetLevelNum(m_index, m_index + 1);
        m_manager.SaveGameData();
        m_onExitButton = false;
        m_iconTransform.position = m_currentButton.GetComponent<RectTransform>().position;
        m_iconTransform.position += new Vector3(0, 60f, 0);
    }

    bool CheckIfOnScreen(Button button)
    {
        //Vector3 buttonScreenPos = Camera.main.WorldToScreenPoint(button.transform.position);
        //CanvasRenderer levelPanelRenderer = m_buttonScript.m_levelsPanel.GetComponent<CanvasRenderer>();
        RectTransform levelsPanelRectTransform = m_buttonScript.m_levelsPanel.GetComponent<RectTransform>();
        if (levelsPanelRectTransform != null)
        {
            //Debug.Log("rect transform attached");
            //Debug.Log("width: " + levelsPanelRectTransform.rect.width);
            //Debug.Log("height: " + levelsPanelRectTransform.rect.height);
            float distX = Vector3.Distance(new Vector3(levelsPanelRectTransform.rect.width / 2, 0f, 0f), new Vector3(button.transform.position.x, 0f, 0f));
            float distY = Vector3.Distance(new Vector3(0f, levelsPanelRectTransform.rect.height / 2, 0f), new Vector3(0f, button.transform.position.y, 0f));
            //Debug.Log("x distance: " + distX);
            //Debug.Log("y distance: " + distY);

            if (distX > levelsPanelRectTransform.rect.width / 2 || distY > levelsPanelRectTransform.rect.height / 2)
            {
                //Debug.Log("invisible");
                return false;
            }
        }
        else
            Debug.Log("no rect transform");

        return true;
    }

    // Update is called once per frame
    void Update()
    {
        if(m_navInputs.WasPressedThisDynamicUpdate() && m_enabled)
        {
            if (m_navInputs.ReadValue<Vector2>() == Vector2.right && m_onExitButton == false && m_index < m_levelButtons.Length - 1)
            {
                if (m_manager.GetGameData().levels[m_index].completed == false && m_manager.GetGameData().levels[m_index].locked == false)
                {
                    m_currentButton.image.sprite = m_buttonSprites[0];
                }

                m_index++;
                m_currentButton = m_levelButtons[m_index];

                if (CheckIfOnScreen(m_currentButton) == false)
                {
                    //ScrollRect scrollRect = m_scrollObject.GetComponent<ScrollRect>();
                    if(m_currentButton.transform.position.x < m_buttonScript.m_levelsPanel.transform.position.x)
                    {
                        m_scrollRect.horizontalNormalizedPosition -= m_scrollAmount;
                    }
                    else
                    {
                        m_scrollRect.horizontalNormalizedPosition += m_scrollAmount;
                    }
                }
            }
            else if (m_navInputs.ReadValue<Vector2>() == Vector2.left && m_onExitButton == false && m_index > 0)
            {
                if (m_manager.GetGameData().levels[m_index].completed == false && m_manager.GetGameData().levels[m_index].locked == false)
                {
                    m_currentButton.image.sprite = m_buttonSprites[0];
                }

                m_index--;
                m_currentButton = m_levelButtons[m_index];

                if (CheckIfOnScreen(m_currentButton) == false)
                {
                    //ScrollRect scrollRect = m_scrollObject.GetComponent<ScrollRect>();
                    if (m_currentButton.transform.position.x > m_buttonScript.m_levelsPanel.transform.position.x)
                    {    
                        m_scrollRect.horizontalNormalizedPosition += m_scrollAmount;
                    }
                    else
                    {
                        m_scrollRect.horizontalNormalizedPosition -= m_scrollAmount;
                    }
                    //ScrollRect scrollRect = m_scrollObject.GetComponent<ScrollRect>();
                }
            }
            else if (m_navInputs.ReadValue<Vector2>() == Vector2.down && m_onExitButton)
            {
                m_currentButton.image.sprite = m_buttonSprites[4];
                m_index = 0;
                m_currentButton = m_levelButtons[m_index];

                if (m_manager.GetGameData().levels[m_index].completed == false && m_manager.GetGameData().levels[m_index].locked == false)
                {
                    m_currentButton.image.sprite = m_buttonSprites[1];
                }
                //m_currentButton.image.sprite = m_buttonSprites[1];
                m_onExitButton = false;
            }
            else if (m_navInputs.ReadValue<Vector2>() == Vector2.up && m_onExitButton == false)
            {
                if (m_manager.GetGameData().levels[m_index].completed == false && m_manager.GetGameData().levels[m_index].locked == false)
                {
                    m_currentButton.image.sprite = m_buttonSprites[0];
                }
                //m_currentButton.image.sprite = m_buttonSprites[0];
                m_currentButton = m_exitButton;
                m_currentButton.image.sprite = m_buttonSprites[5];
                m_onExitButton = true;
            }
        }

        if (m_navInputs.WasReleasedThisDynamicUpdate() && m_enabled)
        {
            if (m_currentButton != null && m_onExitButton == false)
            {
                m_currentButton = m_levelButtons[m_index];

                if (m_manager.GetGameData().levels[m_index].completed == false && m_manager.GetGameData().levels[m_index].locked == false)
                {
                    m_currentButton.image.sprite = m_buttonSprites[1];
                }

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
            //Debug.Log("index: " + m_index);
            //Debug.Log("completed: " + m_manager.GetGameData().levels[m_index].completed);
        }
    }
}