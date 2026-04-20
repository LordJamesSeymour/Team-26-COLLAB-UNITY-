using Group26.Player.Inputs;
using UnityEngine;

namespace Group26.Player.Movement
{
    public class BallRollController : MonoBehaviour
    {
        [Header("References")]
        private InputManager inputManager;
        [HideInInspector] public Rigidbody m_rigidBody;
        [SerializeField] private Transform meshToRotate;
        [SerializeField] private Transform m_cameraYawTransform;
        
        [Header("Movement Settings")]
        [SerializeField] private float m_moveForce = 30f;
        [SerializeField] public float m_maxSpeed = 14f;
        [SerializeField] private float m_airControlMultiplier = 0.4f;
        [SerializeField] private float m_groundDrag = 1.5f;

        [Header("Rolling Visuals")]
        [SerializeField] private float visualRadius = 0.5f;
        [SerializeField] private float minVisualSpeedToRoll = 0.05f;

        [Header("Jump Settings")]
        [SerializeField] private float m_jumpImpulse = 35f;

        [Header("Ground Check Settings")]
        [SerializeField] private Transform m_groundCheck;
        [SerializeField, Range(0.1f, 10f)] private float m_groundCheckDistance = 1.1f;
        [SerializeField] private LayerMask m_groundLayer;

        private void Awake()
        {
            if(m_rigidBody == null)
            {
                m_rigidBody = GetComponent<Rigidbody>();
            }
            if(inputManager == null)
            {
                inputManager = GetComponent<InputManager>();
            }
        }

        private void OnEnable()
        {
            if(inputManager != null) inputManager.OnJumpPressed += Jump;
        }

        private void OnDisable()
        {
            if(inputManager != null) inputManager.OnJumpPressed -= Jump;
        }

        private void FixedUpdate()
        {
            HandleMovement();
            ClampHorizonalSpeed();
            ApplyGroundDrag();
        }

        private void Update()
        {
            RotateVisualMesh();

            // Debug the balls speed
            //Debug.Log($"Speed: {m_rigidBody.linearVelocity.magnitude}");

            Debug.Log($"IsGrounded: {IsGrounded()}") ;
        }

        private void RotateVisualMesh()
        {
            if(meshToRotate == null) return;

            Vector3 velocity = m_rigidBody.linearVelocity;
            Vector3 flatVelocity = new Vector3(velocity.x, 0, velocity.z);

            float speed = flatVelocity.magnitude;
            if(speed < minVisualSpeedToRoll)
            {
                meshToRotate.localRotation = Quaternion.identity;
                return;
            }

            Vector3 moveDir = flatVelocity.normalized;
            Vector3 rollAxis = Vector3.Cross(Vector3.up, moveDir);

            float distance = speed * Time.deltaTime;
            float angle = (distance / visualRadius) * Mathf.Rad2Deg;

            meshToRotate.Rotate(rollAxis, angle, Space.World);
        }

        private void HandleMovement()
        {
            Vector3 input = inputManager?.MoveInput ?? Vector3.zero;

            Vector3 forwardDir = m_cameraYawTransform.forward;
            forwardDir.y = 0;
            forwardDir = forwardDir.sqrMagnitude > 0.01 ? forwardDir.normalized : Vector3.forward;

            Vector3 rightDir = m_cameraYawTransform.right;
            rightDir.y = 0;
            rightDir = rightDir.sqrMagnitude > 0.01 ? rightDir.normalized : Vector3.right;

            Vector3 moveDir = forwardDir * input.y + rightDir * input.x;
            if(moveDir.sqrMagnitude > 0.01f)
                moveDir.Normalize();

            float controlMultiplier = IsGrounded() ? 1f : m_airControlMultiplier;
            m_rigidBody.AddForce(moveDir * m_moveForce * controlMultiplier, ForceMode.Force);
        }

        private void ClampHorizonalSpeed()
        {
            Vector3 flatVelocity = new Vector3(m_rigidBody.linearVelocity.x, 0, m_rigidBody.linearVelocity.z);

            if(flatVelocity.magnitude > m_maxSpeed)
            {
                Vector3 clampedVelocity = flatVelocity.normalized * m_maxSpeed;
                m_rigidBody.linearVelocity = new Vector3(clampedVelocity.x, m_rigidBody.linearVelocity.y, clampedVelocity.z);
            }
        }

        private void ApplyGroundDrag()
        {
            m_rigidBody.linearDamping = IsGrounded() ? m_groundDrag : 0f;
        }

        private void Jump()
        {
            if(IsGrounded())
            {
                m_rigidBody.AddForce(Vector3.up * m_jumpImpulse, ForceMode.Impulse);
            }
        }

        private bool IsGrounded()
        {
            Vector3 origin = m_groundCheck != null ? m_groundCheck.position : transform.position;
            return Physics.Raycast(origin, Vector3.down, m_groundCheckDistance, m_groundLayer);
        }
    }
}