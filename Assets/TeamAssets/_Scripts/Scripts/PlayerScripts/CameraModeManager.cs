using Unity.Cinemachine;
using UnityEngine;
using System.Collections;
using Group26.Player.Movement;
using Group26.Player.Inputs;
//using DG.Tweening;
using Group26.Player.Utility;

namespace Group26.Player.Camera
{
    public enum CameraMode
    {
        //FirstPerson, 
        ThirdPerson
    }

    public class CameraModeManager : MonoBehaviour
    {
        private InputManager playerInput;
        private PlayerController playerController;
        private PlayerModeSwitcher playerModeSwitcher;
        private BallRollController ballRollController;
        private WallRunning wallRunning;

        [SerializeField] private Transform m_playerTransform;
        [SerializeField] private Transform m_cameraPivot;

        [Header("Camera References & Settings")]
        // [SerializeField] public CinemachineCamera firstPersonVirtualCamera;
        [SerializeField] public CinemachineCamera thirdPersonVirtualCamera;
        [SerializeField] public CinemachineCamera leftWallRunningVirtualCamera;
        [SerializeField] public CinemachineCamera rightWallRunningVirtualCamera;

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
        [SerializeField, Range(0f, 0.5f)] private float FOVTransitionDuration = 0.25f;
        
        [Header("DashFOV Settings")]
        [SerializeField] private float dashFOV = 85f;
        [SerializeField, Range(0f, 0.25f)] private float burstTransitionDuration;

        [Header("GrappleBoost FOV Settings")]
        [SerializeField] private float grappleBoostFOV = 85f;
        [SerializeField, Range(0f, 5)] private float grappleBoostTransitionUpDuration;
        [SerializeField, Range(0f, 3f)] private float grappleBoostTransitionDownDuration;
        private bool isSprintingLastFrame = false;
        private bool isBursting;
        private bool isGrappleBoostActive;
        private float currentBaseFOV;
        private float currentDashFOV;
        private float currentGrappleBoostFOV;
        private Coroutine baseFOVCoroutine;
        private Coroutine dashFOVCoroutine;
        private Coroutine grappleBoostFOVCoroutine;
        private Coroutine ballMovementFOVCoroutine;

        private void Awake()
        {
            currentCameraMode = CameraMode.ThirdPerson;

            if (playerInput == null) playerInput = GetComponent<InputManager>();
            if (playerInput == null) Debug.LogError("No input manager found");

            if (playerController == null) playerController = GetComponent<PlayerController>();
            if (playerController == null) Debug.LogError("No player controller found");

            if (wallRunning == null) wallRunning = GetComponent<WallRunning>();
            if (wallRunning == null) Debug.LogError("No wall running script found");

            if (playerModeSwitcher == null) playerModeSwitcher = GetComponent<PlayerModeSwitcher>();
            if (playerModeSwitcher == null) Debug.LogWarning("No player mode switcher found");

            // if (firstPersonVirtualCamera == null) Debug.LogWarning("First person virtual camera not assigned");
            // if (thirdPersonVirtualCamera == null) Debug.LogError("Third person virtual camera not assigned");
            if (leftWallRunningVirtualCamera == null) Debug.LogError("Left wall running virtual camera not assigned");
            if (rightWallRunningVirtualCamera == null) Debug.LogError("Right wall running virtual camera not assigned");

            if(ballRollController == null) ballRollController = GetComponent<BallRollController>();
            if(ballRollController == null) Debug.LogWarning("No ball roll controller found");

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            //currentCameraMode = CameraMode.FirstPerson;
            //UpdateCameraMode(currentCameraMode);
            
            // Initialize FOV values
            currentBaseFOV = defaultFOV;
            currentDashFOV = defaultFOV;
            currentGrappleBoostFOV = defaultFOV;
            SetCameraFOV(defaultFOV);
        }

        private void OnEnable()
        {
            //playerInput.OnCameraSwitchPressed += SwitchCameraMode;
            playerInput.OnDashPressed += BurstFOVIncrease;
        }
        private void OnDisable()
        {
            //playerInput.OnCameraSwitchPressed -= SwitchCameraMode;
            playerInput.OnDashPressed -= BurstFOVIncrease;
        }

        private void OnCollisionEnter(Collision collision)
        {
            EndGrappleBoostFOV();
        }

        private void Update()
        {
            
            // if(currentCameraMode == CameraMode.FirstPerson)
            // {
            //     ApplyFirstPersonLook(playerInput?.LookInput ?? Vector2.zero);

            //     cameraHolder = firstPersonVirtualCamera.transform; 

            //     if(playerController.m_bIsWallRunning && wallRunning.wallLeft)
            //     {
            //         DoTilt(-5f);
            //     }
            //     else if(playerController.m_bIsWallRunning && wallRunning.wallRight)
            //     {
            //         DoTilt(5f);
            //     }
            //     else
            //     {
            //         DoTilt(0f);
            //     }
            // }

            if(currentCameraMode == CameraMode.ThirdPerson)
            {
                ApplyThirdPersonLook(playerInput?.LookInput ?? Vector2.zero);

                cameraHolder = thirdPersonVirtualCamera.transform;

                if(leftWallRunningVirtualCamera == null || rightWallRunningVirtualCamera == null)
                {
                    Debug.LogWarning("Wall running virtual cameras not assigned");
                    return;
                }

                if(playerController.m_bIsWallRunning && wallRunning.wallLeft)
                {
                   leftWallRunningVirtualCamera.Priority = activeCameraPriority;
                   rightWallRunningVirtualCamera.Priority = inactiveCameraPriority;

                   thirdPersonVirtualCamera.Priority = inactiveCameraPriority;
                }
                else if(playerController.m_bIsWallRunning && wallRunning.wallRight)
                {
                    leftWallRunningVirtualCamera.Priority = inactiveCameraPriority;
                    rightWallRunningVirtualCamera.Priority = activeCameraPriority;

                    thirdPersonVirtualCamera.Priority = inactiveCameraPriority;
                }
                else
                {
                    leftWallRunningVirtualCamera.Priority = inactiveCameraPriority;
                    rightWallRunningVirtualCamera.Priority = inactiveCameraPriority;

                    thirdPersonVirtualCamera.Priority = activeCameraPriority;
                }

                UpdateBodyFacingDirection();
            }

            if(playerModeSwitcher != null && playerModeSwitcher.currentMode == PlayerMode.BallMode)
            {
                if (ballMovementFOVCoroutine != null)
                {
                    StopCoroutine(ballMovementFOVCoroutine);
                }

                ballMovementFOVCoroutine = StartCoroutine(DoBallMovementFOV());
            }
            
            // Check for sprint state changes and update FOV
            HandleSprintFOV();
        }

        private IEnumerator DoBallMovementFOV()
        {
            if(playerModeSwitcher == null || ballRollController == null)
            {
                yield break;
            }

            while(playerModeSwitcher != null && playerModeSwitcher.currentMode == PlayerMode.BallMode)
            {
                Vector3 flatVelocity = new Vector3(ballRollController.m_rigidBody.linearVelocity.x, 0f, ballRollController.m_rigidBody.linearVelocity.z);

                float speed = flatVelocity.magnitude;
                float targetFOV = Mathf.Lerp(defaultFOV, sprintFOV, speed / ballRollController.m_maxSpeed); // Increase FOV based on speed, maxing out at sprintFOV

                float smoothedFOV = Mathf.Lerp(GetCurrentCameraFOV(), targetFOV, Time.deltaTime * 5f); // Smoothly interpolate to the target FOV for a more natural effect
                SetCameraFOV(smoothedFOV);

                yield return null;
            }

            while (Mathf.Abs(GetCurrentCameraFOV() - defaultFOV) > 0.1f) // Smoothly transition back to default FOV when exiting ball mode, > 0.1f threshold to prevent unnecessary updates
            {
                float smoothedFOV = Mathf.Lerp(GetCurrentCameraFOV(), defaultFOV, Time.deltaTime * 5f);
                SetCameraFOV(smoothedFOV);
                yield return null;
            }

            // Reset FOV when exiting ball mode
            SetCameraFOV(defaultFOV);
        }

        // private void SwitchCameraMode()
        // {
        //     currentCameraMode = currentCameraMode == CameraMode.FirstPerson ? CameraMode.ThirdPerson : CameraMode.FirstPerson;
        //     UpdateCameraMode(currentCameraMode);
        // }

        // private void UpdateCameraMode(CameraMode targetCam)
        // {
        //     if(targetCam == CameraMode.FirstPerson)
        //     {
        //         firstPersonYaw = m_cameraPivot.eulerAngles.y;
        //         firstPersonYawRoot.rotation = Quaternion.Euler(0f, firstPersonYaw, 0f);

        //         //firstPersonVirtualCamera.Priority = activeCameraPriority;
        //         thirdPersonVirtualCamera.Priority = inactiveCameraPriority;
        //     }
        //     else
        //     {
        //         // Sync TP yaw to current FP yaw (or player yaw) when entering TP
        //         m_yaw = firstPersonYawRoot.eulerAngles.y;
        //         m_pitch = Mathf.Clamp(m_pitch, m_thirdPersonPitchLimits.x, m_thirdPersonPitchLimits.y);

        //         //firstPersonVirtualCamera.Priority = inactiveCameraPriority;
        //         thirdPersonVirtualCamera.Priority = activeCameraPriority;
        //     }
        // }

        // private void ApplyFirstPersonLook(Vector2 lookInput)
        // {
        //     float yawDelta = lookInput.x * firstPersonLookSensitivity.x * Time.deltaTime;;
        //     float pitchDelta = lookInput.y * firstPersonLookSensitivity.y * Time.deltaTime;;

        //     firstPersonYaw += yawDelta;
        //     firstPersonPitch = Mathf.Clamp(firstPersonPitch - pitchDelta, m_firstPersonPitchLimits.x, m_firstPersonPitchLimits.y);

        //     firstPersonYawRoot.rotation = Quaternion.Euler(0f, firstPersonYaw, 0f);
        //     firstPersonPitchPivot.localRotation = Quaternion.Euler(firstPersonPitch, 0f, 0f);
        // }

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

        public void GrappleBoostFOV()
        {
            if (grappleBoostFOVCoroutine != null)
            {
                StopCoroutine(grappleBoostFOVCoroutine);
            }

            isGrappleBoostActive = true;
            currentGrappleBoostFOV = GetResolvedFOV();
            grappleBoostFOVCoroutine = StartCoroutine(DoGrappleBoostUpFOV());
        }

        public void EndGrappleBoostFOV()
        {
            if (!isGrappleBoostActive && grappleBoostFOVCoroutine == null)
            {
                return;
            }

            if (grappleBoostFOVCoroutine != null)
            {
                StopCoroutine(grappleBoostFOVCoroutine);
            }

            currentGrappleBoostFOV = GetResolvedFOV();
            grappleBoostFOVCoroutine = StartCoroutine(DoGrappleBoostDownFOV());
        }

        private IEnumerator DoGrappleBoostUpFOV()
        {
            float initialFOV = currentGrappleBoostFOV;
            float elapsedTime = 0f;

            if (grappleBoostTransitionUpDuration <= 0f)
            {
                currentGrappleBoostFOV = grappleBoostFOV;
                ApplyResolvedFOV();
                grappleBoostFOVCoroutine = null;
                yield break;
            }

            while (elapsedTime < grappleBoostTransitionUpDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsedTime / grappleBoostTransitionUpDuration);
                float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

                currentGrappleBoostFOV = Mathf.Lerp(initialFOV, grappleBoostFOV, smoothProgress);
                ApplyResolvedFOV();
                yield return null;
            }

            currentGrappleBoostFOV = grappleBoostFOV;
            ApplyResolvedFOV();
            grappleBoostFOVCoroutine = null;
        }

        private IEnumerator DoGrappleBoostDownFOV()
        {
            float initialFOV = currentGrappleBoostFOV;
            float targetFOV = GetResolvedFOVWithoutGrapple();
            float elapsedTime = 0f;

            if (grappleBoostTransitionDownDuration <= 0f)
            {
                currentGrappleBoostFOV = targetFOV;
                isGrappleBoostActive = false;
                ApplyResolvedFOV();
                grappleBoostFOVCoroutine = null;
                yield break;
            }

            while (elapsedTime < grappleBoostTransitionDownDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsedTime / grappleBoostTransitionDownDuration);
                float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

                currentGrappleBoostFOV = Mathf.Lerp(initialFOV, targetFOV, smoothProgress);
                ApplyResolvedFOV();
                yield return null;
            }

            currentGrappleBoostFOV = targetFOV;
            isGrappleBoostActive = false;
            ApplyResolvedFOV();
            grappleBoostFOVCoroutine = null;
        }

        private void BurstFOVIncrease()
        {
            if(!playerController.m_bDashing) return;
            if(isBursting) return;

            isBursting = true;

            // Stop any existing burst FOV effect
            if (dashFOVCoroutine != null)
            {
                StopCoroutine(dashFOVCoroutine);
            }
            
            // Start new burst FOV effect
            currentDashFOV = GetResolvedFOV();
            dashFOVCoroutine = StartCoroutine(DoBurstFOV());
        }
        
        private IEnumerator DoBurstFOV()
        {
            // Store the current FOV to return to
            float originalFOV = currentDashFOV;
            float elapsedTime = 0f;

            if (burstTransitionDuration <= 0f)
            {
                currentDashFOV = GetResolvedFOVWithoutDash();
                isBursting = false;
                dashFOVCoroutine = null;
                ApplyResolvedFOV();
                yield break;
            }
            
            // Phase 1: Smooth increase to dash FOV over burstTransitionDuration
            while (elapsedTime < burstTransitionDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsedTime / burstTransitionDuration);
                
                // Use smooth curve for natural feeling
                float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);
                currentDashFOV = Mathf.Lerp(originalFOV, dashFOV, smoothProgress);
                
                ApplyResolvedFOV();
                yield return null;
            }
            
            // Ensure we're exactly at dash FOV
            currentDashFOV = dashFOV;
            ApplyResolvedFOV();
            
            // Phase 3: Smooth return to original FOV
            elapsedTime = 0f;
            float returnTargetFOV = GetResolvedFOVWithoutDash();
            while (elapsedTime < burstTransitionDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsedTime / burstTransitionDuration);
                
                // Use smooth curve for natural feeling
                float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);
                currentDashFOV = Mathf.Lerp(dashFOV, returnTargetFOV, smoothProgress);
                
                ApplyResolvedFOV();
                yield return null;
            }
            
            // Ensure we end exactly at original FOV
            currentDashFOV = returnTargetFOV;
            ApplyResolvedFOV();
            
            // Clear the coroutine reference
            dashFOVCoroutine = null;
            isBursting = false;
        }

        private void HandleSprintFOV()
        {
            if (playerController == null) return;
            if (playerModeSwitcher.currentMode == PlayerMode.BallMode) return;
            
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
            if (baseFOVCoroutine != null)
            {
                StopCoroutine(baseFOVCoroutine);
            }
            
            // Start new FOV transition
            baseFOVCoroutine = StartCoroutine(TransitionFOV(targetFOV));
        }
        
        private IEnumerator TransitionFOV(float targetFOV)
        {
            float startFOV = currentBaseFOV;
            float elapsedTime = 0f;

            if (FOVTransitionDuration <= 0f)
            {
                currentBaseFOV = targetFOV;
                ApplyResolvedFOV();
                baseFOVCoroutine = null;
                yield break;
            }
            
            while (elapsedTime < FOVTransitionDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsedTime / FOVTransitionDuration);
                
                // Use smooth curve for more natural feeling
                float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);
                currentBaseFOV = Mathf.Lerp(startFOV, targetFOV, smoothProgress);
                
                ApplyResolvedFOV();
                
                yield return null;
            }
            
            // Ensure we end exactly at target FOV
            currentBaseFOV = targetFOV;
            ApplyResolvedFOV();
            baseFOVCoroutine = null;
        }

        private float GetResolvedFOV()
        {
            float resolvedFOV = currentBaseFOV;

            if (isBursting)
            {
                resolvedFOV = Mathf.Max(resolvedFOV, currentDashFOV);
            }

            if (isGrappleBoostActive)
            {
                resolvedFOV = Mathf.Max(resolvedFOV, currentGrappleBoostFOV);
            }

            return resolvedFOV;
        }

        private float GetResolvedFOVWithoutDash()
        {
            float resolvedFOV = currentBaseFOV;

            if (isGrappleBoostActive)
            {
                resolvedFOV = Mathf.Max(resolvedFOV, currentGrappleBoostFOV);
            }

            return resolvedFOV;
        }

        private float GetResolvedFOVWithoutGrapple()
        {
            float resolvedFOV = currentBaseFOV;

            if (isBursting)
            {
                resolvedFOV = Mathf.Max(resolvedFOV, currentDashFOV);
            }

            return resolvedFOV;
        }

        private void ApplyResolvedFOV()
        {
            SetCameraFOV(GetResolvedFOV());
        }
        
        private void SetCameraFOV(float fov)
        {
            // if (firstPersonVirtualCamera != null && firstPersonVirtualCamera.Lens.FieldOfView != fov)
            // {
            //     var lens = firstPersonVirtualCamera.Lens;
            //     lens.FieldOfView = fov;
            //     firstPersonVirtualCamera.Lens = lens;
            // }
            
            if (thirdPersonVirtualCamera != null && thirdPersonVirtualCamera.Lens.FieldOfView != fov)
            {
                var lens = thirdPersonVirtualCamera.Lens;
                lens.FieldOfView = fov;
                thirdPersonVirtualCamera.Lens = lens;
            }

            if (leftWallRunningVirtualCamera != null && leftWallRunningVirtualCamera.Lens.FieldOfView != fov)
            {
                var lens = leftWallRunningVirtualCamera.Lens;
                lens.FieldOfView = fov;
                leftWallRunningVirtualCamera.Lens = lens;
            }

            if (rightWallRunningVirtualCamera != null && rightWallRunningVirtualCamera.Lens.FieldOfView != fov)
            {
                var lens = rightWallRunningVirtualCamera.Lens;
                lens.FieldOfView = fov;
                rightWallRunningVirtualCamera.Lens = lens;
            }
        }
        
        private float GetCurrentCameraFOV()
        {
            // if (currentCameraMode == CameraMode.FirstPerson && firstPersonVirtualCamera != null)
            // {
            //     return firstPersonVirtualCamera.Lens.FieldOfView;
            // }

            if (playerController != null && playerController.m_bIsWallRunning)
            {
                if (wallRunning != null && wallRunning.wallLeft && leftWallRunningVirtualCamera != null)
                {
                    return leftWallRunningVirtualCamera.Lens.FieldOfView;
                }

                if (wallRunning != null && wallRunning.wallRight && rightWallRunningVirtualCamera != null)
                {
                    return rightWallRunningVirtualCamera.Lens.FieldOfView;
                }
            }

            if (currentCameraMode == CameraMode.ThirdPerson && thirdPersonVirtualCamera != null)
            {
                return thirdPersonVirtualCamera.Lens.FieldOfView;
            }
            
            return defaultFOV;
        }

        // private void DoTilt(float zTiltAmount)
        // {
        //     cameraHolder.transform.DOLocalRotate(new Vector3(0,0, zTiltAmount), 0.25f);
        // }
    }
}