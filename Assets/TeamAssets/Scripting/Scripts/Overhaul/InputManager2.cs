using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager2 : MonoBehaviour
{
	private InputSystem_Actions playerInputActions;

	[SerializeField] PlayerController playerLocomotion;
	[SerializeField] CameraController cameraLook;

	void Awake()
	{
		playerInputActions = new InputSystem_Actions();
	}

	void OnEnable()
	{
		SubToPlayerControls();
	}

	void OnDisable()
	{
		playerInputActions.Disable();
	}

	private void SubToPlayerControls()
	{
		playerInputActions.Enable();

		playerInputActions.Player.Move.performed += ctx => playerLocomotion.GetInput(ctx.ReadValue<Vector2>());
		playerInputActions.Player.Move.canceled += ctx => playerLocomotion.GetInput(Vector2.zero);

		playerInputActions.Player.Look.performed += ctx => cameraLook.GetInput(ctx.ReadValue<Vector2>());
		playerInputActions.Player.Look.canceled += ctx => cameraLook.GetInput(Vector2.zero);

		playerInputActions.Player.Jump.performed += Jump;
		playerInputActions.Player.Jump.canceled += JumpCanceled;

		playerInputActions.Player.Sprint.performed += Sprint;
		playerInputActions.Player.Sprint.canceled += SprintCanceled;

		playerInputActions.Player.Crouch.performed += Crouch;
		playerInputActions.Player.Crouch.canceled += CrouchCanceled;

	}

	private void Jump(InputAction.CallbackContext context)
	{
		playerLocomotion.Jump();
	}

	private void JumpCanceled(InputAction.CallbackContext context)
	{
		// optional (usually not needed for a press-type jump)
	}

	private void Sprint(InputAction.CallbackContext context)
	{
		playerLocomotion.Sprint(true);
	}

	private void SprintCanceled(InputAction.CallbackContext context)
	{
		playerLocomotion.Sprint(false);
	}

	private void Crouch(InputAction.CallbackContext context)
	{
		playerLocomotion.Crouch(true);
	}

	private void CrouchCanceled(InputAction.CallbackContext context)
	{
		playerLocomotion.Crouch(false);
	}
}