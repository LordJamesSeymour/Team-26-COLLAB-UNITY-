using Group26.Editor.Camera;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Group26.Editor.Inputs
{
    public class EditorInput : MonoBehaviour
    {
        [SerializeField] private EditorCamera editorCamera;

        private InputSystem_Actions editorInputActions;

        private float xInput;
        private float yInput;
        private bool isHoldingClick;

        private void Awake()
        {
            editorInputActions = new InputSystem_Actions();

            if (editorCamera == null)
                editorCamera = GetComponent<EditorCamera>();

            if (editorCamera == null)
                Debug.LogError("No EditorCamera assigned or found.");
        }

        private void OnEnable()
        {
            editorInputActions.Enable();
        }

        private void OnDisable()
        {
            editorInputActions.Disable();
        }

        private void Update()
        {
            if (Mouse.current == null || editorCamera == null)
                return;

            isHoldingClick = Mouse.current.leftButton.isPressed;

            if (isHoldingClick)
            {
                Vector2 mouseDelta = Mouse.current.delta.ReadValue();
                xInput = mouseDelta.x;
                yInput = mouseDelta.y;
            }
            else
            {
                xInput = 0f;
                yInput = 0f;
            }

            editorCamera.SetClickState(isHoldingClick);
            editorCamera.SetLookInput(xInput, yInput);
        }
    }
}