using UnityEngine;
using UnityEngine.InputSystem;

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
		playerInputActions.Enable();

		// Movement: store input continuously, locomotion will compute direction each FixedUpdate
		playerInputActions.Player.Move.performed += ctx => playerLocomotion.SetMoveInput(ctx.ReadValue<Vector2>());
		playerInputActions.Player.Move.canceled += ctx => playerLocomotion.SetMoveInput(Vector2.zero);

		// Look stays the same
		playerInputActions.Player.Look.performed += ctx => cameraLook.PlayerInput(ctx.ReadValue<Vector2>());
		playerInputActions.Player.Look.canceled += ctx => cameraLook.PlayerInput(Vector2.zero);

		playerInputActions.Player.Jump.performed += Jump;
		playerInputActions.Player.Jump.canceled += JumpCanceled;

		playerInputActions.Player.Interact.performed += Interact;
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