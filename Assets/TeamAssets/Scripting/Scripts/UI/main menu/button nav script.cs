using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class buttonnavscript : MonoBehaviour
{
    private InputAction m_navInputs;
    private int m_index = 0;
    private Button m_currentButton;
    private Color m_normalColour;
    private Color m_highlightedColour;
    private ColorBlock m_buttonColorBlock;
    private controlsmenuscript m_controlsScreenScript;
    //private Vector2 direction;

    [SerializeField] Button[] m_buttons;
    [SerializeField] Sprite[] m_buttonSprites;

    private void Awake()
    {
        m_navInputs = InputSystem.actions.FindAction("Navigate");

        m_controlsScreenScript = GetComponent<controlsmenuscript>();
        if (!m_controlsScreenScript)
            Debug.Log("no controls screen script attached");
        //m_normalColour = m_buttons[0].colors.normalColor;
        //m_highlightedColour = m_buttons[0].colors.highlightedColor;
        //Debug.Log(m_highlightedColour);
        //m_buttonColorBlock.normalColor = m_highlightedColour;
        //Debug.Log(m_buttonColorBlock.normalColor);
        //m_currentButton = m_buttons[m_index];
        //m_currentButton.colors = m_buttonColorBlock;

        m_currentButton = m_buttons[m_index];
        m_currentButton.image.sprite = m_buttonSprites[1];
    }

    public void OnPointerEnter(int i)
    {
        //m_buttonColorBlock.normalColor = m_normalColour;
        //m_currentButton.colors = m_buttonColorBlock;
        m_currentButton.image.sprite = m_buttonSprites[0];

        if (i > 0 && i < m_buttons.Length)
        {
            m_index = i;
        }
        else
        {
            m_index = 0;
        }

        m_currentButton = m_buttons[m_index];
    }

    public void OnPointerExit()
    {
        m_currentButton.image.sprite = m_buttonSprites[1];
        //m_buttonColorBlock.normalColor = m_highlightedColour;
        //m_currentButton.colors = m_buttonColorBlock;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_navInputs.WasPressedThisDynamicUpdate())
        {
            Vector2 direction = m_navInputs.ReadValue<Vector2>();
            Debug.Log(direction);

            if(direction == Vector2.up && m_index > 0)
            {
                m_index--;
            }
            else if(direction == Vector2.down && m_index < m_buttons.Length - 1)
            {
                m_index++;
            }
            Debug.Log("Index: " + m_index);
        }

        if (m_navInputs.WasReleasedThisDynamicUpdate())
        {
            if(m_currentButton != null)
            {
                m_currentButton.image.sprite = m_buttonSprites[0];
                m_currentButton = m_buttons[m_index];
                m_currentButton.image.sprite = m_buttonSprites[1];

                //m_buttonColorBlock.normalColor = m_normalColour;
                //m_currentButton.colors = m_buttonColorBlock;
                //m_buttonColorBlock.normalColor = m_highlightedColour;
                //m_currentButton = m_buttons[m_index];
                //m_currentButton.colors = m_buttonColorBlock;
            }      
        }
    }
}
