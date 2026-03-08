using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager2 : MonoBehaviour
{
	private InputSystem_Actions playerInputActions;

	[SerializeField] PlayerController playerController;
	[SerializeField] Sliding playerSlide;
	[SerializeField] Grappling playerGrapple;
	[SerializeField] Dashing playerDash;
	[SerializeField] CameraController cameraLook;
	[SerializeField] WallRunning wallRunning;
	[SerializeField] SwingGun swingingGun;

	[Tooltip("Vector2 - WASD / Left Thumb Stick")]
	[SerializeField] private InputActionReference moveAction;

	bool callbacksHooked;

	public event Action OnJumpPressed;
	public event Action OnJumpRelease;
	public event Action OnDashPressed;
	public event Action OnGrapplePressed;
	public event Action OnSwingPressed;
	public event Action OnSwingRelease;
	public event Action OnInteractCanceled;

	void Awake()
	{
		playerInputActions = new InputSystem_Actions();

		if (playerController == null) playerController = GetComponent<PlayerController>();
		if (playerSlide == null) playerSlide = GetComponent<Sliding>();
		if (playerGrapple == null) playerGrapple = GetComponent<Grappling>();
		if (swingingGun == null) swingingGun = GetComponent<SwingGun>();
	}

	void OnEnable()
	{
		playerInputActions.Enable();
		HookCallbacks();
	}

	void OnDisable()
	{
		UnhookCallbacks();
		playerInputActions.Disable();
	}

	private void HookCallbacks()
	{
		if (callbacksHooked) return;
		callbacksHooked = true;

		playerInputActions.Player.Move.performed += OnMove;
		playerInputActions.Player.Move.canceled += OnMoveCanceled;

		playerInputActions.Player.Look.performed += OnLook;
		playerInputActions.Player.Look.canceled += OnLookCanceled;

		playerInputActions.Player.Jump.performed += OnJump;
		playerInputActions.Player.Jump.canceled += OnJumpCanceled;

		playerInputActions.Player.Dash.performed += OnDash;

		playerInputActions.Player.Grapple.performed += OnGrapple;

		playerInputActions.Player.Swing.performed += OnSwing;
		playerInputActions.Player.Swing.canceled += OnSwingCanceled;

		playerInputActions.Player.Sprint.performed += OnSprint;
		playerInputActions.Player.Sprint.canceled += OnSprintCanceled;

		playerInputActions.Player.Slide.performed += OnSlide;
		playerInputActions.Player.Slide.canceled += OnSlideCanceled;

		playerInputActions.Player.Crouch.performed += OnCrouch;
		playerInputActions.Player.Crouch.canceled += OnCrouchCanceled;
	}

	private void UnhookCallbacks()
	{
		if (!callbacksHooked) return;
		callbacksHooked = false;

		playerInputActions.Player.Move.performed -= OnMove;
		playerInputActions.Player.Move.canceled -= OnMoveCanceled;

		playerInputActions.Player.Look.performed -= OnLook;
		playerInputActions.Player.Look.canceled -= OnLookCanceled;

		playerInputActions.Player.Jump.performed -= OnJump;
		playerInputActions.Player.Jump.canceled -= OnJumpCanceled;

		playerInputActions.Player.Dash.performed -= OnDash;

		playerInputActions.Player.Grapple.performed -= OnGrapple;

		playerInputActions.Player.Swing.performed -= OnSwing;
		playerInputActions.Player.Swing.canceled -= OnSwingCanceled;

		playerInputActions.Player.Sprint.performed -= OnSprint;
		playerInputActions.Player.Sprint.canceled -= OnSprintCanceled;

		playerInputActions.Player.Slide.performed -= OnSlide;
		playerInputActions.Player.Slide.canceled -= OnSlideCanceled;

		playerInputActions.Player.Crouch.performed -= OnCrouch;
		playerInputActions.Player.Crouch.canceled -= OnCrouchCanceled;
	}

	private void OnMove(InputAction.CallbackContext ctx)
	{
		Vector2 v = ctx.ReadValue<Vector2>();

		playerController.GetInput(v);
		wallRunning.GetInput(v);
		playerSlide.GetInput(v);
		playerDash.GetInput(v);
		swingingGun.GetInput(v);
	}

	private void OnMoveCanceled(InputAction.CallbackContext ctx)
	{
		playerController.GetInput(Vector2.zero);
		wallRunning.GetInput(Vector2.zero);
		playerSlide.GetInput(Vector2.zero);
		playerDash.GetInput(Vector2.zero);
		swingingGun.GetInput(Vector2.zero);
	}

	private void OnLook(InputAction.CallbackContext ctx)
	{
		cameraLook.GetInput(ctx.ReadValue<Vector2>());
	}

	private void OnLookCanceled(InputAction.CallbackContext ctx)
	{
		cameraLook.GetInput(Vector2.zero);
	}

	private void OnJump(InputAction.CallbackContext ctx)
	{
		OnJumpPressed?.Invoke();
	}

	private void OnJumpCanceled(InputAction.CallbackContext ctx)
	{
		OnJumpRelease?.Invoke();
	}

	private void OnDash(InputAction.CallbackContext ctx)
	{
		OnDashPressed?.Invoke();
	}

	private void OnGrapple(InputAction.CallbackContext ctx)
	{
		OnGrapplePressed?.Invoke();
	}

	private void OnSwing(InputAction.CallbackContext ctx)
	{
		OnSwingPressed?.Invoke();
	}

	private void OnSwingCanceled(InputAction.CallbackContext ctx)
	{
		OnSwingRelease?.Invoke();
	}

	private void OnSprint(InputAction.CallbackContext ctx)
	{
		playerController.Sprint(true);
	}

	private void OnSprintCanceled(InputAction.CallbackContext ctx)
	{
		playerController.Sprint(false);
	}

	private void OnSlide(InputAction.CallbackContext ctx)
	{
		playerSlide.Slide(true);
	}

	private void OnSlideCanceled(InputAction.CallbackContext ctx)
	{
		playerSlide.Slide(false);
	}

	private void OnCrouch(InputAction.CallbackContext ctx)
	{
		playerController.Crouch(true);
	}

	private void OnCrouchCanceled(InputAction.CallbackContext ctx)
	{
		playerController.Crouch(false);
	}
}