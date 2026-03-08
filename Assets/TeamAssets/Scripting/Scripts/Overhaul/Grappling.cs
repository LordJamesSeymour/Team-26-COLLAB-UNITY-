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
		if (grappleCooldownTimer > 0)
			grappleCooldownTimer -= Time.deltaTime;
	}

	private void StartGrapple()
	{
		if (grappleCooldownTimer > 0) return;
		if (m_bGrappling) return;

		GetComponent<SwingGun>().StopSwing();

		m_bGrappling = true;
		_grappleToken++;

		CancelInvoke();

		PlayerController.m_bFreeze = true;

		bool didHit = Physics.Raycast(Cam.position, Cam.forward, out RaycastHit hit, maxGrappleDistance, m_grappableLayer);
		grapplePoint = didHit ? hit.point : (Cam.position + Cam.forward * maxGrappleDistance);

		if (grappleDelayTime <= 0)
		{
			if (didHit)
			{
				Invoke(nameof(ExecuteGrapple_InvokeWrapper), grappleDelayTime);
			}
			else
			{
				PlayerController.m_bFreeze = false;

				float t = Mathf.Max(missLineTime, 0.01f);
				Invoke(nameof(StopGrapple_InvokeWrapper), t);
			}
		}
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

		if (grapplePointRelativeYPos < 0)
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