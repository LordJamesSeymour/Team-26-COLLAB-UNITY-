using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class levelselectionmenuscript : MonoBehaviour
{
    [SerializeField] Image m_playerIcon;
    [SerializeField] Button m_backButton;
    [SerializeField] Button[] m_levelButtons;
    [SerializeField] Sprite[] m_buttonSprites;
    [SerializeField] Vector2[] m_points;

    private RectTransform m_iconTransform;
    private int m_index = 0;
    private bool m_onBackButton;
    private Button m_currentButton;
    private InputAction m_navInputs;
    private InputAction m_selectInput;

    private void Awake()
    {
        m_navInputs = InputSystem.actions.FindAction("Navigate");
        m_selectInput = InputSystem.actions.FindAction("Select");

        m_currentButton = m_backButton;
        m_currentButton.image.sprite = m_buttonSprites[3];

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

    public void OnPointerPressed(int i)
    {
        if (m_currentButton == m_backButton)
            m_currentButton.image.sprite = m_buttonSprites[2];
        else
            m_currentButton.image.sprite = m_buttonSprites[0];

        m_currentButton = m_levelButtons[i];
        m_currentButton.image.sprite = m_buttonSprites[1];
        m_iconTransform.position = m_currentButton.GetComponent<RectTransform>().position;
        m_iconTransform.position += new Vector3(0, 60f, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if(m_navInputs.WasPressedThisDynamicUpdate())
        {
            if (m_navInputs.ReadValue<Vector2>() == Vector2.right && m_onBackButton == false)
            {
                m_index++;
            }
            else if (m_navInputs.ReadValue<Vector2>() == Vector2.left && m_onBackButton == false)
            {
                m_index--;
            }
            else if (m_navInputs.ReadValue<Vector2>() == Vector2.down && m_onBackButton)
            {
                m_currentButton.image.sprite = m_buttonSprites[2];
                m_index = 0;
                m_currentButton = m_levelButtons[m_index];
                m_currentButton.image.sprite = m_buttonSprites[1];
                m_onBackButton = false;
            }
            else if (m_navInputs.ReadValue<Vector2>() == Vector2.up && m_onBackButton == false)
            {
                m_currentButton.image.sprite = m_buttonSprites[0];
                m_currentButton = m_backButton;
                m_currentButton.image.sprite = m_buttonSprites[3];
                m_onBackButton = true;
            }
        }
    }
}
