using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private InputSystem_Actions m_playerInputActions;

	[SerializeField] PlayerLocomotion m_playerLocomotion;
	[SerializeField] CameraLook m_cameraLook;
    [SerializeField] GrapplePrototype m_grapplePrototype;

    [HideInInspector] public Vector2 m_moveInput;
    [HideInInspector] public Vector2 m_lookInput;
    [HideInInspector] public bool m_bjumpPressed;
    [HideInInspector] public bool m_bcanInteract;

    public event Action GrappleAction;


    void Awake()
    {
        m_playerInputActions = new InputSystem_Actions();

		if (m_playerLocomotion == null)
		{
			m_playerLocomotion = GetComponent<PlayerLocomotion>();
		}

		if (m_cameraLook == null)
		{
			Debug.LogError("No camera look script has been asigned!");
		}

        m_grapplePrototype = GetComponent<GrapplePrototype>();
        if (m_grapplePrototype == null) {

            Debug.LogError("No grapple prototype script has been assigned");
        }
	}

    void OnEnable()
    {
        SubToPlayerControls();
    }

    void OnDisable()
    {
        m_playerInputActions.Disable();
    }


    #region Player Controls
    private void SubToPlayerControls()
    {
        m_playerInputActions.Enable();

        
		m_playerInputActions.Player.Move.performed += context => m_playerLocomotion.PlayerInput(context.ReadValue<Vector2>());
        m_playerInputActions.Player.Move.canceled += context => m_playerLocomotion.PlayerInput(Vector2.zero);
		//playerInputActions.Player.Move.performed += context => Debug.Log(context.ReadValue<Vector2>());


		m_playerInputActions.Player.Look.performed += context => m_cameraLook.PlayerInput(context.ReadValue<Vector2>());
        m_playerInputActions.Player.Look.canceled += context => m_cameraLook.PlayerInput(Vector2.zero);
		//playerInputActions.Player.Look.performed += context => Debug.Log(context.ReadValue<Vector2>());

		m_playerInputActions.Player.Jump.performed += Jump;
        m_playerInputActions.Player.Jump.canceled += JumpCanceled;

        m_playerInputActions.Player.Interact.performed += Interact;

        m_playerInputActions.Player.Grapple.performed += GrapplePerformed;
    }

    private void GrapplePerformed(InputAction.CallbackContext context)
    {
        GrappleAction.Invoke();
    }

    private void Jump(InputAction.CallbackContext context)
    {
		m_playerLocomotion.PlayerJump();
	}

    private void JumpCanceled(InputAction.CallbackContext context)
    {
        //TO DO:
    }

    private void Interact(InputAction.CallbackContext context)
    {
        //TO DO:
    }


    // IN CASE WE NEED TO CHANGE THE CONTROL SCHEME.
    private void UnsubFromPlayerControls()
    {
        Debug.Log("Action mode now in PC controls");

        m_playerInputActions.Player.Move.performed -= context => m_moveInput = context.ReadValue<Vector2>();
        m_playerInputActions.Player.Move.canceled -= context => m_moveInput = Vector2.zero;

        m_playerInputActions.Player.Look.performed -= context => m_lookInput = context.ReadValue<Vector2>();
        m_playerInputActions.Player.Look.canceled -= context => m_lookInput = Vector2.zero;

        m_playerInputActions.Player.Jump.performed -= Jump;
        m_playerInputActions.Player.Jump.canceled -= JumpCanceled;

        m_playerInputActions.Player.Interact.performed -= Interact;
    }
    #endregion
}