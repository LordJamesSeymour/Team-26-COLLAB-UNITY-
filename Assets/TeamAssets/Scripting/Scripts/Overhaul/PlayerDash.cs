using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
    public class PlayerDash : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform m_cameraYawSource; // CameraPivot or camera (yaw source)
        [SerializeField] private InputManager2 inputManager;        // your input manager

        [SerializeField] private PlayerController playerController; // reference to your player controller (for checking states, optional)

        [Header("Dash")]
        [SerializeField] private float m_dashImpulse = 16f;     // horizontal speed during dash
        [SerializeField] private float m_dashDuration = 0.12f;
        [SerializeField] private float m_dashCooldown = 0.5f;

        [Header("Direction")]
        [SerializeField] private float m_inputDeadzone = 0.1f;
        [SerializeField] private bool m_allowAllDirections = true;

        [Header("Tuning")]
        [Tooltip("If true, clears existing horizontal velocity before applying dash impulse (more consistent punch).")]
        [SerializeField] private bool m_resetHorizontalVelocity = true;

        [Tooltip("If true, prevents the dash impulse from adding upward/downward speed changes.")]
        [SerializeField] private bool m_preserveVerticalVelocity = true;

        private Rigidbody _rb;
        private float _dashEndTime;
        private float _nextDashTime;
        private bool _isDashing;
        private Vector3 _dashDir; // stored world-space direction (horizontal)

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();

            if (inputManager == null) inputManager = GetComponent<InputManager2>();
            if (m_cameraYawSource == null && Camera.main != null) m_cameraYawSource = Camera.main.transform;

            if (inputManager == null) Debug.LogError("PlayerDash: InputManager not found.", this);
            if (m_cameraYawSource == null) Debug.LogError("PlayerDash: Camera yaw source not assigned.", this);
        }

        public bool IsDashing => _isDashing;

        private void OnEnable()
        {
            // Adapt this to your actual dash input event/callback
            inputManager.OnDashPressed += TryDash;
        }

        private void OnDisable()
        {
            inputManager.OnDashPressed -= TryDash;
        }

        // Call this from your input code when dash is pressed
        public void TryDash()
        {
            if (Time.time < _nextDashTime) return;
            if (_isDashing) return;

            Vector3 dir = GetDashDirection();
            if (dir.sqrMagnitude < 1e-6f) return;

            _nextDashTime = Time.time + m_dashCooldown;
            _dashEndTime = Time.time + m_dashDuration;
            _isDashing = true;

            if (playerController != null)
                playerController.m_bIsDashing = true;

            // Prepare velocity
            Vector3 v = _rb.linearVelocity;

            if (m_resetHorizontalVelocity)
                v = new Vector3(0f, v.y, 0f);

            _rb.linearVelocity = v;

            // Apply impulse (punch)
            _rb.AddForce(dir * m_dashImpulse, ForceMode.Impulse);
        }

        private void FixedUpdate()
        {
            if (!_isDashing) return;

            if (m_preserveVerticalVelocity)
            {
                // Prevent dash from messing with jump arcs too much (optional)
                Vector3 v = _rb.linearVelocity;
                // NOTE: If you want to *fully* preserve vertical, you'd need to store pre-dash y.
                // This just avoids accidentally injecting vertical in the dash direction (dir.y should be 0 anyway).
                _rb.linearVelocity = new Vector3(v.x, v.y, v.z);
            }

            if (Time.time >= _dashEndTime)
            {
                _isDashing = false;
                if (playerController != null)
                    playerController.m_bIsDashing = false;
            }
        }

        private Vector3 GetDashDirection()
        {
            // If you want “dash where you’re moving”, use MoveInput.
            // If no input, dash forward.
            Vector2 move = (inputManager != null) ? inputManager.MoveInput : Vector2.zero;

            Vector3 forward = m_cameraYawSource.forward;
            forward.y = 0f;
            forward = forward.sqrMagnitude > 1e-6f ? forward.normalized : Vector3.forward;

            Vector3 right = m_cameraYawSource.right;
            right.y = 0f;
            right = right.sqrMagnitude > 1e-6f ? right.normalized : Vector3.right;

            Vector3 dir;

            if (m_allowAllDirections && move.sqrMagnitude > m_inputDeadzone * m_inputDeadzone)
            {
                // camera-relative move direction
                dir = forward * move.y + right * move.x;
            }
            else
            {
                // default dash forward relative to camera yaw
                dir = forward;
            }

            dir.y = 0f;
            return dir.sqrMagnitude > 1e-6f ? dir.normalized : Vector3.zero;
        }
    }