using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class controlsmenuscript : menuscreenscript
{
    //private InputAction m_navInputs;
    //private InputAction m_selectInput;
    //private bool m_onExitButton = false;
    private Button m_tempButton;
    //private Button m_currentButton;
    //private buttonnavscript m_buttonScript;
    private Coroutine m_toggleOff;

    //[HideInInspector] public bool m_enabled = false;

    [SerializeField] Image m_controlsImage;
    [SerializeField] Sprite m_controllerControlsPicture;
    [SerializeField] Sprite m_keyboardControlsPicture;
    [SerializeField] Button m_keyboardButton;
    [SerializeField] Button m_controllerButton;
    //[SerializeField] Button m_exitButton;
    //[SerializeField] Sprite[] m_buttonSprites;

    protected override void Awake()
    {
        base.Awake();

        //m_buttonScript = GetComponent<buttonnavscript>();
        //if (!m_buttonScript)
        //    Debug.LogError("no button script attached");

        m_keyboardButton.image.sprite = m_buttonSprites[1];
        m_currentButton = m_keyboardButton;
        m_controlsImage.sprite = m_keyboardControlsPicture;
    }

    public IEnumerator ToggleControlsMenuOff()
    {
        m_buttonScript.m_controlsPanel.SetActive(false);
        m_buttonScript.m_mainMenuPanel.SetActive(true);
        m_exitButton.image.sprite = m_buttonSprites[0];
        m_controllerButton.image.sprite = m_buttonSprites[0];
        m_keyboardButton.image.sprite = m_buttonSprites[1];
        m_controlsImage.sprite = m_keyboardControlsPicture;
        m_currentButton = m_keyboardButton;
        m_onExitButton = false;
        m_enabled = false;

        yield return new WaitUntil(() => m_buttonScript.m_mainMenuPanel.activeSelf == true && m_buttonScript.m_controlsPanel.activeSelf == false);
        yield return new WaitForSeconds(0.1f);
        m_buttonScript.m_mainMenuPanelEnabled = true;
        m_toggleOff = null;
        StopCoroutine(ToggleControlsMenuOff());
    }

    public void RunToggleMenuOff()
    {
        if(m_toggleOff == null)
            m_toggleOff = StartCoroutine(ToggleControlsMenuOff());
    }

    //public void OnPointerEnter(int i)
    //{
    //    if (m_currentButtonInt != i)
    //    {
    //        switch (i)
    //        {
    //            case 0:
    //                m_controllerButton.image.sprite = m_buttonSprites[0];
    //                break;
    //            case 1:
    //                m_keyboardButton.image.sprite = m_buttonSprites[0];
    //                break;
    //        }

    //        m_currentButtonInt = i;
    //    }
    //    //m_controllerButton.image.sprite = m_buttonSprites[0];
    //    //m_keyboardButton.image.sprite = m_buttonSprites[0];
    //}

    public void OnPointerPressed(int i)
    {
        switch (i)
        {
            case 0:
                //if the keyboard button is pressed
                m_keyboardButton.image.sprite = m_buttonSprites[1];
                m_controllerButton.image.sprite = m_buttonSprites[0];
                m_currentButton = m_keyboardButton;
                m_controlsImage.sprite = m_keyboardControlsPicture;
                break;
            case 1:
                //if the controller button is pressed
                m_controllerButton.image.sprite = m_buttonSprites[1];
                m_keyboardButton.image.sprite = m_buttonSprites[0];
                m_currentButton = m_controllerButton;
                m_controlsImage.sprite = m_controllerControlsPicture;
                break;
            default:
                break;
        }

        m_exitButton.image.sprite = m_buttonSprites[0];
        m_onExitButton = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(m_enabled && m_navInputs.WasPressedThisDynamicUpdate())
        {
            if(m_navInputs.ReadValue<Vector2>() == Vector2.right && m_onExitButton == false)
            {
                m_keyboardButton.image.sprite = m_buttonSprites[0];
                m_exitButton.image.sprite = m_buttonSprites[0];
                m_controllerButton.image.sprite = m_buttonSprites[1];
                m_currentButton = m_controllerButton;
                m_controlsImage.sprite = m_controllerControlsPicture;
            }
            else if(m_navInputs.ReadValue<Vector2>() == Vector2.left && m_onExitButton == false)
            {
                m_controllerButton.image.sprite = m_buttonSprites[0];
                m_exitButton.image.sprite = m_buttonSprites[0];
                m_keyboardButton.image.sprite = m_buttonSprites[1];
                m_currentButton = m_keyboardButton;
                m_controlsImage.sprite = m_keyboardControlsPicture;
            }
            else if(m_navInputs.ReadValue<Vector2>() == Vector2.down)
            {
                m_keyboardButton.image.sprite = m_buttonSprites[0];
                m_controllerButton.image.sprite = m_buttonSprites[0];
                m_exitButton.image.sprite = m_buttonSprites[1];
                m_onExitButton = true;
                m_tempButton = m_currentButton;
                m_currentButton = m_exitButton;
            }
            else if(m_navInputs.ReadValue<Vector2>() == Vector2.up && m_onExitButton)
            {
                m_exitButton.image.sprite = m_buttonSprites[0];
                m_onExitButton = false;
                m_currentButton = m_tempButton;
                m_currentButton.image.sprite = m_buttonSprites[1];
            }
        }

        if(m_enabled && m_selectInput.WasReleasedThisDynamicUpdate() && m_onExitButton)
        {
            //Debug.Log("exiting");

            RunToggleMenuOff();
        }
    }
}
