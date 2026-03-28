using System;
using UnityEngine;
using Group26.Player.Camera;
using Group26.Player.Movement;
using UnityEngine.InputSystem;

namespace Group26.Player.Inputs
{
    public class InputManager : MonoBehaviour
    {
        [Header("Input References")]
        [Space(10)]
        private InputSystem_Actions playerInputActions;
        private CameraModeManager cameraMode;
        private PlayerController playerController;

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

        [Tooltip("Button - ModeSwitch")]
        [SerializeField] private InputActionReference modeSwitchAction;

        [Tooltip("Button - Pausing game and triggering UI event")]
        [SerializeField] private InputActionReference pauseAction;

        [HideInInspector] public Vector2 MoveInput { get; private set; }
        [HideInInspector] public Vector2 LookInput { get; private set; }

        [HideInInspector] public bool canGrapple;
        [HideInInspector] public bool isSprinting;
        [HideInInspector] public bool isCrouching;
        [HideInInspector] public bool isSwinging;
        [HideInInspector] public bool isGrappling;

        public event Action OnJumpPressed;
        public event Action OnJumpRelease;
        public event Action OnDashPressed;
        public event Action OnGrapplePressed;
        public event Action OnGrappleReleased;
        public event Action OnSwingStarted;
        public event Action OnSwingStopped;
        public event Action OnCameraSwitchPressed;
        public event Action OnModeSwitchPressed;
        public event Action OnPausePressed;

        void Awake()
        {
            if (playerInputActions == null) playerInputActions = new InputSystem_Actions();
            if (cameraMode == null) cameraMode = GetComponent<CameraModeManager>();
            if (playerController == null) playerController = GetComponent<PlayerController>();
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

            if (IsRailLocked())
            {
                isSprinting = false;
                isCrouching = false;

                if (isSwinging)
                {
                    OnSwingStopped?.Invoke();
                    isSwinging = false;
                }
            }
        }

        public void ClearRailBlockedInputs()
        {
            isSprinting = false;
            isCrouching = false;

            if (isSwinging)
            {
                OnSwingStopped?.Invoke();
                isSwinging = false;
            }
        }

        private bool IsRailLocked()
        {
            return playerController != null && playerController.IsOnRail;
        }

        private void SubToPlayerControls()
        {
            playerInputActions.Enable();

            SubscribePerformed(jumpAction, HandleJump);
            //SubscribePerformed(grappleAction, HandleInteract);
            SubscribePerformed(dashAction, HandleDash);
            SubscribePerformed(cameraSwitchAction, HandleCameraSwitch);
            SubscribePerformed(modeSwitchAction, HandleModeSwitch);
            SubscribePerformed(pauseAction, HandlePause);

            SubscribeToggled(grappleAction, HandleGrappleChanged);
            SubscribeToggled(sprintAction, HandleSprintChanged);
            SubscribeToggled(crouchAction, HandleCrouchChanged);
            SubscribeToggled(swingAction, HandleSwingChanged);
        }

        private void UnsubFromPlayerControls()
        {
            playerInputActions.Disable();

            UnsubscribePerformed(jumpAction, HandleJump);
            //UnsubscribePerformed(grappleAction, HandleInteract);
            UnsubscribePerformed(dashAction, HandleDash);
            UnsubscribePerformed(cameraSwitchAction, HandleCameraSwitch);
            UnsubscribePerformed(modeSwitchAction, HandleModeSwitch);
            UnsubscribePerformed(pauseAction, HandlePause);

            UnsubscribeToggled(grappleAction, HandleGrappleChanged);
            UnsubscribeToggled(sprintAction, HandleSprintChanged);
            UnsubscribeToggled(crouchAction, HandleCrouchChanged);
            UnsubscribeToggled(swingAction, HandleSwingChanged);
        }

        private static Vector2 ReadVector2(InputActionReference reference)
        {
            return reference != null && reference.action != null
                ? reference.action.ReadValue<Vector2>()
                : Vector2.zero;
        }

        private void HandleJump(InputAction.CallbackContext context)
        {
            OnJumpPressed?.Invoke();
        }

        private void HandleInteract(InputAction.CallbackContext context)
        {
            if (IsRailLocked()) return;
            OnGrapplePressed?.Invoke();
        }

        private void HandleDash(InputAction.CallbackContext context)
        {
            if (IsRailLocked()) return;
            OnDashPressed?.Invoke();
        }

        private void HandleCameraSwitch(InputAction.CallbackContext context)
        {
            OnCameraSwitchPressed?.Invoke();
        }

        private void HandleModeSwitch(InputAction.CallbackContext context)
        {
            OnModeSwitchPressed?.Invoke();
        }

        private void HandlePause(InputAction.CallbackContext context)
        {
            OnPausePressed?.Invoke();
        }

        private void HandleSprintChanged(InputAction.CallbackContext context)
        {
            if (IsRailLocked())
            {
                isSprinting = false;
                return;
            }

            if (context.performed)
                isSprinting = true;
            else if (context.canceled)
                isSprinting = false;
        }

        private void HandleCrouchChanged(InputAction.CallbackContext context)
        {
            if (IsRailLocked())
            {
                isCrouching = false;
                return;
            }

            if (context.performed)
                isCrouching = true;
            else if (context.canceled)
                isCrouching = false;
        }

        private void HandleSwingChanged(InputAction.CallbackContext context)
        {
            if (IsRailLocked())
            {
                if (isSwinging)
                {
                    OnSwingStopped?.Invoke();
                    isSwinging = false;
                }
                return;
            }

            if (context.performed)
            {
                OnSwingStarted?.Invoke();
                isSwinging = true;
            }
            else if (context.canceled)
            {
                OnSwingStopped?.Invoke();
                isSwinging = false;
            }
        }

        private void HandleGrappleChanged(InputAction.CallbackContext context)
        {
            if(IsRailLocked())
            {
                if (isGrappling)
                {
                    OnGrappleReleased?.Invoke();
                    isGrappling = false;
                }
            }
            else
            {
                if (context.performed)
                {
                    OnGrapplePressed?.Invoke();
                    isGrappling = true;
                }
                else if (context.canceled)
                {
                    OnGrappleReleased?.Invoke();
                    isGrappling = false;
                }
            }
        }

        private static void SubscribePerformed(InputActionReference reference, Action<InputAction.CallbackContext> actionHandler)
        {
            if (reference == null || reference.action == null) return;
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
}