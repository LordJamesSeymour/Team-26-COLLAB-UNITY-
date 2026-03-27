using UnityEngine;
using Group26.Player.Camera;

namespace Group26.Player.Movement
{
	public class GrappleBoosting : MonoBehaviour
	{
		[Header("References")]
		[SerializeField] Transform orientation;
		[SerializeField] Transform playerCam;
		private Rigidbody rigidBody;
		private PlayerController playerController;
		private CameraModeManager cameraModeManager;
		private GrappleGun grappleGun;

		[Header("Dashing")]
		[SerializeField] float grappleForce;
		[SerializeField] float grappleUpwardForce;
		[SerializeField] float maxGrappleYSpeed;
		[SerializeField] float grappleDuration;

		[Header("Cooldown")]
		[SerializeField, Range(0f, 5f)] float grappleCooldown;
		private float grappleCdTimer;

		[Header("Settings")]
		[SerializeField] bool useCameraForward = true;
		[SerializeField] bool allowAllDirections = true;
		[SerializeField] bool disableGravity = true;
		[SerializeField] bool resetVel = true;

		private float horizontalinput;
		private float verticalinput;

		private void Awake()
		{
			if (rigidBody == null)
				rigidBody = GetComponent<Rigidbody>();
			if (playerController == null)
				playerController = GetComponent<PlayerController>();
			if (grappleGun == null)
				grappleGun = GetComponent<GrappleGun>();
			if(cameraModeManager == null)
				cameraModeManager = GetComponent<CameraModeManager>();

			rigidBody = GetComponent<Rigidbody>();
		}

		private void FixedUpdate()
		{
			if (grappleCdTimer > 0)
				grappleCdTimer -= Time.deltaTime;
		}
		public void InvokeBoost()
		{
			GrappleDash();
		}
		private void GrappleDash()
		{
			if (grappleCdTimer > 0) return;
			else grappleCdTimer = grappleCooldown;

			playerController.BeginDashState(maxGrappleYSpeed);

			Transform forwardT;
			if (useCameraForward)
				forwardT = playerCam;
			else
				forwardT = orientation;

			Vector3 direction = GetDirection(forwardT);
			Vector3 forceToApply = direction * grappleForce + orientation.up * grappleUpwardForce;

			if (disableGravity)
				rigidBody.useGravity = false;

			delayedForceToApply = forceToApply;
			Invoke(nameof(DelayedGrappleForce), 0.125f);
			Invoke(nameof(ResetGrappleDash), grappleDuration);

			cameraModeManager?.GrappleBoostFOV();

			grappleGun.ForceStopGrapple();
		}

        private void OnCollisionEnter(Collision collision)
        {
            Debug.Log("Collided with grapple point");
        }

        private Vector3 delayedForceToApply;
		private void DelayedGrappleForce()
		{
			if (resetVel)
				rigidBody.linearVelocity = Vector3.zero;

			rigidBody.AddForce(delayedForceToApply, ForceMode.VelocityChange);
			playerController.ReleaseDashMovementLock();
		}

		private void ResetGrappleDash()
		{
			cameraModeManager?.EndGrappleBoostFOV();
			playerController.EndDashState();

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
