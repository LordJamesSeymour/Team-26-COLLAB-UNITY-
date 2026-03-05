using UnityEngine;
using Group26.Player.Input;

namespace Group26.Player.Locomotion
{
    public class PlayerBodyRotation : MonoBehaviour
    {
        private InputManager playerInput;
        [SerializeField] private Transform m_bodyTransform;
        [SerializeField] private Transform m_cameraYawTransform;

        [Header("Settings")]
        [Tooltip("Degrees per second to rotate toward target direction")]
        [SerializeField, Range(0f, 600f)] private float turnSpeedDegPerSec = 540f;

        [Tooltip("Stick magnitude required before rotating from input")]
        [SerializeField, Range(0f, 0.5f)] private float inputDeadzone = 0.1f;

        private void Awake()
        {
            playerInput = GetComponent<InputManager>();
            if (playerInput == null) Debug.LogError("No input manager found");

            if(m_bodyTransform == null) Debug.LogError("No body transform assigned");
            if(m_cameraYawTransform == null) Debug.LogError("No camera yaw transform assigned");
        }

        private void Update()
        {
            if(playerInput == null || m_bodyTransform == null || m_cameraYawTransform == null) return;

            Vector3 targetDir = GetInputDeadzone();
            if(targetDir.sqrMagnitude < 1e-4f) return;

            Quaternion targetRot = Quaternion.LookRotation(targetDir, Vector3.up);
            m_bodyTransform.rotation = Quaternion.RotateTowards(m_bodyTransform.rotation, targetRot, turnSpeedDegPerSec * Time.deltaTime);

            if (playerInput == null) return;
            Debug.Log($"MoveInput: {playerInput.MoveInput}");
        }

        private Vector3 GetInputDeadzone()
        {
            Vector2 m = playerInput.MoveInput;
            if(m.sqrMagnitude < inputDeadzone * inputDeadzone) return Vector3.zero;

            return ComputeMovementDirection(m).normalized;
        }

        private Vector3 ComputeMovementDirection(Vector2 moveInput)
        {
            Vector3 forward = m_cameraYawTransform.forward;
            forward.y = 0f;
            forward = forward.sqrMagnitude > 1e-4f ? forward.normalized : Vector3.forward;

            Vector3 right = m_cameraYawTransform.right;
            right.y = 0f;
            right = right.sqrMagnitude > 1e-4f ? right.normalized : Vector3.right;

            Vector3 worldMoveDir = forward * moveInput.y + right * moveInput.x;
            return worldMoveDir;
        }
    }
}