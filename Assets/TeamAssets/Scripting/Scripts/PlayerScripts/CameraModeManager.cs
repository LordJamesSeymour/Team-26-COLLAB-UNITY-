using Unity.Cinemachine;
using UnityEngine;
using System.Collections;
using Group26.Player.Movement;
using Group26.Player.Inputs;
using DG.Tweening;

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
        private PlayerController playerController;
        private WallRunning wallRunning;

        [SerializeField] private Transform m_playerTransform;
        [SerializeField] private Transform m_cameraPivot;

        [Header("Camera References & Settings")]
        [SerializeField] public CinemachineCamera firstPersonVirtualCamera;
        [SerializeField] public CinemachineCamera thirdPersonVirtualCamera;

        private Transform cameraHolder;

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

        [Header("FOV Settings")]
        [SerializeField] private float defaultFOV = 60f;
        [SerializeField] private float sprintFOV = 75f;
        [SerializeField, Range(0f, 0.5f)] private float fovTransitionDuration = 0.25f;
        
        [Header("Dash/Burst FOV Settings")]
        [SerializeField] private float dashFOV = 85f;
        [SerializeField, Range(0f, 0.25f)] private float burstTransitionDuration;
        
        private bool isSprintingLastFrame = false;
        private Coroutine fovTransitionCoroutine;
        private Coroutine burstFOVCoroutine;


        private void Awake()
        {
            if (playerInput == null) playerInput = GetComponent<InputManager>();
            if (playerInput == null) Debug.LogError("No input manager found");

            if (playerController == null) playerController = GetComponent<PlayerController>();
            if (playerController == null) Debug.LogError("No player controller found");

            if (wallRunning == null) wallRunning = GetComponent<WallRunning>();
            if (wallRunning == null) Debug.LogError("No wall running script found");

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            //currentCameraMode = CameraMode.FirstPerson;
            UpdateCameraMode(currentCameraMode);
            
            // Initialize FOV values
            SetCameraFOV(defaultFOV);
        }

        private void OnEnable()
        {
            playerInput.OnCameraSwitchPressed += () => SwitchCameraMode();
            playerInput.OnDashPressed += BurstFOVIncrease;
        }
        private void OnDisable()
        {
            playerInput.OnCameraSwitchPressed -= () => SwitchCameraMode();
            playerInput.OnDashPressed -= BurstFOVIncrease;
        }

        private void Update()
        {


            if(currentCameraMode == CameraMode.FirstPerson)
            {
                ApplyFirstPersonLook(playerInput?.LookInput ?? Vector2.zero);

                cameraHolder = firstPersonVirtualCamera.transform;

                if (playerController.m_bIsWallRunning && wallRunning.wallLeft)
                {
                    DoTilt(-5f);
                }
                else if(playerController.m_bIsWallRunning && wallRunning.wallRight)
                {
                    DoTilt(5f);
                }
                else
                {
                    DoTilt(0f);
                }
            }

            else if(currentCameraMode == CameraMode.ThirdPerson)
            {
                ApplyThirdPersonLook(playerInput?.LookInput ?? Vector2.zero);

                cameraHolder = thirdPersonVirtualCamera.transform;

                if (playerController.m_bIsWallRunning && wallRunning.wallLeft)
                {
                    DoTilt(-5f);
                }
                else if(playerController.m_bIsWallRunning && wallRunning.wallRight)
                {
                    DoTilt(5f);
                }
                else
                {
                    DoTilt(0f);
                }

                UpdateBodyFacingDirection();
            }
            
            // Check for sprint state changes and update FOV
            HandleSprintFOV();
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
                firstPersonYaw = m_cameraPivot.eulerAngles.y;
                firstPersonYawRoot.rotation = Quaternion.Euler(0f, firstPersonYaw, 0f);

                firstPersonVirtualCamera.Priority = activeCameraPriority;
                thirdPersonVirtualCamera.Priority = inactiveCameraPriority;
            }
            else
            {
                // Sync TP yaw to current FP yaw (or player yaw) when entering TP
                m_yaw = firstPersonYawRoot.eulerAngles.y;
                m_pitch = Mathf.Clamp(m_pitch, m_thirdPersonPitchLimits.x, m_thirdPersonPitchLimits.y);

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

        private void BurstFOVIncrease()
        {
            if(!playerController.m_bDashing) return;

            // Stop any existing burst FOV effect
            if (burstFOVCoroutine != null)
            {
                StopCoroutine(burstFOVCoroutine);
            }
            
            // Start new burst FOV effect
            burstFOVCoroutine = StartCoroutine(DoBurstFOV());
        }
        
        private IEnumerator DoBurstFOV()
        {
            // Store the current FOV to return to
            float originalFOV = GetCurrentCameraFOV();
            float elapsedTime = 0f;
            
            // Phase 1: Smooth increase to dash FOV over burstTransitionDuration
            while (elapsedTime < burstTransitionDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / burstTransitionDuration;
                
                // Use smooth curve for natural feeling
                float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);
                float currentFOV = Mathf.Lerp(originalFOV, dashFOV, smoothProgress);
                
                SetCameraFOV(currentFOV);
                yield return null;
            }
            
            // Ensure we're exactly at dash FOV
            SetCameraFOV(dashFOV);
            
            // Phase 3: Smooth return to original FOV
            elapsedTime = 0f;
            while (elapsedTime < burstTransitionDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / burstTransitionDuration;
                
                // Use smooth curve for natural feeling
                float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);
                float currentFOV = Mathf.Lerp(dashFOV, originalFOV, smoothProgress);
                
                SetCameraFOV(currentFOV);
                yield return null;
            }
            
            // Ensure we end exactly at original FOV
            SetCameraFOV(originalFOV);
            
            // Clear the coroutine reference
            burstFOVCoroutine = null;
        }

        private void HandleSprintFOV()
        {
            if (playerController == null) return;
            
            bool isCurrentlySprinting = playerInput.isSprinting /*&& playerController.IsGrounded*/ || playerController.m_bIsWallRunning || playerController.m_bActiveGrapple;
            
            // Check if sprint state changed
            if (isCurrentlySprinting != isSprintingLastFrame)
            {
                float targetFOV = isCurrentlySprinting ? sprintFOV : defaultFOV;
                StartFOVTransition(targetFOV);
                isSprintingLastFrame = isCurrentlySprinting;
            }
        }
        
        private void StartFOVTransition(float targetFOV)
        {
            // Stop any existing FOV transition
            if (fovTransitionCoroutine != null)
            {
                StopCoroutine(fovTransitionCoroutine);
            }
            
            // Start new FOV transition
            fovTransitionCoroutine = StartCoroutine(TransitionFOV(targetFOV));
        }
        
        private IEnumerator TransitionFOV(float targetFOV)
        {
            float startFOV = GetCurrentCameraFOV();
            float elapsedTime = 0f;
            
            while (elapsedTime < fovTransitionDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / fovTransitionDuration;
                
                // Use smooth curve for more natural feeling
                float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);
                float currentFOV = Mathf.Lerp(startFOV, targetFOV, smoothProgress);
                
                SetCameraFOV(currentFOV);
                
                yield return null;
            }
            
            // Ensure we end exactly at target FOV
            SetCameraFOV(targetFOV);
            fovTransitionCoroutine = null;
        }
        
        private void SetCameraFOV(float fov)
        {
            if (firstPersonVirtualCamera != null && firstPersonVirtualCamera.Lens.FieldOfView != fov)
            {
                var lens = firstPersonVirtualCamera.Lens;
                lens.FieldOfView = fov;
                firstPersonVirtualCamera.Lens = lens;
            }
            
            if (thirdPersonVirtualCamera != null && thirdPersonVirtualCamera.Lens.FieldOfView != fov)
            {
                var lens = thirdPersonVirtualCamera.Lens;
                lens.FieldOfView = fov;
                thirdPersonVirtualCamera.Lens = lens;
            }
        }
        
        private float GetCurrentCameraFOV()
        {
            if (currentCameraMode == CameraMode.FirstPerson && firstPersonVirtualCamera != null)
            {
                return firstPersonVirtualCamera.Lens.FieldOfView;
            }
            else if (currentCameraMode == CameraMode.ThirdPerson && thirdPersonVirtualCamera != null)
            {
                return thirdPersonVirtualCamera.Lens.FieldOfView;
            }
            
            return defaultFOV;
        }

        private void DoTilt(float zTiltAmount)
        {
            cameraHolder.transform.DOLocalRotate(new Vector3(0,0, zTiltAmount), 0.25f);
        }
    }
}