using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class InputManager : MonoBehaviour
{
    private InputSystem_Actions playerInputActions;

    [HideInInspector] public Vector2 MoveInput;
    [HideInInspector] public Vector2 LookInput;
    [HideInInspector] public bool jumpPressed;
    [HideInInspector] public bool canInteract;


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


    #region Player Controls
    private void SubToPlayerControls()
    {
        playerInputActions.Player.Move.performed += context => MoveInput = context.ReadValue<Vector2>();
        playerInputActions.Player.Move.canceled += context => MoveInput = Vector2.zero;

        playerInputActions.Player.Look.performed += context => LookInput = context.ReadValue<Vector2>();
        playerInputActions.Player.Look.canceled += context => LookInput = Vector2.zero;

        playerInputActions.Player.Jump.performed += Jump;
        playerInputActions.Player.Jump.canceled += JumpCanceled;

        playerInputActions.Player.Interact.performed += Interact;

        playerInputActions.Enable();
    }

    private void Jump(InputAction.CallbackContext context)
    {
        //TO DO:
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