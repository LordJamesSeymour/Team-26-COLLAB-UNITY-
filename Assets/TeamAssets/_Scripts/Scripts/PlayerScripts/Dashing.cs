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

		private int dashToken = 0;

		private float horizontalinput;
		private float verticalinput;

		private Vector3 delayedForceToApply;

		private void Awake()
		{
			CacheReferences();
		}

		private void OnEnable()
		{
			CacheReferences();

			if (inputManager != null)
				inputManager.OnDashPressed += Dash;
		}

		private void OnDisable()
		{
			if (inputManager != null)
				inputManager.OnDashPressed -= Dash;

			ForceCancelDash(false);
		}

		private void Start()
		{
			CacheReferences();
		}

		private void CacheReferences()
		{
			if (inputManager == null)
				inputManager = GetComponent<InputManager>();

			if (rigidBody == null)
				rigidBody = GetComponent<Rigidbody>();

			if (playerController == null)
				playerController = GetComponent<PlayerController>();
		}

		private void FixedUpdate()
		{
			if (dashCdTimer > 0)
				dashCdTimer -= Time.fixedDeltaTime;
		}

		public void InvokeDash()
		{
			Dash();
		}

		private void Dash()
		{
			CacheReferences();

			if (dashCdTimer > 0f) return;
			if (dashToken != 0) return;
			if (rigidBody == null || playerController == null) return;
			if (playerController.m_bActiveGrapple || playerController.m_bActiveSwing || playerController.m_bOnRail) return;

			dashCdTimer = dashCooldown;
			dashToken++;

			playerController.BeginDashState(maxDashYSpeed);

			Transform forwardT = GetDashForwardTransform();
			if (forwardT == null)
			{
				ForceCancelDash(true);
				return;
			}

			Vector3 direction = GetDirection(forwardT);
			Vector3 upDirection = orientation != null ? orientation.up : transform.up;
			Vector3 forceToApply = direction * dashForce + upDirection * dashUpwardsForce;

			if (disableGravity)
				rigidBody.useGravity = false;

			delayedForceToApply = forceToApply;
			Invoke(nameof(DelayedDashForce), 0.125f);
			Invoke(nameof(ResetDash), dashDuration);
		}

		private Transform GetDashForwardTransform()
		{
			if (!useCameraForward)
				return orientation != null ? orientation : transform;

			if (playerCam != null)
				return playerCam;

			UnityEngine.Camera mainCamera = UnityEngine.Camera.main;
			if (mainCamera != null)
				return mainCamera.transform;

			return orientation != null ? orientation : transform;
		}

		private void DelayedDashForce()
		{
			if (dashToken == 0) return;
			if (rigidBody == null) return;

			// If grapple started during the dash startup window, grapple wins.
			// This prevents the delayed dash force from zeroing velocity or fighting the grapple.
			if (playerController != null && playerController.m_bActiveGrapple)
				return;

			if (resetVel)
				rigidBody.linearVelocity = Vector3.zero;

			rigidBody.AddForce(delayedForceToApply, ForceMode.VelocityChange);
		}

		private void ResetDash()
		{
			if (playerController != null)
				playerController.EndDashState();

			dashToken = 0;
			delayedForceToApply = Vector3.zero;

			if (disableGravity && rigidBody != null)
				rigidBody.useGravity = true;
		}

		public bool IsDashingActive()
		{
			return dashToken != 0 || IsInvoking(nameof(DelayedDashForce)) || IsInvoking(nameof(ResetDash));
		}

		public void ForceCancelDash(bool keepCooldown = true)
		{
			CancelInvoke(nameof(DelayedDashForce));
			CancelInvoke(nameof(ResetDash));

			delayedForceToApply = Vector3.zero;
			dashToken = 0;

			if (!keepCooldown)
				dashCdTimer = 0f;

			if (playerController != null)
				playerController.EndDashState();

			if (disableGravity && rigidBody != null)
				rigidBody.useGravity = true;
		}

		public void GetInput(Vector2 Inputs)
		{
			horizontalinput = Inputs.x;
			verticalinput = Inputs.y;
		}

		private Vector3 GetDirection(Transform forwardT)
		{
			Vector3 direction;

			if (allowAllDirections)
			{
				direction = forwardT.forward * verticalinput + forwardT.right * horizontalinput;
			}
			else
			{
				direction = forwardT.forward;
			}

			if (verticalinput == 0 && horizontalinput == 0)
				direction = forwardT.forward;

			direction.y = Mathf.Clamp(direction.y, -1f, 1f);

			if (direction.sqrMagnitude < 0.0001f)
				return transform.forward;

			return direction.normalized;
		}
	}
}
