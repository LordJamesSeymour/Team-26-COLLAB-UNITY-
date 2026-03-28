using Group26.Player.Inputs;
using UnityEngine;

namespace Group26.Player.Movement
{
    public class BallRollController : MonoBehaviour
    {
        [Header("References")]
        private Rigidbody m_rigidBody;
        private InputManager inputManager;
        [SerializeField] private Transform cameraYawTransform;


        [Header("Rolling Settings")]
        [SerializeField] private float torque = 35f;
        [SerializeField] private float maxAngularSpeed = 35f;
        [SerializeField] private float jumpImpulse = 35f;

        [Header("Ground Check Settings")]
        [SerializeField] private float groundCheckDistance = 0.51f;
        [SerializeField] private LayerMask groundLayer;

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
            inputManager.OnJumpPressed += Jump;
        }

        private void OnDisable()
        {
            inputManager.OnJumpPressed -= Jump;
        }

        private void FixedUpdate()
        {
            Vector3 moveDirection = cameraYawTransform.forward * inputManager.MoveInput.y + cameraYawTransform.right * inputManager.MoveInput.x;
            moveDirection.y = 0f;
            moveDirection.Normalize();

            Vector3 torqueAxis = Vector3.Cross(Vector3.up, moveDirection);
            m_rigidBody.AddTorque(torqueAxis * torque, ForceMode.Acceleration);
        }

        private void Jump()
        {
            if(IsGrounded())
            {
                m_rigidBody.AddForce(Vector3.up * jumpImpulse, ForceMode.Impulse);
            }
        }

        private bool IsGrounded()
        {
            return Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
        }
    }
}