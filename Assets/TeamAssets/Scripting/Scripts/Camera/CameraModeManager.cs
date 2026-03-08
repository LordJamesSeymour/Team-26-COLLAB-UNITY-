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
        private InputManager2 playerInput;

        private PlayerController playerController;

        [SerializeField] private Transform m_playerTransform;
        [SerializeField] private Transform m_cameraPivot;

        [Header("Camera References & Settings")]
        [SerializeField] public CinemachineCamera firstPersonVirtualCamera;
        [SerializeField] public CinemachineCamera thirdPersonVirtualCamera;

        [SerializeField] private Vector2 firstPersonLookSensitivity = Vector2.one;
        [SerializeField] private Vector2 thirdPersonLookSensitivity = Vector2.one;

        public CameraMode currentCameraMode = CameraMode.ThirdPerson;
        private const int activeCameraPriority = 10;
        private const int inactiveCameraPriority = 1;

        [Header("First Person Camera References & Settings")]
        [SerializeField] private Vector2 m_firstPersonPitchLimits = new Vector2(-85f, 85f);
        [SerializeField] private Transform firstPersonYawRoot;
        [SerializeField] private Transform firstPersonPitchPivot;
        private float firstPersonYaw;
        private float firstPersonPitch;

        [Header("Third Person Camera References & Settings")]

        [SerializeField] private Vector2 m_thirdPersonPitchLimits = new Vector2(-60f, 80f);

        [Tooltip("Settings to adjust how quickly the character turns to match the camera direction in third person mode")]
        private float m_yaw;
        private float m_pitch;
        private float m_bodyYawVel;
        [SerializeField, Range(0f, 0.5f)] private float m_bodyTurnSmoothTime = 0.12f;
        [SerializeField, Range(0f, 180f)] private float m_turnWhenCameraYawOffsetExceeds = 25f; // degrees threshold to turn body


        private void Awake()
        {
            if (playerInput == null) playerInput = GetComponent<InputManager2>();
            if (playerInput == null) Debug.LogError("No input manager found");
            if (playerController == null) playerController = GetComponent<PlayerController>();
            if(playerController == null) Debug.LogError("No player controller found");

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            //currentCameraMode = CameraMode.FirstPerson;
            UpdateCameraMode(currentCameraMode);
        }

        private void OnEnable()
        {
            playerInput.OnCameraSwitchPressed += () => SwitchCameraMode();
        }
        private void OnDisable()
        {
            playerInput.OnCameraSwitchPressed -= () => SwitchCameraMode();
        }

        private void Update()
        {
            if(currentCameraMode == CameraMode.FirstPerson)
            {
                ApplyFirstPersonLook(playerInput?.LookInput ?? Vector2.zero);
            }
            else if(currentCameraMode == CameraMode.ThirdPerson)
            {
                ApplyThirdPersonLook(playerInput?.LookInput ?? Vector2.zero);
                UpdateBodyFacingDirection();
            }
        }

        private void SwitchCameraMode()
        {
            currentCameraMode = currentCameraMode == CameraMode.FirstPerson ? CameraMode.ThirdPerson : CameraMode.FirstPerson;
            UpdateCameraMode(currentCameraMode);
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

        private void ApplyFirstPersonLook(Vector2 lookInput)
        {
            float yawDelta = lookInput.x * firstPersonLookSensitivity.x * Time.deltaTime;;
            float pitchDelta = lookInput.y * firstPersonLookSensitivity.y * Time.deltaTime;;

            firstPersonYaw += yawDelta;
            firstPersonPitch = Mathf.Clamp(firstPersonPitch - pitchDelta, m_firstPersonPitchLimits.x, m_firstPersonPitchLimits.y);

            firstPersonYawRoot.rotation = Quaternion.Euler(0f, firstPersonYaw, 0f);
            firstPersonPitchPivot.localRotation = Quaternion.Euler(firstPersonPitch, 0f, 0f);
        }

        private void ApplyThirdPersonLook(Vector2 lookInput)
        {
            m_yaw += lookInput.x * thirdPersonLookSensitivity.x * Time.deltaTime;
            m_pitch -= lookInput.y * thirdPersonLookSensitivity.y * Time.deltaTime;
            m_pitch = Mathf.Clamp(m_pitch, m_thirdPersonPitchLimits.x, m_thirdPersonPitchLimits.y);

            m_cameraPivot.rotation = Quaternion.Euler(m_pitch, m_yaw, 0f);
        }

        private void UpdateBodyFacingDirection()
        {
            if (currentCameraMode != CameraMode.ThirdPerson) return;
            
            float targetYaw = m_cameraPivot.eulerAngles.y;

            float currentYaw = m_playerTransform.eulerAngles.y;
            float yawDifference = Mathf.DeltaAngle(currentYaw, targetYaw);

            if (Mathf.Abs(yawDifference) < m_turnWhenCameraYawOffsetExceeds) return;

            float newYaw = Mathf.SmoothDampAngle(currentYaw, targetYaw, ref m_bodyYawVel, m_bodyTurnSmoothTime);
            m_playerTransform.rotation = Quaternion.Euler(0f, newYaw, 0f);
        }
    }
}