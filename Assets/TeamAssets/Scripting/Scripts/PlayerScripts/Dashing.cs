using UnityEngine;
using Group26.Player.Inputs;

namespace Group26.Player.Movement
{
	public class Dashing : MonoBehaviour
	{
		[Header("References")]
		[SerializeField] Transform orientation;
		[SerializeField] Transform playerCam;
		private InputManager inputManager;
		private Rigidbody rigidBody;
		private PlayerController playerController;

		[Header("Dashing")]
		[SerializeField] float dashForce;
		[SerializeField] float dashUpwardsForce;
		[SerializeField] float maxDashYSpeed;
		[SerializeField] float dashDuration;

		[Header("Cooldown")]
		[SerializeField, Range(0f, 5f)] float dashCooldown;
		private float dashCdTimer;

		[Header("Settings")]
		[SerializeField] bool useCameraForward = true;
		[SerializeField] bool allowAllDirections = true;
		[SerializeField] bool disableGravity = true;
		[SerializeField] bool resetVel = true;

		private float horizontalinput;
		private float verticalinput;

		private void OnEnable()
		{
			if (inputManager == null)
				inputManager = GetComponent<InputManager>();
			if (rigidBody == null)
				rigidBody = GetComponent<Rigidbody>();
			if (playerController)
				playerController = GetComponent<PlayerController>();

			inputManager.OnDashPressed += Dash;
		}

		private void Start()
		{
			rigidBody = GetComponent<Rigidbody>();
			playerController = GetComponent<PlayerController>();
		}

		private void FixedUpdate()
		{
			if (dashCdTimer > 0)
				dashCdTimer -= Time.deltaTime;
		}

		private void Dash()
		{
			if (dashCdTimer > 0) return;
			else dashCdTimer = dashCooldown;

			playerController.m_bDashing = true;
			playerController.maxYSpeed = maxDashYSpeed;

			Transform forwardT;
			if (useCameraForward)
				forwardT = playerCam;
			else
				forwardT = orientation;

			Vector3 direction = GetDirection(forwardT);
			Vector3 forceToApply = direction * dashForce + orientation.up * dashUpwardsForce;

			if (disableGravity)
				rigidBody.useGravity = false;

			delayedForceToApply = forceToApply;
			Invoke(nameof(DelayedDashForce), 0.025f);
			Invoke(nameof(ResetDash), dashDuration);
		}

		private Vector3 delayedForceToApply;
		private void DelayedDashForce()
		{
			if (resetVel)
				rigidBody.linearVelocity = Vector3.zero;

			rigidBody.AddForce(delayedForceToApply, ForceMode.Impulse); 
		}

		private void ResetDash()
		{
			playerController.m_bDashing = false;
			playerController.maxYSpeed = 0;


			if (disableGravity)
				rigidBody.useGravity = true;

		}

		public void GetInput(Vector2 Inputs)
		{
			horizontalinput = Inputs.x;
			verticalinput = Inputs.y;
		}

		private Vector3 GetDirection(Transform forwardT)
		{
			Vector3 direction = new Vector3();

			if (allowAllDirections)
			{
				direction = forwardT.forward * verticalinput + forwardT.right * horizontalinput;
			}
			else
				direction = forwardT.forward;

			if (verticalinput == 0 && horizontalinput == 0)
				direction = forwardT.forward;

			return direction.normalized;
		}
	}
}
