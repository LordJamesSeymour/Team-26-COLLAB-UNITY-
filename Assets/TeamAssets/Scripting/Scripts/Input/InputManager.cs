using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
	private InputSystem_Actions m_playerInputActions;

	[SerializeField] PlayerLocomotion playerLocomotion;
	[SerializeField] Wallrun playerWallrun;
	[SerializeField] CameraLook cameraLook;

	public event Action m_grappleAction;

	void Awake()
	{
		m_playerInputActions = new InputSystem_Actions();

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
		m_playerInputActions.Disable();
	}

	private void SubToPlayerControls()
	{
		m_playerInputActions.Enable();

		m_playerInputActions.Player.Move.performed += ctx => playerLocomotion.SetMoveInput(ctx.ReadValue<Vector2>());
		m_playerInputActions.Player.Move.canceled += ctx => playerLocomotion.SetMoveInput(Vector2.zero);

		m_playerInputActions.Player.Sprint.performed += Sprint;
		m_playerInputActions.Player.Sprint.canceled += SprintCanceled;

		m_playerInputActions.Player.Look.performed += ctx => cameraLook.PlayerInput(ctx.ReadValue<Vector2>());
		m_playerInputActions.Player.Look.canceled += ctx => cameraLook.PlayerInput(Vector2.zero);

		m_playerInputActions.Player.Jump.performed += Jump;
		m_playerInputActions.Player.Jump.canceled += JumpCanceled;

		m_playerInputActions.Player.Interact.performed += Interact;

		m_playerInputActions.Player.Grapple.performed += GrapplePerformed;
    }

	private void GrapplePerformed(InputAction.CallbackContext context)
	{
		m_grappleAction.Invoke();
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

	private void Interact(InputAction.CallbackContext context)
	{
		// TO DO:
	}
}