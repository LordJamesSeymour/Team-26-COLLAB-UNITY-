using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class InputManager : MonoBehaviour
{
    private InputSystem_Actions playerInputActions;

	[SerializeField] PlayerLocomotion playerLocomotion;
	[SerializeField] CameraLook cameraLook;

    [HideInInspector] public Vector2 MoveInput;
    [HideInInspector] public Vector2 LookInput;
    [HideInInspector] public bool jumpPressed;
    [HideInInspector] public bool canInteract;


    void Awake()
    {
        playerInputActions = new InputSystem_Actions();

		if (playerLocomotion == null)
		{
			playerLocomotion = GetComponent<PlayerLocomotion>();
		}

		if (cameraLook == null)
		{
			Debug.LogError("No camera look script has been asigned!");
		}
	}

    void OnEnable()
    {
        SubToPlayerControls();
    }

    void OnDisable()
    {
        playerInputActions.Disable();
    }


    #region Player Controls
    private void SubToPlayerControls()
    {
		playerInputActions.Player.Move.performed += context => playerLocomotion.PlayerInput(context.ReadValue<Vector2>());
        playerInputActions.Player.Move.canceled += context => playerLocomotion.PlayerInput(Vector2.zero);
		//playerInputActions.Player.Move.performed += context => Debug.Log(context.ReadValue<Vector2>());


		playerInputActions.Player.Look.performed += context => cameraLook.PlayerInput(context.ReadValue<Vector2>());
        playerInputActions.Player.Look.canceled += context => cameraLook.PlayerInput(Vector2.zero);
		//playerInputActions.Player.Look.performed += context => Debug.Log(context.ReadValue<Vector2>());

		playerInputActions.Player.Jump.performed += Jump;
        playerInputActions.Player.Jump.canceled += JumpCanceled;

        playerInputActions.Player.Interact.performed += Interact;

        playerInputActions.Enable();
    }

    private void Jump(InputAction.CallbackContext context)
    {
		playerLocomotion.PlayerJump();
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

        playerInputActions.Player.Move.performed -= context => MoveInput = context.ReadValue<Vector2>();
        playerInputActions.Player.Move.canceled -= context => MoveInput = Vector2.zero;

        playerInputActions.Player.Look.performed -= context => LookInput = context.ReadValue<Vector2>();
        playerInputActions.Player.Look.canceled -= context => LookInput = Vector2.zero;

        playerInputActions.Player.Jump.performed -= Jump;
        playerInputActions.Player.Jump.canceled -= JumpCanceled;

        playerInputActions.Player.Interact.performed -= Interact;
    }
    #endregion
}