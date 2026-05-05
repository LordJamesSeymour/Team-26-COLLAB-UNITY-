using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class levelcompletescript : MonoBehaviour
{
    [HideInInspector] public bool m_enabled = false;
    [SerializeField] protected Sprite[] m_buttonSprites;
    [SerializeField] TextMeshProUGUI m_uiTexts;

    private InputAction m_navInputs;
    private InputAction m_selectInput;

    private void Awake()
    {
        m_navInputs = InputSystem.actions.FindAction("Navigate");
        m_selectInput = InputSystem.actions.FindAction("Select");
    }

    private void OnEnable()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(m_navInputs.WasPressedThisDynamicUpdate() && m_enabled)
        {

        }
    }
}
