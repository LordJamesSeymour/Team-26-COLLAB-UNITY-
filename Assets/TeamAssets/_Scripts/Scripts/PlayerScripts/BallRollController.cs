using Group26.Player.Inputs;
using UnityEngine;

namespace Group26.Player.Movement
{
	public class BallRollController : MonoBehaviour
	{
		[Header("References")]
		private InputManager inputManager;
		private PlayerController playerController;
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

		private Vector3 m_lastVisualPosition;

		private void Awake()
		{
			if (m_rigidBody == null)
				m_rigidBody = GetComponent<Rigidbody>();

			if (inputManager == null)
				inputManager = GetComponent<InputManager>();

			if (playerController == null)
				playerController = GetComponent<PlayerController>();
		}

		private void OnEnable()
		{
			if (inputManager != null)
				inputManager.OnJumpPressed += Jump;

			if (playerController != null)
				playerController.SetBallFormState(true);

			m_lastVisualPosition = transform.position;
		}

		private void OnDisable()
		{
			if (inputManager != null)
				inputManager.OnJumpPressed -= Jump;

			if (playerController != null)
				playerController.SetBallFormState(false);
		}

		private void FixedUpdate()
		{
			if (playerController != null && playerController.IsOnRail)
			{
				m_rigidBody.linearDamping = 0f;
				return;
			}

			HandleMovement();
			ClampHorizonalSpeed();
			ApplyGroundDrag();
		}

		private void Update()
		{
			RotateVisualMesh();
		}

		private void RotateVisualMesh()
		{
			if (meshToRotate == null)
			{
				m_lastVisualPosition = transform.position;
				return;
			}

			Vector3 flatMotion;

			if (playerController != null && playerController.IsOnRail)
			{
				Vector3 frameDelta = transform.position - m_lastVisualPosition;
				flatMotion = new Vector3(frameDelta.x, 0f, frameDelta.z);
			}
			else
			{
				Vector3 velocity = m_rigidBody.linearVelocity;
				flatMotion = new Vector3(velocity.x, 0f, velocity.z) * Time.deltaTime;
			}

			float distance = flatMotion.magnitude;
			float speed = Time.deltaTime > 0f ? distance / Time.deltaTime : 0f;

			if (speed >= minVisualSpeedToRoll && distance > 0.00001f)
			{
				Vector3 moveDir = flatMotion.normalized;
				Vector3 rollAxis = Vector3.Cross(Vector3.up, moveDir);
				float angle = (distance / visualRadius) * Mathf.Rad2Deg;

				meshToRotate.Rotate(rollAxis, angle, Space.World);
			}

			m_lastVisualPosition = transform.position;
		}

		private void HandleMovement()
		{
			Vector3 input = inputManager?.MoveInput ?? Vector3.zero;

			Vector3 forwardDir = m_cameraYawTransform.forward;
			forwardDir.y = 0f;
			forwardDir = forwardDir.sqrMagnitude > 0.01f ? forwardDir.normalized : Vector3.forward;

			Vector3 rightDir = m_cameraYawTransform.right;
			rightDir.y = 0f;
			rightDir = rightDir.sqrMagnitude > 0.01f ? rightDir.normalized : Vector3.right;

			Vector3 moveDir = forwardDir * input.y + rightDir * input.x;
			if (moveDir.sqrMagnitude > 0.01f)
				moveDir.Normalize();

			float controlMultiplier = IsGrounded() ? 1f : m_airControlMultiplier;
			m_rigidBody.AddForce(moveDir * m_moveForce * controlMultiplier, ForceMode.Force);
		}

		private void ClampHorizonalSpeed()
		{
			Vector3 flatVelocity = new Vector3(m_rigidBody.linearVelocity.x, 0f, m_rigidBody.linearVelocity.z);

			if (flatVelocity.magnitude > m_maxSpeed)
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
			if (playerController != null && playerController.IsOnRail)
				return;

			if (IsGrounded())
			{
				m_rigidBody.AddForce(Vector3.up * m_jumpImpulse, ForceMode.Impulse);
			}
		}

		public bool IsGrounded()
		{
			Vector3 origin = m_groundCheck != null ? m_groundCheck.position : transform.position;
			return Physics.Raycast(origin, Vector3.down, m_groundCheckDistance, m_groundLayer);
		}
	}
}