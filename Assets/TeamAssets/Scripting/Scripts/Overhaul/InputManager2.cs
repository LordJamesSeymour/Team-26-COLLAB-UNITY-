using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager2 : MonoBehaviour
{
	private InputSystem_Actions playerInputActions;

	[SerializeField] PlayerController playerLocomotion;
	[SerializeField] Sliding playerSlide;
	[SerializeField] CameraController cameraLook;

	bool callbacksHooked;

	void Awake()
	{
		playerInputActions = new InputSystem_Actions();

		if (playerLocomotion == null) playerLocomotion = GetComponent<PlayerController>();
		if (playerSlide == null) playerSlide = GetComponent<Sliding>();
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

		playerInputActions.Player.Sprint.performed += OnSprint;
		playerInputActions.Player.Sprint.canceled += OnSprintCanceled;

		// Momentum slide: press starts slide; releasing does NOT end it
		playerInputActions.Player.Slide.performed += OnSlide;

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

		playerInputActions.Player.Sprint.performed -= OnSprint;
		playerInputActions.Player.Sprint.canceled -= OnSprintCanceled;

		playerInputActions.Player.Slide.performed -= OnSlide;

		playerInputActions.Player.Crouch.performed -= OnCrouch;
		playerInputActions.Player.Crouch.canceled -= OnCrouchCanceled;
	}

	private void OnMove(InputAction.CallbackContext ctx)
	{
		Vector2 v = ctx.ReadValue<Vector2>();
		playerLocomotion.GetInput(v);
		playerSlide.GetInput(v);
	}

	private void OnMoveCanceled(InputAction.CallbackContext ctx)
	{
		playerLocomotion.GetInput(Vector2.zero);
		playerSlide.GetInput(Vector2.zero);
	}

	private void OnLook(InputAction.CallbackContext ctx) => cameraLook.GetInput(ctx.ReadValue<Vector2>());
	private void OnLookCanceled(InputAction.CallbackContext ctx) => cameraLook.GetInput(Vector2.zero);

	private void OnJump(InputAction.CallbackContext ctx) => playerLocomotion.Jump();

	private void OnSprint(InputAction.CallbackContext ctx) => playerLocomotion.Sprint(true);
	private void OnSprintCanceled(InputAction.CallbackContext ctx) => playerLocomotion.Sprint(false);

	private void OnSlide(InputAction.CallbackContext ctx) => playerSlide.Slide(true);

	private void OnCrouch(InputAction.CallbackContext ctx) => playerLocomotion.Crouch(true);
	private void OnCrouchCanceled(InputAction.CallbackContext ctx) => playerLocomotion.Crouch(false);
}