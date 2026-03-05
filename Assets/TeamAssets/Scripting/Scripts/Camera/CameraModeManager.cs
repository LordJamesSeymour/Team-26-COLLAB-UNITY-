using Unity.Cinemachine;
using UnityEngine;

namespace Group26.Player.Camera
{
    public class CameraModeManager : MonoBehaviour
    {
        private Input.InputManager playerInput;

        [Header("Camera References")]
        [SerializeField] private Transform m_playerBody;
        [SerializeField] private Transform m_cameraPivot;
        [SerializeField] private CinemachineCamera m_virtualCamera;

        [Header("Camera Settings")]
        [SerializeField] private Vector2 m_lookSensitivity = Vector2.one;
        [SerializeField] private Vector2 m_pitchLimits = new Vector2(-60f, 80f);
        
        [Header("Sprint Field of View")]
        [SerializeField] private float m_normalFOV = 60f;
        [SerializeField] private float m_sprintFOV = 70f;
        [SerializeField] private float m_fovBlendLength = 0.25f;

        [Header("Body Turn To Camera")]
        [SerializeField, Range(0f, 0.5f)] private float m_bodyTurnSmoothTime = 0.12f;
        [SerializeField, Range(0f, 180f)] private float m_turnWhenCameraYawOffsetExceeds = 25f; // degrees threshold to turn body

        private float m_yaw;
        private float m_pitch;
        private float m_bodyYawVel;
        
        // FOV Transition variables
        private bool m_isTransitioningFOV = false;
        private bool m_targetSprintState = false;
        private float m_fovTransitionTime = 0f;
        private float m_startFOV;
        private float m_targetFOV;

        private void Awake()
        {
            playerInput = GetComponent<Input.InputManager>();
            if(playerInput == null) Debug.LogError("No input manager found");

            if (m_cameraPivot == null) Debug.LogError("CameraPivot not assigned");
            if (m_playerBody == null) Debug.LogError("PlayerBody not assigned");
            if (m_virtualCamera == null) Debug.LogError("VirtualCamera not assigned");

            if (m_cameraPivot != null)
            {
                var euler = m_cameraPivot.rotation.eulerAngles;
                m_yaw = euler.y;
                m_pitch = euler.x;
            }

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            ApplyCameraLook(playerInput.LookInput);
            UpdateBodyFacingDirection();
            SprintFieldOfView(playerInput.isSprinting);
            UpdateFOVTransition();
        }

        private void ApplyCameraLook(Vector2 lookInput)
        {
            m_yaw += lookInput.x * m_lookSensitivity.x * Time.deltaTime;
            m_pitch -= lookInput.y * m_lookSensitivity.y * Time.deltaTime;
            m_pitch = Mathf.Clamp(m_pitch, m_pitchLimits.x, m_pitchLimits.y);

            m_cameraPivot.rotation = Quaternion.Euler(m_pitch, m_yaw, 0f);
        }

        private void UpdateBodyFacingDirection()
        {
            float targetYaw = m_cameraPivot.eulerAngles.y;

            float currentYaw = m_playerBody.eulerAngles.y;
            float yawDifference = Mathf.DeltaAngle(currentYaw, targetYaw);

            if (Mathf.Abs(yawDifference) < m_turnWhenCameraYawOffsetExceeds) return;

            float newYaw = Mathf.SmoothDampAngle(currentYaw, targetYaw, ref m_bodyYawVel, m_bodyTurnSmoothTime);
            m_playerBody.rotation = Quaternion.Euler(0f, newYaw, 0f);
        }

        private void SprintFieldOfView(bool isSprinting)
        {
            // Only start a new transition if the sprint state has changed
            if (m_targetSprintState != isSprinting)
            {
                m_targetSprintState = isSprinting;
                StartFOVTransition(isSprinting);
            }
        }
        
        private void StartFOVTransition(bool toSprint)
        {
            m_startFOV = m_virtualCamera.Lens.FieldOfView;
            m_targetFOV = toSprint ? m_sprintFOV : m_normalFOV;
            m_fovTransitionTime = 0f;
            m_isTransitioningFOV = true;
        }
        
        private void UpdateFOVTransition()
        {
            if (!m_isTransitioningFOV) return;
            
            m_fovTransitionTime += Time.deltaTime;
            float normalizedTime = m_fovTransitionTime / m_fovBlendLength;
            
            if (normalizedTime >= 1f)
            {
                // Transition complete
                m_virtualCamera.Lens.FieldOfView = m_targetFOV;
                m_isTransitioningFOV = false;
            }
            else
            {
                // Smooth interpolation using smoothstep for eased transition
                float smoothedTime = Mathf.SmoothStep(0f, 1f, normalizedTime);
                m_virtualCamera.Lens.FieldOfView = Mathf.Lerp(m_startFOV, m_targetFOV, smoothedTime);
            }
        }
    }
}