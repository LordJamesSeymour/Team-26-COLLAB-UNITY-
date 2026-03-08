using UnityEngine;

public class Grappling : MonoBehaviour
{
	[Header("References")]
	private InputManager2 InputManager;
	[SerializeField] Transform Cam;
	[SerializeField] Transform gunTip;
	[SerializeField] LayerMask m_grappableLayer;
	private PlayerController PlayerController;
	private Vector3 grapplePoint;

	[Header("Grappling")]
	[SerializeField] float maxGrappleDistance;
	[SerializeField] float grappleDelayTime;
	[SerializeField] float overshootYAxis;

	[Header("Prediction")]
	[SerializeField] RaycastHit predictionHit;
	[SerializeField] float predictionSphereCastRadius;
	[SerializeField] Transform predictionPoint;

	[Header("Miss Visual")]
	[Tooltip("How long to show the line when the grapple misses.")]
	[SerializeField, Range(0f, 1f)] float missLineTime = 0.08f;

	[Header("Cooldown")]
	[SerializeField] float grappleCooldown;
	private float grappleCooldownTimer;
	private bool m_bGrappling;
	private int _grappleToken;

	private void Awake()
	{
		if (InputManager == null) InputManager = GetComponent<InputManager2>();
		if (PlayerController == null) PlayerController = GetComponent<PlayerController>();

		if (predictionPoint != null)
			predictionPoint.gameObject.SetActive(false);
	}

	private void OnEnable()
	{
		InputManager.OnGrapplePressed += StartGrapple;
	}

	private void OnDisable()
	{
		InputManager.OnGrapplePressed -= StartGrapple;
		CancelInvoke();
	}

	private void FixedUpdate()
	{
		if (grappleCooldownTimer > 0f)
			grappleCooldownTimer -= Time.deltaTime;

		CheckForGrapplePoints();
	}

	private void CheckForGrapplePoints()
	{
		if (m_bGrappling)
		{
			if (predictionPoint != null)
				predictionPoint.gameObject.SetActive(false);

			return;
		}

		RaycastHit sphereCastHit;
		Physics.SphereCast(
			Cam.position,
			predictionSphereCastRadius,
			Cam.forward,
			out sphereCastHit,
			maxGrappleDistance,
			m_grappableLayer
		);

		RaycastHit raycastHit;
		Physics.Raycast(
			Cam.position,
			Cam.forward,
			out raycastHit,
			maxGrappleDistance,
			m_grappableLayer
		);

		Vector3 realHitPoint;

		// Option 1 - Direct hit
		if (raycastHit.point != Vector3.zero)
			realHitPoint = raycastHit.point;

		// Option 2 - Indirect (predicted) hit
		else if (sphereCastHit.point != Vector3.zero)
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

	private void StartGrapple()
	{
		if (grappleCooldownTimer > 0f) return;
		if (m_bGrappling) return;

		// Use the SAME cached prediction logic as SwingGun
		if (predictionHit.point == Vector3.zero)
		{
			PlayerController.m_bFreeze = false;
			return;
		}

		GetComponent<SwingGun>().StopSwing();

		m_bGrappling = true;
		_grappleToken++;

		CancelInvoke();

		PlayerController.m_bFreeze = true;
		grapplePoint = predictionHit.point;

		if (predictionPoint != null)
			predictionPoint.gameObject.SetActive(false);

		// This now works whether delay is 0 or above 0
		Invoke(nameof(ExecuteGrapple_InvokeWrapper), Mathf.Max(grappleDelayTime, 0f));
	}

	private void ExecuteGrapple_InvokeWrapper() => ExecuteGrapple(_grappleToken);
	private void StopGrapple_InvokeWrapper() => StopGrapple(_grappleToken);

	private void ExecuteGrapple(int token)
	{
		if (token != _grappleToken) return;

		PlayerController.m_bFreeze = false;

		Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);

		float grapplePointRelativeYPos = grapplePoint.y - lowestPoint.y;
		float highestPointOnArc = grapplePointRelativeYPos + overshootYAxis;

		if (grapplePointRelativeYPos < 0f)
			highestPointOnArc = overshootYAxis;

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

		if (predictionPoint != null)
			predictionPoint.gameObject.SetActive(false);
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