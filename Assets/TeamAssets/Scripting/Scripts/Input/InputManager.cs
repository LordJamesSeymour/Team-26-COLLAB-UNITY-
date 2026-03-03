using System;
using Group26.Player.Camera;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
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

    [Tooltip("Button - Interact")]
    [SerializeField] private InputActionReference interactAction;

    [Tooltip("Button - Pausing game and triggering UI event")]
    [SerializeField] private InputActionReference pauseAction;

    [HideInInspector] public Vector2 MoveInput { get; private set; }
    [HideInInspector] public Vector2 LookInput { get; private set; }
    
    [HideInInspector] public bool canInteract;

    public event Action OnJumpPressed;
    public event Action OnInteractPressed;

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
        SubscribePerformed(interactAction, HandleInteract);

        SubscribePerformed(jumpAction, HandleJump);
        SubscribePerformed(jumpAction, HandleJump);
        SubscribePerformed(jumpAction, HandleJump);
    }

    private void UnsubFromPlayerControls()
    {
        playerInputActions.Disable();

        UnsubscribePerformed(jumpAction, HandleJump);
        UnsubscribePerformed(interactAction, HandleInteract);

        UnsubscribePerformed(jumpAction, HandleJump);
        UnsubscribePerformed(jumpAction, HandleJump);
        UnsubscribePerformed(jumpAction, HandleJump);
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
        OnInteractPressed?.Invoke();
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

    private void EnableAction(InputActionReference reference)
    {
        if (reference == null || reference.action == null) return;
        if (!reference.action.enabled) reference.action.Enable();
    }

    private void DisableAction(InputActionReference reference)
    {
        if(reference == null || reference.action == null) return;
        if(reference.action.enabled) reference.action.Disable();
    }
}