using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
	private InputSystem_Actions playerInputActions;

	[SerializeField] PlayerLocomotion playerLocomotion;
	[SerializeField] Wallrun playerWallrun;
	[SerializeField] CameraLook cameraLook;

	void Awake()
	{
		playerInputActions = new InputSystem_Actions();

		if (playerLocomotion == null)
			playerLocomotion = GetComponent<PlayerLocomotion>();

		if (playerWallrun == null)
			playerWallrun = GetComponent<Wallrun>();

		if (cameraLook == null)
			Debug.LogError("No camera look script has been assigned!");
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

		playerInputActions.Player.Move.performed += ctx => playerLocomotion.SetMoveInput(ctx.ReadValue<Vector2>());
		playerInputActions.Player.Move.canceled += ctx => playerLocomotion.SetMoveInput(Vector2.zero);

		playerInputActions.Player.Sprint.performed += Sprint;
		playerInputActions.Player.Sprint.canceled += SprintCanceled;

		playerInputActions.Player.Look.performed += ctx => cameraLook.PlayerInput(ctx.ReadValue<Vector2>());
		playerInputActions.Player.Look.canceled += ctx => cameraLook.PlayerInput(Vector2.zero);

		playerInputActions.Player.Jump.performed += Jump;
		playerInputActions.Player.Jump.canceled += JumpCanceled;
	}

	private void Jump(InputAction.CallbackContext context)
	{
		// Always send jump input to wallrun (it will only act if currently wallrunning)
		playerLocomotion.PlayerJump();
		playerWallrun.m_jumpRequested = true;
	}

	private void JumpCanceled(InputAction.CallbackContext context)
	{
		// optional (usually not needed for a press-type jump)
	}

	private void Sprint(InputAction.CallbackContext context)
	{
		playerLocomotion.PlayerSprint(true);
	}

	private void SprintCanceled(InputAction.CallbackContext context)
	{
		playerLocomotion.PlayerSprint(false);
	}
}