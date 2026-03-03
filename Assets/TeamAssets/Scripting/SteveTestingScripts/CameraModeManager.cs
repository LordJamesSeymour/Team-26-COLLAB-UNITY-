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
        private CameraMode currentCameraMode = CameraMode.ThirdPerson;
        private const int activeCameraPriority = 10;
        private const int inactiveCameraPriority = 0;

        private void Awake()
        {
            //if(playerInput == null) playerInput = GetComponent<InputManager>();
            //if(playerInput == null) Debug.LogError("No input manager found");

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
    }
}