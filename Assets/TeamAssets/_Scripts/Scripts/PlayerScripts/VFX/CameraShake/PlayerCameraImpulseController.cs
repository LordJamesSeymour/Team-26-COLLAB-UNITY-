using UnityEngine;
using Unity.Cinemachine;
using Group26.Player.Movement;
using Group26.Player.Utility;

namespace Group26.Player.Camera
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(PlayerController))]
	public class PlayerCameraImpulseController : MonoBehaviour
	{
		private PlayerController playerController;
		private PlayerModeSwitcher playerModeSwitcher;
		private CinemachineImpulseSource impulseSource;

		[Header("References")]
		[SerializeField] private PlayerController m_playerController;
		[SerializeField] private PlayerModeSwitcher m_playerModeSwitcher;
		[SerializeField] private CinemachineImpulseSource m_impulseSource;

		[Header("Landing Impulse")]
		[SerializeField] private bool enableLandingImpulse = true;
		[SerializeField] private float landingMinImpactSpeed = 2.5f;
		[SerializeField] private float landingMaxImpactSpeed = 18f;
		[SerializeField] private float landingMinForce = 0.25f;
		[SerializeField] private float landingMaxForce = 1.0f;
		[SerializeField] private float landingCooldown = 0.05f;

		[Header("Grapple Impact Impulse")]
		[SerializeField] private bool enableGrappleImpactImpulse = true;
		[SerializeField] private float grappleMinImpactSpeed = 3f;
		[SerializeField] private float grappleMaxImpactSpeed = 26f;
		[SerializeField] private float grappleMinForce = 0.35f;
		[SerializeField] private float grappleMaxForce = 1.35f;
		[SerializeField] private float grappleCooldown = 0.05f;

		[Header("Context Multipliers")]
		[SerializeField] private float defaultLandingMultiplier = 1f;
		[SerializeField] private float wallRunLandingMultiplier = 0.9f;
		[SerializeField] private float swingLandingMultiplier = 1.1f;
		[SerializeField] private float dashLandingMultiplier = 1.05f;
		[SerializeField] private float ballModeLandingMultiplier = 1.25f;
		[SerializeField] private float grappleImpactMultiplier = 1.15f;

		[Header("Direction Settings")]
		[SerializeField] private Vector3 defaultLandingDirection = Vector3.down;
		[SerializeField] private bool useVelocityDirectionForGrappleImpact = true;

		private float nextLandingImpulseTime;
		private float nextGrappleImpulseTime;

		private void Awake()
		{
			playerController = m_playerController != null ? m_playerController : GetComponent<PlayerController>();
			playerModeSwitcher = m_playerModeSwitcher != null ? m_playerModeSwitcher : GetComponent<PlayerModeSwitcher>();
			impulseSource = ResolveImpulseSource();
		}

		private void Reset()
		{
			m_playerController = GetComponent<PlayerController>();
			m_playerModeSwitcher = GetComponent<PlayerModeSwitcher>();
			m_impulseSource = GetComponent<CinemachineImpulseSource>();
		}

		private void OnEnable()
		{
			if (playerController != null)
			{
				playerController.CameraShakeEvent += HandleCameraShakeEvent;
			}
		}

		private void OnDisable()
		{
			if (playerController != null)
			{
				playerController.CameraShakeEvent -= HandleCameraShakeEvent;
			}
		}

		private CinemachineImpulseSource ResolveImpulseSource()
		{
			if (m_impulseSource != null)
				return m_impulseSource;

			CinemachineImpulseSource existing = GetComponent<CinemachineImpulseSource>();
			if (existing != null)
			{
				m_impulseSource = existing;
				return existing;
			}

			CinemachineImpulseSource created = gameObject.AddComponent<CinemachineImpulseSource>();
			m_impulseSource = created;
			return created;
		}

		private void HandleCameraShakeEvent(PlayerCameraShakeEventType eventType, float impactSpeed)
		{
			float currentTime = Time.time;

			switch (eventType)
			{
				case PlayerCameraShakeEventType.Landing:
					{
						if (!enableLandingImpulse) return;
						if (currentTime < nextLandingImpulseTime) return;

						float force = EvaluateForce(impactSpeed, landingMinImpactSpeed, landingMaxImpactSpeed, landingMinForce, landingMaxForce);
						if (force <= 0f) return;

						force *= GetLandingContextMultiplier();
						EmitImpulse(force, defaultLandingDirection);

						nextLandingImpulseTime = currentTime + landingCooldown;

						break;
					}

				case PlayerCameraShakeEventType.GrappleImpact:
					{
						if (!enableGrappleImpactImpulse) return;
						if (currentTime < nextGrappleImpulseTime) return;

						float force = EvaluateForce(impactSpeed, grappleMinImpactSpeed, grappleMaxImpactSpeed, grappleMinForce, grappleMaxForce);
						if (force <= 0f) return;

						force *= grappleImpactMultiplier;

						Vector3 direction = Vector3.down;
						if (useVelocityDirectionForGrappleImpact && playerController != null && playerController.Body != null)
						{
							Vector3 velocity = playerController.Body.linearVelocity;
							if (velocity.sqrMagnitude > 0.0001f)
								direction = -velocity.normalized;
						}

						EmitImpulse(force, direction);

						nextGrappleImpulseTime = currentTime + grappleCooldown;

						break;
					}
			}
		}

		private float EvaluateForce(float impactSpeed, float minSpeed, float maxSpeed, float minForce, float maxForce)
		{
			if (impactSpeed < minSpeed)
				return 0f;

			float t = Mathf.InverseLerp(minSpeed, maxSpeed, impactSpeed);
			return Mathf.Lerp(minForce, maxForce, t);
		}

		private float GetLandingContextMultiplier()
		{
			float multiplier = defaultLandingMultiplier;

			if (playerController != null)
			{
				switch (playerController.state)
				{
					case PlayerController.MovementState.wallRunning:
						multiplier *= wallRunLandingMultiplier;
						break;

					case PlayerController.MovementState.swinging:
						multiplier *= swingLandingMultiplier;
						break;

					case PlayerController.MovementState.dashing:
						multiplier *= dashLandingMultiplier;
						break;
				}
			}

			if (playerModeSwitcher != null && playerModeSwitcher.currentMode == PlayerMode.BallMode)
			{
				multiplier *= ballModeLandingMultiplier;
			}

			return multiplier;
		}

		private void EmitImpulse(float force, Vector3 direction)
		{
			if (impulseSource == null)
				return;

			Vector3 impulseVelocity = direction.sqrMagnitude > 0.0001f
				? direction.normalized * force
				: Vector3.down * force;

			impulseSource.GenerateImpulseWithVelocity(impulseVelocity);
		}
	}
}