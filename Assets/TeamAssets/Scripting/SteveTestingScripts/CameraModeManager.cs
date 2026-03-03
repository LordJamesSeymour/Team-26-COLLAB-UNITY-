using Unity.Cinemachine;
using UnityEngine;

namespace Group26.Player.Camera
{
    public enum CameraMode
    {
        FirstPerson, 
        ThirdPerson
    }

    public class CameraModeManager : MonoBehaviour
    {
        private InputManager playerInput;

        [SerializeField] private Transform playerTransform;

        // -- Camera Mode Management Variables -- //
        [SerializeField] private bool switchCameraMode = false; // Just for testing purposes, will be removed
        [SerializeField]private CinemachineCamera firstPersonVirtualCamera;
        [SerializeField] private CinemachineCamera thirdPersonVirtualCamera;

        [SerializeField] private Vector2 firstPersonLookSensitivity = Vector2.one;
        [SerializeField] private Vector2 thirdPersonLookSensitivity = Vector2.one;

        private CameraMode currentCameraMode = CameraMode.ThirdPerson;
        private const int activeCameraPriority = 10;
        private const int inactiveCameraPriority = 0;

        [SerializeField] private Transform firstPersonYawRoot;
        [SerializeField] private Transform firstPersonPitchPivot;

        private float firstPersonYaw;
        private float firstPersonPitch;

        private void Awake()
        {
            if(playerInput == null) playerInput = GetComponent<InputManager>();
            if(playerInput == null) Debug.LogError("No input manager found");

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            currentCameraMode = CameraMode.ThirdPerson;
            UpdateCameraMode(currentCameraMode);
        }

        private void Update()
        {
            if (switchCameraMode)
            {
                switchCameraMode = false;
                currentCameraMode = currentCameraMode == CameraMode.FirstPerson ? CameraMode.ThirdPerson : CameraMode.FirstPerson;
                UpdateCameraMode(currentCameraMode);
            }

            if(currentCameraMode == CameraMode.FirstPerson)
            {
                ApplyFirstPersonLook(playerInput?.LookInput ?? Vector2.zero);
            }
            else if(currentCameraMode == CameraMode.ThirdPerson)
            {
                // Apply third person look logic here if needed
            }
        }

        private void UpdateCameraMode(CameraMode targetCam)
        {
            if(targetCam == CameraMode.FirstPerson)
            {
                firstPersonVirtualCamera.Priority = activeCameraPriority;
                thirdPersonVirtualCamera.Priority = inactiveCameraPriority;
            }
            else
            {
                firstPersonVirtualCamera.Priority = inactiveCameraPriority;
                thirdPersonVirtualCamera.Priority = activeCameraPriority;
            }
        }

        public void ApplyFirstPersonLook(Vector2 lookInput)
        {
            float yawDelta = lookInput.x * firstPersonLookSensitivity.x;
            float pitchDelta = lookInput.y * firstPersonLookSensitivity.y;

            firstPersonYaw += yawDelta;
            firstPersonPitch = Mathf.Clamp(firstPersonPitch - pitchDelta, -85f, 85f);

            firstPersonYawRoot.rotation = Quaternion.Euler(0f, firstPersonYaw, 0f);
            firstPersonPitchPivot.localRotation = Quaternion.Euler(firstPersonPitch, 0f, 0f);
        }
    }
}