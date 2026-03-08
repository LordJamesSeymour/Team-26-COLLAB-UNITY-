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

    [Tooltip("Button - Interact")]
    [SerializeField] private InputActionReference interactAction;

    [Tooltip("Button - CameraSwitch")]
    [SerializeField] private InputActionReference cameraSwitchAction;

    [Tooltip("Button - Pausing game and triggering UI event")]
    [SerializeField] private InputActionReference pauseAction;

    [HideInInspector] public Vector2 MoveInput { get; private set; }
    [HideInInspector] public Vector2 LookInput { get; private set; }
    
    [HideInInspector] public bool canInteract;

    [HideInInspector] public bool isSprinting { get; set; }
    [HideInInspector] public bool isCrouching { get; set; }

    public event Action OnJumpPressed;
	public event Action OnDashPressed;
    public event Action OnInteractPressed;
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
        SubscribePerformed(interactAction, HandleInteract);
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
        UnsubscribePerformed(interactAction, HandleInteract);
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
        OnInteractPressed?.Invoke();
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




// using System;
// using UnityEngine;
// using UnityEngine.InputSystem;

// public class InputManager2 : MonoBehaviour
// {
// 	private InputSystem_Actions playerInputActions;

// 	[SerializeField] PlayerController playerController;
// 	[SerializeField] Sliding playerSlide;
// 	[SerializeField] Grappling playerGrapple;
// 	[SerializeField] Dashing playerDash;
// 	[SerializeField] CameraController cameraLook;
// 	[SerializeField] WallRunning wallRunning;

// 	[Tooltip("Vector2 - WASD / Left Thumb Stick")]
//     [SerializeField] private InputActionReference moveAction;

// 	bool callbacksHooked;

// 	public event Action OnJumpPressed;
// 	public event Action OnDashPressed;
// 	public event Action OnInteractPressed;

// 	//[HideInInspector] public Vector2 MoveInput;

// 	void Awake()
// 	{
// 		playerInputActions = new InputSystem_Actions();

// 		if (playerController == null) playerController = GetComponent<PlayerController>();
// 		if (playerSlide == null) playerSlide = GetComponent<Sliding>();
// 		if (playerGrapple == null) playerGrapple = GetComponent<Grappling>();
// 	}

// 	void OnEnable()
// 	{
// 		playerInputActions.Enable();
// 		HookCallbacks();
// 	}

// 	void OnDisable()
// 	{
// 		UnhookCallbacks();
// 		playerInputActions.Disable();
// 	}

// 	private void Update()
//     {
//         //MoveInput = ReadVector2(moveAction);
//     }

// 	private static Vector2 ReadVector2(InputActionReference reference)
//     {
//         return reference != null && reference.action != null ? reference.action.ReadValue<Vector2>() : Vector2.zero;
//     }

// 	private void HookCallbacks()
// 	{
// 		if (callbacksHooked) return;
// 		callbacksHooked = true;

// 		playerInputActions.Player.Move.performed += OnMove;
// 		playerInputActions.Player.Move.canceled += OnMoveCanceled;

// 		playerInputActions.Player.Look.performed += OnLook;
// 		playerInputActions.Player.Look.canceled += OnLookCanceled;

// 		playerInputActions.Player.Jump.performed += OnJump;


// 		playerInputActions.Player.Dash.performed += OnDash;
// 		playerInputActions.Player.Interact.performed += OnInteract;


// 		playerInputActions.Player.Sprint.performed += OnSprint;
// 		playerInputActions.Player.Sprint.canceled += OnSprintCanceled;

// 		playerInputActions.Player.Slide.performed += OnSlide;
// 		playerInputActions.Player.Slide.canceled += OnSlideCanceled;

// 		playerInputActions.Player.Crouch.performed += OnCrouch;
// 		playerInputActions.Player.Crouch.canceled += OnCrouchCanceled;
// 	}

// 	private void UnhookCallbacks()
// 	{
// 		if (!callbacksHooked) return;
// 		callbacksHooked = false;

// 		playerInputActions.Player.Move.performed -= OnMove;
// 		playerInputActions.Player.Move.canceled -= OnMoveCanceled;

// 		playerInputActions.Player.Look.performed -= OnLook;
// 		playerInputActions.Player.Look.canceled -= OnLookCanceled;

// 		playerInputActions.Player.Jump.performed -= OnJump;

// 		playerInputActions.Player.Sprint.performed -= OnSprint;
// 		playerInputActions.Player.Sprint.canceled -= OnSprintCanceled;

// 		playerInputActions.Player.Slide.performed -= OnSlide;
// 		playerInputActions.Player.Slide.canceled -= OnSlideCanceled;

// 		playerInputActions.Player.Crouch.performed -= OnCrouch;
// 		playerInputActions.Player.Crouch.canceled -= OnCrouchCanceled;
// 	}

// 	private void OnMove(InputAction.CallbackContext ctx)
// 	{
// 		Vector2 v = ctx.ReadValue<Vector2>();
// 		playerController.GetInput(v);
// 		wallRunning.GetInput(v);
// 		playerSlide.GetInput(v);
// 		playerDash.GetInput(v);
// 	}

// 	private void OnMoveCanceled(InputAction.CallbackContext ctx)
// 	{
// 		playerController.GetInput(Vector2.zero);
// 		playerSlide.GetInput(Vector2.zero);
// 	}

// 	private void OnLook(InputAction.CallbackContext ctx) => cameraLook.GetInput(ctx.ReadValue<Vector2>());
// 	private void OnLookCanceled(InputAction.CallbackContext ctx) => cameraLook.GetInput(Vector2.zero);

// 	private void OnJump(InputAction.CallbackContext ctx)
// 	{
// 		OnJumpPressed?.Invoke();
// 	}

// 	private void OnDash(InputAction.CallbackContext ctx)
// 	{
// 		OnDashPressed?.Invoke();
// 	}

// 	private void OnInteract(InputAction.CallbackContext ctx)
// 	{
// 		OnInteractPressed?.Invoke();
// 	}

// 	private void OnSprint(InputAction.CallbackContext ctx) => playerController.Sprint(true);
// 	private void OnSprintCanceled(InputAction.CallbackContext ctx) => playerController.Sprint(false);

// 	private void OnSlide(InputAction.CallbackContext ctx) => playerSlide.Slide(true);
// 	private void OnSlideCanceled(InputAction.CallbackContext ctx) => playerSlide.Slide(false);

// 	private void OnCrouch(InputAction.CallbackContext ctx) => playerController.Crouch(true);
// 	private void OnCrouchCanceled(InputAction.CallbackContext ctx) => playerController.Crouch(false);
// }


// // using System;
// // using Group26.Player.Camera;
// // using Group26.Player.Locomotion;
// // using UnityEngine;
// // using UnityEngine.InputSystem;

// // namespace Group26.Player.Input
// // {
// //     public class InputManager : MonoBehaviour
// //     {
// //         [Header("Input References")]
// //         [Space(10)]

// //         private InputSystem_Actions playerInputActions;
// //         private PlayerLocomotion playerLocomotion;
// //         //private CameraModeManager cameraMode;

// //         [Tooltip("Vector2 - WASD / Left Thumb Stick")]
// //         [SerializeField] private InputActionReference moveAction;

// //         [Tooltip("Vector2 - Mouse Delta / Right Thumb Stick")]
// //         [SerializeField] private InputActionReference lookAction;

// //         [Tooltip("Button - Jump")]
// //         [SerializeField] private InputActionReference jumpAction;

// // 		[Tooltip("Button - Sprint")]
// //         [SerializeField] private InputActionReference sprintAction;

// // 		[Tooltip("Button - Slide")]
// //         [SerializeField] private InputActionReference slideAction;

// //         [Tooltip("Button - Crouch")]
// //         [SerializeField] private InputActionReference crouchAction;

// //         [Tooltip("Button - Interact")]
// //         [SerializeField] private InputActionReference interactAction;

// //         [Tooltip("Button - Pausing game and triggering UI event")]
// //         [SerializeField] private InputActionReference pauseAction;

// //         [HideInInspector] public Vector2 MoveInput { get; private set; }
// //         [HideInInspector] public Vector2 LookInput { get; private set; }
        
// //         [HideInInspector] public bool canInteract;

// //         public event Action OnJumpPressed;
// //         public event Action OnInteractPressed;

// //         public bool isSprinting { get; private set; }
// // 		public bool isSliding { get; private set; }
// //         public bool isRequestingWallRun { get; set; }

// //         void Awake()
// //         {
// //             if (playerInputActions == null) playerInputActions = new InputSystem_Actions();
// //             if (playerLocomotion == null) playerLocomotion = GetComponent<PlayerLocomotion>();
// //             //if (cameraMode == null) cameraMode = GetComponent<CameraModeManager>();
// //         }

// //         void OnEnable()
// //         {
// //             SubToPlayerControls();
// //         }

// //         void OnDisable()
// //         {
// //             UnsubFromPlayerControls();
// //         }

// //         private void Update()
// //         {
// //             MoveInput = ReadVector2(moveAction);
// //             LookInput = ReadVector2(lookAction);
// //         }

// //         private void SubToPlayerControls()
// //         {
// //             playerInputActions.Enable();

// //             SubscribePerformed(jumpAction, HandleJump);
// //             SubscribePerformed(interactAction, HandleInteract);

// //             SubscribeToggled(sprintAction, HandleSprintChanged);
// //             SubscribeToggled(slideAction, HandleSlideChanged);
// //         }

// //         private void UnsubFromPlayerControls()
// //         {
// //             playerInputActions.Disable();

// //             UnsubscribePerformed(jumpAction, HandleJump);
// //             UnsubscribePerformed(interactAction, HandleInteract);

// //             UnsubscribeToggled(sprintAction, HandleSprintChanged);
// // 			UnsubscribeToggled(slideAction, HandleSlideChanged);
// //         }

// //         private void HandleSprintChanged(InputAction.CallbackContext context)
// //         {
// //             if(context.performed)
// //             {
// //                 isSprinting = true;
// //             }
// //             else if(context.canceled)
// //             {
// //                 isSprinting = false;
// //             }
// //         }

// // 		private void HandleSlideChanged(InputAction.CallbackContext context)
// // 		{
// // 			if (context.performed)
// // 			{
// // 				isSliding = true;
// // 			}
// // 			else if (context.canceled)
// // 			{
// // 				isSliding = false;
// // 			}
// // 		}

// //         private static Vector2 ReadVector2(InputActionReference reference)
// //         {
// //             return reference != null && reference.action != null ? reference.action.ReadValue<Vector2>() : Vector2.zero;
// //         }

// //         private void HandleJump(InputAction.CallbackContext context)
// //         {
// //             OnJumpPressed?.Invoke();
// //             isRequestingWallRun = true;
// //         }

// //         private void HandleInteract(InputAction.CallbackContext context)
// //         {
// //             OnInteractPressed?.Invoke();
// //         }

// //         private static void SubscribePerformed(InputActionReference reference, Action<InputAction.CallbackContext> actionHandler)
// //         {
// //             if(reference == null || reference.action == null) return;
// //             reference.action.performed += actionHandler;
// //         }

// //         private static void UnsubscribePerformed(InputActionReference reference, Action<InputAction.CallbackContext> actionHandler)
// //         {
// //             if (reference == null || reference.action == null) return;
// //             reference.action.performed -= actionHandler;
// //         }

// //         private static void SubscribeToggled(InputActionReference reference, Action<InputAction.CallbackContext> actionHandler)
// //         {
// //             if (reference == null || reference.action == null) return;
// //             reference.action.performed += actionHandler;
// //             reference.action.canceled += actionHandler;
// //         }

// //         private static void UnsubscribeToggled(InputActionReference reference, Action<InputAction.CallbackContext> actionHandler)
// //         {
// //             if (reference == null || reference.action == null) return;
// //             reference.action.performed -= actionHandler;
// //             reference.action.canceled -= actionHandler;
// //         }
// //     }
// // }