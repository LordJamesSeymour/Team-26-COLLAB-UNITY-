using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class menuscreenscript : MonoBehaviour
{
    [HideInInspector] public bool m_enabled = false;
    [SerializeField] protected Sprite[] m_buttonSprites;
    [SerializeField] protected Button m_exitButton;

    protected bool m_onExitButton = false;
    protected InputAction m_navInputs;
    protected InputAction m_selectInput;
    protected Button m_currentButton;

    protected virtual void Awake()
    {
        m_navInputs = InputSystem.actions.FindAction("Navigate");
        m_selectInput = InputSystem.actions.FindAction("Select");
    }
}
