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
    protected buttonnavscript m_buttonScript;
    public static bool m_run;

    protected virtual void Awake()
    {
        m_navInputs = InputSystem.actions.FindAction("Navigate");
        m_selectInput = InputSystem.actions.FindAction("Select");

        m_buttonScript = GetComponent<buttonnavscript>();
        if (!m_buttonScript)
            Debug.LogError("no buttonnavscript attached");
    }
}
