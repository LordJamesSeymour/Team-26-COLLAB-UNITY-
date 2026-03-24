using UnityEngine;
using Group26.Player.Camera;
using Group26.Player.Inputs;
using UnityEngine.Rendering;

namespace Group26.Player.Movement
{
	public class GrappleGun : MonoBehaviour
	{
		[Header("References")]
		private InputManager InputManager;
		private CameraModeManager cameraModeManager;
		[SerializeField] private Transform firstPersonCam; 
		[SerializeField] private Transform thirdPersonCam;
		private Transform Cam;
		[SerializeField] private Transform gunTip;
		[SerializeField] private LayerMask m_grappableLayer;
		[SerializeField] private LineRenderer lineRenderer;
		private PlayerController PlayerController;
		private Vector3 grapplePoint;
		[SerializeField] private Transform m_maincam;

		[Header("Grappling")]
		[SerializeField] private float maxGrappleDistance;
		[SerializeField] private float grappleDelayTime;
		[SerializeField] private float overshootYAxis;
		/// <summary>
		/// Testing variable to be set in editor. This toggles the prevention code where an extra ray checks if there is a wall in the way before beginning the grapple.
		/// This variable will most likely be temporary, so it can be removed after testing.
		/// </summary>
		[SerializeField] private bool m_preventGrappleThroughWalls = true;
		[SerializeField] private LayerMask m_ignoredGrapplePredictionLayer;

        [Header("Prediction")]
		[SerializeField] RaycastHit predictionHit;
		[SerializeField] float predictionSphereCastRadius;
		[SerializeField] Transform predictionPoint;

		// [Header("Miss Visual")]
		// [Tooltip("How long to show the line when the grapple misses.")]
		// [SerializeField, Range(0f, 1f)] private float missLineTime = 0.08f;

		[Header("Cooldown")]
		[SerializeField] private float grappleCooldown;
		private float grappleCooldownTimer;
		private bool m_bGrappling;
		private int _grappleToken = 0;

		[Header("Debug")]
		[SerializeField] private bool m_logGrappleCooldown = false;
		[SerializeField] private bool m_logIncorrectLayerHits = false;
		[SerializeField] private bool m_bDrawPredictionRays = false;
        private void Awake()
		{
			if (InputManager == null) InputManager = GetComponent<InputManager>();
			if (PlayerController == null) PlayerController = GetComponent<PlayerController>();
			if (cameraModeManager == null) cameraModeManager = GetComponent<CameraModeManager>();
			if(predictionPoint != null) predictionPoint.gameObject.SetActive(false);

			Cam = cameraModeManager.currentCameraMode == CameraMode.FirstPerson ? firstPersonCam : thirdPersonCam;

			//~ inverts the layermask bits
			m_ignoredGrapplePredictionLayer = ~m_ignoredGrapplePredictionLayer;
        }

		void OnEnable()
		{
			InputManager.OnGrapplePressed += StartGrapple;
			InputManager.OnCameraSwitchPressed += () => lineRenderer.enabled = false; // Hide line when switching camera modes, to avoid weird line positions due to camera changes during grapple
		}

		void OnDisable()
		{
			InputManager.OnGrapplePressed -= StartGrapple;
			InputManager.OnCameraSwitchPressed -= () => lineRenderer.enabled = false;
			CancelInvoke();
		}

		private void LateUpdate()
		{
			if (m_bGrappling)
				lineRenderer.SetPosition(0, gunTip.position);
		}

		private void FixedUpdate()
		{
			if (grappleCooldownTimer > 0)
				grappleCooldownTimer -= Time.deltaTime;

			CheckForGrapplePoints();

			if (m_logGrappleCooldown)
			{
                Debug.Log(grappleCooldownTimer);
            }
        }

		private void CheckForGrapplePoints()
		{
			if(m_bGrappling)
			{
				if(predictionPoint != null)
					predictionPoint.gameObject.SetActive(false);
				return;
			}

			Cam = cameraModeManager.currentCameraMode == CameraMode.FirstPerson ? firstPersonCam : thirdPersonCam;

			RaycastHit sphereCastHit;
			Physics.SphereCast(Cam.position, predictionSphereCastRadius, Cam.forward, out sphereCastHit, maxGrappleDistance, m_grappableLayer);

			RaycastHit raycastHit; 
			Physics.Raycast(Cam.position, Cam.forward, out raycastHit, maxGrappleDistance, m_grappableLayer);

			Vector3 realHitPoint;

			// Option 1 - Direct hit
			if (raycastHit.point != Vector3.zero)
				realHitPoint = raycastHit.point;

			// Option 2 - Indirect (predicted) hit
			else if (sphereCastHit.point != Vector3.zero) // Do we need 2 Casts if sphere is doing most of the work?
				realHitPoint = sphereCastHit.point;

			// Option 3 - Miss
			else
				realHitPoint = Vector3.zero;

			if (realHitPoint != Vector3.zero)
			{
				if (predictionPoint != null)
				{
					predictionPoint.gameObject.SetActive(true);
					predictionPoint.position = realHitPoint;
				}
			}
			else
			{
				if (predictionPoint != null)
					predictionPoint.gameObject.SetActive(false);
			}

			predictionHit = raycastHit.point == Vector3.zero ? sphereCastHit : raycastHit;
		}

		void StartGrapple()
		{
			Cam = cameraModeManager.currentCameraMode == CameraMode.FirstPerson ? firstPersonCam : thirdPersonCam;

			if (grappleCooldownTimer > 0) return;
			if (m_bGrappling) return;

			// Use the SAME cached prediction logic as SwingGun
			if (predictionHit.point == Vector3.zero)
			{
				PlayerController.m_bFreeze = false;
				return;
			}

			if (m_preventGrappleThroughWalls)
			{
				Transform camera = null;
				if(m_maincam != null)
				{
					camera = m_maincam;
				}
				else
				{
                    Debug.LogWarning("No main cam reference set in the GrappleGun script, obstacle prevent ray may be inaccurate");
                    camera = Cam;
				}

				//casts a ray to the predicted grapple point to check for obstacles in the way and prevent grappling through walls
				RaycastHit obstaclePrevention;
				Vector3 distance = predictionHit.point - camera.position;
                Physics.Raycast(camera.position, distance.normalized, out obstaclePrevention, maxGrappleDistance,m_ignoredGrapplePredictionLayer);
                if (obstaclePrevention.collider != null)
                {
                    if (m_bDrawPredictionRays)
                    {
						Vector3 direction = obstaclePrevention.point - camera.position;
                        Debug.DrawRay(camera.position,direction.normalized * maxGrappleDistance,Color.red, 100.0f);
                    }

                    if (obstaclePrevention.collider.gameObject != null)
                    {
                        //layers need to be bit shifted to the left by 1 to be compared with a layer mask
                        if ((1 << obstaclePrevention.collider.gameObject.layer) != m_grappableLayer.value)
                        {
                            if (m_logIncorrectLayerHits)
                            {
                                Debug.Log("Grapple cancelled because it hit an object with the layer: " + (1 << obstaclePrevention.collider.gameObject.layer) + " first, instead of the expected " + m_grappableLayer.value.ToString() + " layer");
                                Debug.Log("Hit object is named: " + obstaclePrevention.collider.gameObject.name);
                            }
                            //PlayerController.m_bFreeze = false;
                            return;
                        }
                    }
                }
            }

            GetComponent<SwingGun>().StopSwing();

			m_bGrappling = true;
			_grappleToken++;

			CancelInvoke();

			PlayerController.m_bFreeze = true;

			// bool didHit = Physics.Raycast(Cam.position, Cam.forward, out RaycastHit hit, maxGrappleDistance, m_grappableLayer);
			// grapplePoint = didHit ? hit.point : (Cam.position + Cam.forward * maxGrappleDistance);

			// lineRenderer.enabled = true;
			// lineRenderer.SetPosition(0, gunTip.position);
			// lineRenderer.SetPosition(1, grapplePoint);

			grapplePoint = predictionHit.point;

			if(predictionPoint != null)
				predictionPoint.gameObject.SetActive(false);

			int tokenAtSchedule = _grappleToken;

			Invoke(nameof(ExecuteGrapple_InvokeWrapper), Mathf.Max(grappleDelayTime, 0f));

			// if(grappleDelayTime <= 0)
			// {
			// 	if (didHit) Invoke(nameof(ExecuteGrapple_InvokeWrapper), grappleDelayTime);
			// 	else // Display line for a moment, then stop grapple
			// 	{
			// 		PlayerController.m_bFreeze = false;

			// 		//float t = Mathf.Max(missLineTime, 0.01f); // Guarantees a frame at least for visual feedbcak

			// 		// Stop after a short visual delay, not grappleDelayTime
			// 		Invoke(nameof(StopGrapple_InvokeWrapper), t);
			// 	}
			// }
		}

		private void ExecuteGrapple_InvokeWrapper() => ExecuteGrapple(_grappleToken);
		private void StopGrapple_InvokeWrapper() => StopGrapple(_grappleToken);

		void ExecuteGrapple(int token)
		{
			if(token != _grappleToken) return;

			PlayerController.m_bFreeze = false;

			Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);

			float grapplePointRelativeYPos = grapplePoint.y - lowestPoint.y;
			float highestPointOnArc = grapplePointRelativeYPos + overshootYAxis;

			if (grapplePointRelativeYPos < 0) highestPointOnArc = overshootYAxis;

			PlayerController.JumpToPosition(grapplePoint, highestPointOnArc);

			Invoke(nameof(StopGrapple_InvokeWrapper), 1f);
		}

		public void ForceStopGrapple()
		{
			StopGrapple(_grappleToken);
		}

		private void StopGrapple(int token)
		{
			if (token != _grappleToken) return;

			PlayerController.m_bFreeze = false;
			m_bGrappling = false;
			grappleCooldownTimer = grappleCooldown;
			lineRenderer.enabled = false;
		}

		public Vector3 GetGrapplePoint()
		{
			return grapplePoint;
		}

		public Transform GetGunTip()
		{
			return gunTip;
		}

		public bool IsRopeActive()
		{
			return m_bGrappling;
		}
	}
}