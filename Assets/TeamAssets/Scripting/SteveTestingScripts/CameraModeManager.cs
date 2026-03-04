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

        [Header("Body Turn To Camera")]
        [SerializeField, Range(0f, 0.5f)] private float m_bodyTurnSmoothTime = 0.12f;
        [SerializeField, Range(0f, 180f)] private float m_turnWhenCameraYawOffsetExceeds = 25f; // degrees threshold to turn body

        private float m_yaw;
        private float m_pitch;
        private float m_bodyYawVel;

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
        }


        public void ApplyCameraLook(Vector2 lookInput)
        {
            m_yaw += lookInput.x * m_lookSensitivity.x * Time.deltaTime;
            m_pitch -= lookInput.y * m_lookSensitivity.y * Time.deltaTime;
            m_pitch = Mathf.Clamp(m_pitch, m_pitchLimits.x, m_pitchLimits.y);

            m_cameraPivot.rotation = Quaternion.Euler(m_pitch, m_yaw, 0f);
        }

        public void UpdateBodyFacingDirection()
        {
            float targetYaw = m_cameraPivot.eulerAngles.y;

            float currentYaw = m_playerBody.eulerAngles.y;
            float yawDifference = Mathf.DeltaAngle(currentYaw, targetYaw);

            if (Mathf.Abs(yawDifference) < m_turnWhenCameraYawOffsetExceeds) return;

            float newYaw = Mathf.SmoothDampAngle(currentYaw, targetYaw, ref m_bodyYawVel, m_bodyTurnSmoothTime);
            m_playerBody.rotation = Quaternion.Euler(0f, newYaw, 0f);
        }
    }
}