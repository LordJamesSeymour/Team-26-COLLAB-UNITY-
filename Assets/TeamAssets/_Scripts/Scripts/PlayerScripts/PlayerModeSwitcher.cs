using UnityEngine;
using Group26.Player.Movement;
using Group26.Player.Inputs;

namespace Group26.Player.Utility
{
	public enum PlayerMode
	{
		CapsuleMode,
		BallMode
	}

	public class PlayerModeSwitcher : MonoBehaviour
	{
		[Header("References")]
		private PlayerController playerController;
		private BallRollController ballRollController;
		private InputManager inputManager;

		[SerializeField] private GameObject capsuleModeObject;
		[SerializeField] private GameObject ballModeObject;

		[SerializeField] private Transform capsuleModeTransform;
		[SerializeField] private Transform sphereColliderTransform;

		private Rigidbody m_rigidbody;
		private GrappleGun m_grappleGunScript;

		public PlayerMode currentMode = PlayerMode.CapsuleMode;

		private void Awake()
		{
			if (playerController == null)
				playerController = GetComponent<PlayerController>();

			if (ballRollController == null)
				ballRollController = GetComponent<BallRollController>();

			if (inputManager == null)
				inputManager = GetComponent<InputManager>();

			if (capsuleModeObject == null || ballModeObject == null)
				Debug.LogError("PlayerModeSwitcher: One or more mode objects are not assigned.");

			m_grappleGunScript = GetComponent<GrappleGun>();
			if (m_grappleGunScript == null)
				Debug.LogError("Grapple gun script is not attached");

			m_rigidbody = GetComponent<Rigidbody>();
		}

		private void OnEnable()
		{
			if (inputManager != null)
				inputManager.OnModeSwitchPressed += SwitchMode;
		}

		private void OnDisable()
		{
			if (inputManager != null)
				inputManager.OnModeSwitchPressed -= SwitchMode;
		}

		private void Start()
		{
			ApplyMode(currentMode);
		}

		private void SwitchMode()
		{
			currentMode = currentMode == PlayerMode.CapsuleMode ? PlayerMode.BallMode : PlayerMode.CapsuleMode;
			ApplyMode(currentMode);
		}

		private void ApplyMode(PlayerMode mode)
		{
			switch (mode)
			{
				case PlayerMode.CapsuleMode:
					{
						ballModeObject.SetActive(false);
						capsuleModeObject.SetActive(true);

						// KEEP PlayerController enabled at all times.
						// It is the script that drives the rail system.
						if (playerController != null)
						{
							playerController.enabled = true;
							playerController.SetBallFormState(false);
						}

						if (ballRollController != null)
							ballRollController.enabled = false;

						if (m_rigidbody != null)
							m_rigidbody.angularVelocity = Vector3.zero;

						if (sphereColliderTransform != null)
							sphereColliderTransform.localRotation = Quaternion.identity;

						break;
					}

				case PlayerMode.BallMode:
					{
						Vector3 forwardDirection = capsuleModeTransform != null ? capsuleModeTransform.forward : transform.forward;
						forwardDirection.y = 0f;

						if (forwardDirection.sqrMagnitude > 0.01f)
							ballModeObject.transform.rotation = Quaternion.LookRotation(forwardDirection.normalized, Vector3.up);

						capsuleModeObject.SetActive(false);
						ballModeObject.SetActive(true);

						// IMPORTANT:
						// Do NOT disable PlayerController in ball mode.
						// It must stay enabled so rail movement can run.
						if (playerController != null)
						{
							playerController.enabled = true;
							playerController.m_bIsGrounded = false;
							playerController.SetBallFormState(true);
						}

						if (ballRollController != null)
							ballRollController.enabled = true;

						if (m_grappleGunScript != null)
							m_grappleGunScript.ForceStopGrapple();

						if (playerController != null)
							playerController.ReleaseGrappleMovement();

						break;
					}
			}
		}
	}
}