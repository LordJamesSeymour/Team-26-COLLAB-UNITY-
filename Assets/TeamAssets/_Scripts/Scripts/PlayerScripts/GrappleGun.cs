using UnityEngine;
using Group26.Player.Inputs;
using Group26.Player.Utility;

namespace Group26.Player.Movement
{
	public class GrappleGun : MonoBehaviour
	{
		[Header("References")]
		private InputManager InputManager;
		private PlayerController PlayerController;
		private PlayerModeSwitcher PlayerModeSwitcher;

		[SerializeField] private Transform grappleCamera;
		[SerializeField] private Transform firePoint;
		[SerializeField] private LayerMask m_grappableLayer;
		[SerializeField] private LineRenderer lineRenderer;
		[SerializeField] private Transform m_maincam;

		private Vector3 grapplePoint;

		[Header("Grappling")]
		[SerializeField] private float maxGrappleDistance;
		[SerializeField] private float grappleDelayTime;
		[SerializeField] private float straightGrappleSpeed = 35f;
		[SerializeField] private bool m_preventGrappleThroughWalls = true;
		[SerializeField] private LayerMask m_ignoredGrapplePredictionLayer;

		[Header("Prediction")]
		[SerializeField] private RaycastHit predictionHit;
		[SerializeField] private float predictionSphereCastRadius;
		[SerializeField] private Transform predictionPoint;

		[Header("Highlight Distance Colors")]
		[SerializeField] private Color m_maxDistanceColor = Color.red;
		[SerializeField] private Color m_midDistanceColor = Color.yellow;
		[SerializeField] private Color m_minDistanceColor = Color.green;

		[Header("Cooldown")]
		[SerializeField] private float grappleCooldown;
		private float grappleCooldownTimer;
		private bool m_bGrappling;
		private int _grappleToken = 0;

		[Header("Debug")]
		[SerializeField] private bool m_logGrappleCooldown = false;
		[SerializeField] private bool m_logIncorrectLayerHits = false;
		[SerializeField] private bool m_bDrawPredictionRays = false;

		private GrappleHighlightTarget m_currentHighlightTarget;

		private void Awake()
		{
			if (InputManager == null) InputManager = GetComponent<InputManager>();
			if (PlayerController == null) PlayerController = GetComponent<PlayerController>();
			if (PlayerModeSwitcher == null) PlayerModeSwitcher = GetComponent<PlayerModeSwitcher>();

			if (predictionPoint != null)
				predictionPoint.gameObject.SetActive(false);

			m_ignoredGrapplePredictionLayer = ~m_ignoredGrapplePredictionLayer;
		}

		private void OnEnable()
		{
			if (InputManager != null)
				InputManager.OnGrapplePressed += StartGrapple;
		}

		private void OnDisable()
		{
			if (InputManager != null)
				InputManager.OnGrapplePressed -= StartGrapple;

			ClearCurrentHighlight();
			CancelInvoke();
		}

		private void LateUpdate()
		{
			if (m_bGrappling && lineRenderer != null && firePoint != null)
				lineRenderer.SetPosition(0, firePoint.position);
		}

		private void FixedUpdate()
		{
			if (grappleCooldownTimer > 0f)
				grappleCooldownTimer -= Time.fixedDeltaTime;

			CheckForGrapplePoints();

			if (m_logGrappleCooldown)
				Debug.Log(grappleCooldownTimer);
		}

		private Transform GetActiveGrappleCamera()
		{
			if (grappleCamera != null)
				return grappleCamera;

			if (m_maincam != null)
				return m_maincam;

			if (UnityEngine.Camera.main != null)
				return UnityEngine.Camera.main.transform;

			return null;
		}

		private void CheckForGrapplePoints()
		{
			if (m_bGrappling)
			{
				if (predictionPoint != null)
					predictionPoint.gameObject.SetActive(false);

				ClearCurrentHighlight();
				return;
			}

			Transform activeCam = GetActiveGrappleCamera();
			if (activeCam == null)
			{
				if (predictionPoint != null)
					predictionPoint.gameObject.SetActive(false);

				predictionHit = default;
				ClearCurrentHighlight();
				return;
			}

			Physics.SphereCast(
				activeCam.position,
				predictionSphereCastRadius,
				activeCam.forward,
				out RaycastHit sphereCastHit,
				maxGrappleDistance,
				m_grappableLayer);

			Physics.Raycast(
				activeCam.position,
				activeCam.forward,
				out RaycastHit raycastHit,
				maxGrappleDistance,
				m_grappableLayer);

			Vector3 realHitPoint;

			if (raycastHit.point != Vector3.zero)
				realHitPoint = raycastHit.point;
			else if (sphereCastHit.point != Vector3.zero)
				realHitPoint = sphereCastHit.point;
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

			if (predictionHit.collider != null && realHitPoint != Vector3.zero)
			{
				GrappleHighlightTarget target = ResolveHighlightTarget(predictionHit);
				Color highlightColor = GetHighlightColor(activeCam, realHitPoint);
				SetCurrentHighlight(target, highlightColor);
			}
			else
			{
				ClearCurrentHighlight();
			}
		}

		private GrappleHighlightTarget ResolveHighlightTarget(RaycastHit hit)
		{
			if (hit.collider == null)
				return null;

			GrapplePointScript pointScript = hit.collider.GetComponent<GrapplePointScript>();
			if (pointScript == null)
				pointScript = hit.collider.GetComponentInParent<GrapplePointScript>();

			if (pointScript != null && pointScript.HighlightTarget != null)
				return pointScript.HighlightTarget;

			GrappleHighlightTarget directTarget = hit.collider.GetComponent<GrappleHighlightTarget>();
			if (directTarget != null)
				return directTarget;

			return hit.collider.GetComponentInParent<GrappleHighlightTarget>();
		}

		private Color GetHighlightColor(Transform activeCam, Vector3 hitPoint)
		{
			if (activeCam == null)
				return m_maxDistanceColor;

			float distance = Vector3.Distance(activeCam.position, hitPoint);
			float normalizedDistance = Mathf.Clamp01(distance / Mathf.Max(maxGrappleDistance, 0.0001f));

			return EvaluateDistanceColor(normalizedDistance);
		}

		private Color EvaluateDistanceColor(float normalizedDistance)
		{
			if (normalizedDistance <= 0.5f)
			{
				float t = normalizedDistance / 0.5f;
				return Color.Lerp(m_minDistanceColor, m_midDistanceColor, t);
			}
			else
			{
				float t = (normalizedDistance - 0.5f) / 0.5f;
				return Color.Lerp(m_midDistanceColor, m_maxDistanceColor, t);
			}
		}

		private void SetCurrentHighlight(GrappleHighlightTarget target, Color color)
		{
			if (m_currentHighlightTarget != target)
			{
				if (m_currentHighlightTarget != null)
					m_currentHighlightTarget.SetHighlighted(false);

				m_currentHighlightTarget = target;
			}

			if (m_currentHighlightTarget != null)
			{
				m_currentHighlightTarget.SetHighlightColor(color);
				m_currentHighlightTarget.SetHighlighted(true);
			}
		}

		private void ClearCurrentHighlight()
		{
			if (m_currentHighlightTarget != null)
				m_currentHighlightTarget.SetHighlighted(false);

			m_currentHighlightTarget = null;
		}

		private void StartGrapple()
		{
			Transform activeCam = GetActiveGrappleCamera();
			if (activeCam == null)
				return;

			if(PlayerModeSwitcher != null && PlayerModeSwitcher.currentMode != PlayerMode.CapsuleMode)
				return;

			if (grappleCooldownTimer > 0f) return;
			if (m_bGrappling) return;

			if (predictionHit.point == Vector3.zero)
			{
				PlayerController.m_bFreeze = false;
				return;
			}

			if (m_preventGrappleThroughWalls)
			{
				Transform cameraTransform = activeCam;

				RaycastHit obstaclePrevention;
				Vector3 distance = predictionHit.point - cameraTransform.position;

				Physics.Raycast(
					cameraTransform.position,
					distance.normalized,
					out obstaclePrevention,
					maxGrappleDistance,
					m_ignoredGrapplePredictionLayer);

				if (obstaclePrevention.collider != null)
				{
					if (m_bDrawPredictionRays)
					{
						Vector3 direction = obstaclePrevention.point - cameraTransform.position;
						Debug.DrawRay(cameraTransform.position, direction.normalized * maxGrappleDistance, Color.red, 100.0f);
					}

					if (obstaclePrevention.collider.gameObject != null)
					{
						if ((1 << obstaclePrevention.collider.gameObject.layer) != m_grappableLayer.value)
						{
							if (m_logIncorrectLayerHits)
							{
								Debug.Log("Grapple cancelled because it hit an object with the layer: " + (1 << obstaclePrevention.collider.gameObject.layer) + " first, instead of the expected " + m_grappableLayer.value);
								Debug.Log("Hit object is named: " + obstaclePrevention.collider.gameObject.name);
							}
							return;
						}
					}
				}
			}

			SwingGun swingGun = GetComponent<SwingGun>();
			if (swingGun != null)
				swingGun.StopSwing();

			m_bGrappling = true;
			_grappleToken++;

			CancelInvoke();
			ClearCurrentHighlight();

			PlayerController.m_bFreeze = true;
			grapplePoint = predictionHit.point;

			if (predictionPoint != null)
				predictionPoint.gameObject.SetActive(false);

			Invoke(nameof(ExecuteGrapple_InvokeWrapper), Mathf.Max(grappleDelayTime, 0f));
		}

		private void ExecuteGrapple_InvokeWrapper() => ExecuteGrapple(_grappleToken);
		private void StopGrapple_InvokeWrapper() => StopGrapple(_grappleToken);

		private void ExecuteGrapple(int token)
		{
			if (token != _grappleToken) return;

            AudioManager.instance.PlaySoundAtPoint(AudioManager.SoundType.GRAPPLE, transform.position, volume: .7f, pitchRange: .2f, spatialBlend: 0);
            PlayerController.m_bFreeze = false;
			PlayerController.GrappleToPositionStraight(grapplePoint, straightGrappleSpeed);

			float distanceToTarget = Vector3.Distance(transform.position, grapplePoint);
			float autoStopDelay = (straightGrappleSpeed > 0.01f)
				? (distanceToTarget / straightGrappleSpeed) + 0.15f
				: 1f;

			Invoke(nameof(StopGrapple_InvokeWrapper), Mathf.Max(autoStopDelay, 0.1f));
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

			if (lineRenderer != null)
				lineRenderer.enabled = false;
		}

		public Vector3 GetGrapplePoint()
		{
			return grapplePoint;
		}

		public Transform GetGunTip()
		{
			return firePoint;
		}

		public bool IsRopeActive()
		{
			return m_bGrappling;
		}
	}
}