using System;
using Group26.Player.Camera;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager2 : MonoBehaviour
{
    [Header("Input References")]
    [Space(10)]

    private InputSystem_Actions playerInputActions;
	private PlayerLocomotion playerLocomotion;
    private CameraModeManager cameraMode;

    [Tooltip("Vector2 - WASD / Left Thumb Stick")]
    [SerializeField] private InputActionReference moveAction;

    [Tooltip("Vector2 - Mouse Delta / Right Thumb Stick")]
    [SerializeField] private InputActionReference lookAction;

    [Tooltip("Button - Jump")]
    [SerializeField] private InputActionReference jumpAction;

    [Tooltip("Button - Crouch")]
    [SerializeField] private InputActionReference crouchAction;

    [Tooltip("Button - Sprint")]
    [SerializeField] private InputActionReference sprintAction;

	[Tooltip("Button - Dash")]
    [SerializeField] private InputActionReference dashAction;

    [Tooltip("Button - Grapple")]
    [SerializeField] private InputActionReference grappleAction;

    [Tooltip("Button - Swing")]
    [SerializeField] private InputActionReference swingAction;

    [Tooltip("Button - CameraSwitch")]
    [SerializeField] private InputActionReference cameraSwitchAction;

    [Tooltip("Button - Pausing game and triggering UI event")]
    [SerializeField] private InputActionReference pauseAction;

    [HideInInspector] public Vector2 MoveInput { get; private set; }
    [HideInInspector] public Vector2 LookInput { get; private set; }
    
    [HideInInspector] public bool canGrapple;

    [HideInInspector] public bool isSprinting { get; set; }
    [HideInInspector] public bool isCrouching { get; set; }

    public event Action OnJumpPressed;
	public event Action OnDashPressed;
    public event Action OnGrapplePressed;
    public event Action OnCameraSwitchPressed;
    public event Action OnPausePressed;

    void Awake()
    {
        if (playerInputActions == null) playerInputActions = new InputSystem_Actions();
		if (playerLocomotion == null) playerLocomotion = GetComponent<PlayerLocomotion>();
		if (cameraMode == null) cameraMode = GetComponent<CameraModeManager>();
	}

    void OnEnable()
    {
        SubToPlayerControls();
    }

    void OnDisable()
    {
        UnsubFromPlayerControls();
    }

    private void Update()
    {
        MoveInput = ReadVector2(moveAction);
        LookInput = ReadVector2(lookAction);
    }

    private void SubToPlayerControls()
    {
        playerInputActions.Enable();

        SubscribePerformed(jumpAction, HandleJump);
        SubscribePerformed(grappleAction, HandleInteract);
        SubscribePerformed(dashAction, HandleDash);
        SubscribePerformed(cameraSwitchAction, HandleCameraSwitch);
        SubscribePerformed(pauseAction, HandlePause);

        SubscribeToggled(sprintAction, HandleSprintChanged);
        SubscribeToggled(crouchAction, HandleCrouchChanged);
    }

    private void UnsubFromPlayerControls()
    {
        playerInputActions.Disable();

        UnsubscribePerformed(jumpAction, HandleJump);
        UnsubscribePerformed(grappleAction, HandleInteract);
        UnsubscribePerformed(dashAction, HandleDash);
        UnsubscribePerformed(cameraSwitchAction, HandleCameraSwitch);
        UnsubscribePerformed(pauseAction, HandlePause);

        UnsubscribeToggled(sprintAction, HandleSprintChanged);
        UnsubscribeToggled(crouchAction, HandleCrouchChanged);
    }

    private static Vector2 ReadVector2(InputActionReference reference)
    {
        return reference != null && reference.action != null ? reference.action.ReadValue<Vector2>() : Vector2.zero;
    }

    private void HandleJump(InputAction.CallbackContext context)
    {
		OnJumpPressed?.Invoke();
    }

    private void HandleInteract(InputAction.CallbackContext context)
    {
        OnGrapplePressed?.Invoke();
    }

	private void HandleDash(InputAction.CallbackContext context)
	{
		OnDashPressed?.Invoke();
	}

    private void HandleCameraSwitch(InputAction.CallbackContext context)
    {
        OnCameraSwitchPressed?.Invoke();
    }

    private void HandlePause(InputAction.CallbackContext context)
    {
        OnPausePressed?.Invoke();
    }

    private void HandleSprintChanged(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            isSprinting = true;
        }
        else if(context.canceled)
        {
            isSprinting = false;
        }
    }

    private void HandleCrouchChanged(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isCrouching = true;
        }
        else if (context.canceled)
        {
            isCrouching = false;
        }
    }

    private static void SubscribePerformed(InputActionReference reference, Action<InputAction.CallbackContext> actionHandler)
    {
        if(reference == null || reference.action == null) return;
        reference.action.performed += actionHandler;
    }

    private static void UnsubscribePerformed(InputActionReference reference, Action<InputAction.CallbackContext> actionHandler)
    {
        if (reference == null || reference.action == null) return;
        reference.action.performed -= actionHandler;
    }

    private static void SubscribeToggled(InputActionReference reference, Action<InputAction.CallbackContext> actionHandler)
    {
        if (reference == null || reference.action == null) return;
        reference.action.performed += actionHandler;
        reference.action.canceled += actionHandler;
    }

    private static void UnsubscribeToggled(InputActionReference reference, Action<InputAction.CallbackContext> actionHandler)
    {
        if (reference == null || reference.action == null) return;
        reference.action.performed -= actionHandler;
        reference.action.canceled -= actionHandler;
    }
}