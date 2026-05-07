using UnityEngine;
using Group26.Player.Inputs;
using Group26.Player.Camera;
using Group26.Player.Utility;

namespace Group26.Player.Movement
{
	public class SwingGun : MonoBehaviour
	{
		[Header("References")]
		private InputManager inputManager;
		private CameraModeManager cameraModeManager;

		private PlayerModeSwitcher PlayerModeSwitcher;
		private BallRollController ballRollController;

		[SerializeField] private Transform thirdPersonCam;
		private Transform Cam;
		public Transform firePoint;
		[SerializeField] private Transform player;
		[SerializeField] private LayerMask m_grappableLayer;
		private PlayerController playerController;

		[Header("Swinging")]
		[SerializeField] private float maxSwingDistance = 25f;
		private Vector3 swingPoint;
		[HideInInspector] public SpringJoint joint;
		[SerializeField] private bool m_bpreventSwingingThroughWalls = true;

		[Header("Momentum Preservation")]
		[SerializeField] private bool preserveMomentumOnSwingStart = true;
		[SerializeField] private float momentumRestoreMultiplier = 1f;
		[SerializeField] private bool preserveAngularMomentumOnSwingStart = true;

		[Header("ODMGear")]
		[SerializeField] private Transform Orientation;
		private Rigidbody rigidBody;
		[SerializeField] private float horizontalThrustForce;
		[SerializeField] private float forwardThrustForce;
		[SerializeField] private float extendedCableSpeed;

		[Header("Prediction")]
		[SerializeField] private RaycastHit predictionHit;
		[SerializeField] private float predictionSphereCastRadius;
		private float predictionDefaultSphereCastRadius;
		[SerializeField] private Transform predictionPoint;
		[SerializeField] private LayerMask m_ignoredSwingPredictionLayer;
		[SerializeField] private Transform m_maincam;

		[Header("Debug")]
		[SerializeField] private bool m_bDrawPredictionRays = false;
		[SerializeField] private bool m_bLogIncorrectLayerHits = false;

		private Vector2 m_vMoveInput;
		private bool m_bClimbingRope;
		private bool m_reenableBallRollAfterSwing;

		private Vector3 m_cachedPreSwingLinearVelocity;
		private Vector3 m_cachedPreSwingAngularVelocity;

		private void Awake()
		{
			rigidBody = GetComponent<Rigidbody>();
			playerController = GetComponent<PlayerController>();
			inputManager = GetComponent<InputManager>();
			cameraModeManager = GetComponent<CameraModeManager>();
			PlayerModeSwitcher = GetComponent<PlayerModeSwitcher>();
			ballRollController = GetComponent<BallRollController>();

			if (rigidBody == null) Debug.LogError("No rigidbody found on SwingGun object.");
			if (playerController == null) Debug.LogError("No PlayerController found on SwingGun object.");

			swingPoint = firePoint.position;
			predictionDefaultSphereCastRadius = predictionSphereCastRadius;

			m_ignoredSwingPredictionLayer = ~m_ignoredSwingPredictionLayer;
		}

		private void OnEnable()
		{
			inputManager.OnSwingStarted += StartSwing;
			inputManager.OnSwingStopped += StopSwing;
		}

		private void OnDisable()
		{
			inputManager.OnSwingStarted -= StartSwing;
			inputManager.OnSwingStopped -= StopSwing;

			StopSwing();
		}

		private void FixedUpdate()
		{
			GetInput(inputManager.MoveInput);
			CheckForSwingPoints();

			if (joint == null)
				return;

			if (m_bClimbingRope)
			{
				Vector3 directionToPoint = swingPoint - transform.position;
				rigidBody.AddForce(directionToPoint.normalized * forwardThrustForce * Time.fixedDeltaTime, ForceMode.Force);

				float distanceFromPoint = Vector3.Distance(transform.position, swingPoint);
				joint.maxDistance = distanceFromPoint * 0.8f;
				joint.minDistance = distanceFromPoint * 0.25f;
			}

			ApplySwingInput();
		}

		public void WallRunPredictionSphere(float Increase)
		{
			predictionSphereCastRadius = Increase;
		}

		public void PredictionSphereDefault()
		{
			predictionSphereCastRadius = predictionDefaultSphereCastRadius;
		}

		private void CacheMomentumBeforeSwing()
		{
			if (rigidBody == null)
				return;

			m_cachedPreSwingLinearVelocity = rigidBody.linearVelocity;
			m_cachedPreSwingAngularVelocity = rigidBody.angularVelocity;
		}

		private void RestoreMomentumAfterSwingAttach()
		{
			if (rigidBody == null)
				return;

			if (preserveMomentumOnSwingStart)
				rigidBody.linearVelocity = m_cachedPreSwingLinearVelocity * momentumRestoreMultiplier;

			if (preserveAngularMomentumOnSwingStart)
				rigidBody.angularVelocity = m_cachedPreSwingAngularVelocity;
		}

		private void PrepareRigidbodyForSwing()
		{
			if (rigidBody == null)
				return;

			if (rigidBody.isKinematic)
				rigidBody.isKinematic = false;

			rigidBody.detectCollisions = true;
			rigidBody.useGravity = true;
			rigidBody.WakeUp();
		}

		private void SuspendBallRollForSwing()
		{
			m_reenableBallRollAfterSwing = false;

			if (ballRollController != null && ballRollController.enabled)
			{
				m_reenableBallRollAfterSwing = true;
				ballRollController.enabled = false;
			}
		}

		private void RestoreBallRollAfterSwing()
		{
			if (ballRollController == null)
				return;

			if (!m_reenableBallRollAfterSwing)
				return;

			if (PlayerModeSwitcher != null && PlayerModeSwitcher.currentMode == PlayerMode.BallMode)
				ballRollController.enabled = true;

			m_reenableBallRollAfterSwing = false;
		}

		void CheckForSwingPoints()
		{
			if (joint != null) return;

			Cam = thirdPersonCam;

			RaycastHit sphereCastHit;
			Physics.SphereCast(Cam.position, predictionSphereCastRadius, Cam.forward, out sphereCastHit, maxSwingDistance, m_grappableLayer);

			RaycastHit raycastHit;
			Physics.Raycast(Cam.position, Cam.forward, out raycastHit, maxSwingDistance, m_grappableLayer);

			Vector3 realHitPoint;

			if (raycastHit.point != Vector3.zero)
				realHitPoint = raycastHit.point;
			else if (sphereCastHit.point != Vector3.zero)
				realHitPoint = sphereCastHit.point;
			else
				realHitPoint = Vector3.zero;

			if (realHitPoint != Vector3.zero)
			{
				predictionPoint.gameObject.SetActive(true);
				predictionPoint.position = realHitPoint;
			}
			else
			{
				predictionPoint.gameObject.SetActive(false);
			}

			predictionHit = raycastHit.point == Vector3.zero ? sphereCastHit : raycastHit;
		}

		private void ApplySwingInput()
		{
			if (m_vMoveInput == Vector2.zero || joint == null)
				return;

			if (m_vMoveInput.y > 0f)
				rigidBody.AddForce(Orientation.forward * forwardThrustForce * Time.fixedDeltaTime, ForceMode.Force);

			if (m_vMoveInput.x < 0f)
				rigidBody.AddForce(-Orientation.right * horizontalThrustForce * Time.fixedDeltaTime, ForceMode.Force);

			if (m_vMoveInput.x > 0f)
				rigidBody.AddForce(Orientation.right * horizontalThrustForce * Time.fixedDeltaTime, ForceMode.Force);
		}

		private void StartSwing()
		{
			if (predictionHit.point == Vector3.zero) return;

			if (PlayerModeSwitcher != null && PlayerModeSwitcher.currentMode != PlayerMode.BallMode)
				return;

			if (m_bpreventSwingingThroughWalls)
			{
				Transform camera = null;
				if (m_maincam != null)
				{
					camera = m_maincam;
				}
				else
				{
					Debug.LogWarning("No main cam reference set in the SwingGun script, obstacle prevent ray may be inaccurate");
					camera = Cam;
				}

				RaycastHit obstaclePrevention;
				Vector3 distance = predictionHit.point - camera.position;
				Physics.Raycast(camera.position, distance.normalized, out obstaclePrevention, maxSwingDistance, m_ignoredSwingPredictionLayer);
				if (obstaclePrevention.collider != null)
				{
					if (m_bDrawPredictionRays)
					{
						Vector3 direction = obstaclePrevention.point - camera.position;
						Debug.DrawRay(camera.position, direction.normalized * maxSwingDistance, Color.red, 100.0f);
					}

					if (obstaclePrevention.collider.gameObject != null)
					{
						if ((1 << obstaclePrevention.collider.gameObject.layer) != m_grappableLayer.value)
						{
							if (m_bLogIncorrectLayerHits)
							{
								Debug.Log("Swing cancelled because it hit an object with the layer: " + (1 << obstaclePrevention.collider.gameObject.layer) + " first, instead of the expected " + m_grappableLayer.value.ToString() + " layer");
								Debug.Log("Hit object is named: " + obstaclePrevention.collider.gameObject.name);
							}
							return;
						}
					}
				}
			}

			GrappleGun grappleGun = GetComponent<GrappleGun>();
			if (grappleGun != null)
				grappleGun.ForceStopGrapple();

			playerController.ResetRestrictions();

			CacheMomentumBeforeSwing();
			SuspendBallRollForSwing();
			PrepareRigidbodyForSwing();

			AudioManager.instance.PlaySoundAtPoint(AudioManager.SoundType.GRAPPLE, transform.position, volume: .7f, pitchRange: .2f, spatialBlend: 0);

			playerController.m_bActiveSwing = true;
			swingPoint = predictionHit.point;
			joint = player.gameObject.AddComponent<SpringJoint>();
			joint.autoConfigureConnectedAnchor = false;
			joint.connectedAnchor = swingPoint;

			float distanceFromPoint = Vector3.Distance(player.position, swingPoint);

			joint.maxDistance = distanceFromPoint * 0.8f;
			joint.minDistance = distanceFromPoint * 0.25f;

			joint.spring = 4.5f;
			joint.damper = 7f;
			joint.massScale = 4.5f;

			RestoreMomentumAfterSwingAttach();
		}

		public void StopSwing()
		{
			if (playerController != null)
				playerController.m_bActiveSwing = false;

			m_bClimbingRope = false;
			m_vMoveInput = Vector2.zero;
			swingPoint = firePoint.position;

			if (joint != null)
			{
				Destroy(joint);
				joint = null;
			}

			RestoreBallRollAfterSwing();
		}

		private void GetInput(Vector2 inputs)
		{
			m_vMoveInput = inputs;
		}

		public Vector3 GetSwingPoint()
		{
			return swingPoint;
		}

		public bool IsSwinging()
		{
			return joint != null;
		}
	}
}