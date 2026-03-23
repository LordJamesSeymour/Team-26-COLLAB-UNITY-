using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class controlsmenuscript : MonoBehaviour
{
    private InputAction m_navInputs;
    private bool m_onExitButton = false;
    //private int m_currentButtonInt;

    public bool m_enabled;

    [SerializeField] Image m_controllerControlsImage;
    [SerializeField] Image m_keyboardControlsImage;
    [SerializeField] Button m_keyboardButton;
    [SerializeField] Button m_controllerButton;
    [SerializeField] Button m_exitButton;
    [SerializeField] Sprite[] m_buttonSprites;

    private void Awake()
    {
        m_navInputs = InputSystem.actions.FindAction("Navigate");
        m_keyboardButton.image.sprite = m_buttonSprites[1];
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
                m_exitButton.image.sprite = m_buttonSprites[0];
                m_controllerButton.image.sprite = m_buttonSprites[0];
                m_onExitButton = false;
                break;
            case 1:
                //if the controller button is pressed
                m_controllerButton.image.sprite = m_buttonSprites[1];
                m_exitButton.image.sprite = m_buttonSprites[0];
                m_keyboardButton.image.sprite = m_buttonSprites[0];
                m_onExitButton = false;
                break;
            case 2:
                //if the exit button is pressed
                m_exitButton.image.sprite = m_buttonSprites[1];
                m_controllerButton.image.sprite = m_buttonSprites[0];
                m_keyboardButton.image.sprite = m_buttonSprites[0];
                m_onExitButton = true;
                break;
        }
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
                //m_currentButtonInt = 1;
            }
            else if(m_navInputs.ReadValue<Vector2>() == Vector2.left && m_onExitButton == false)
            {
                m_controllerButton.image.sprite = m_buttonSprites[0];
                m_exitButton.image.sprite = m_buttonSprites[0];
                m_keyboardButton.image.sprite = m_buttonSprites[1];
                //m_currentButtonInt = 0;
            }
            else if(m_navInputs.ReadValue<Vector2>() == Vector2.down)
            {
                m_keyboardButton.image.sprite = m_buttonSprites[0];
                m_controllerButton.image.sprite = m_buttonSprites[0];
                m_exitButton.image.sprite = m_buttonSprites[1];
                m_onExitButton = true;
            }
            else if(m_navInputs.ReadValue<Vector2>() == Vector2.up && m_onExitButton)
            {
                m_controllerButton.image.sprite = m_buttonSprites[0];
                m_exitButton.image.sprite = m_buttonSprites[0];
                m_keyboardButton.image.sprite = m_buttonSprites[1];
                m_onExitButton = false;
            }
        }
    }
}
