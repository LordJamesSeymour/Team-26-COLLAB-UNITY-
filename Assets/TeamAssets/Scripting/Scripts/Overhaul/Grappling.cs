using UnityEngine;

public class Grappling : MonoBehaviour
{
	[Header("References")]
	[SerializeField] Transform Cam;
	[SerializeField] Transform gunTip;
	[SerializeField] LayerMask m_lGrappable;
	[SerializeField] LineRenderer lr;
	private PlayerController PlayerController;

	[Header("Grappling")]
	[SerializeField] float maxGrappleDistance;
	[SerializeField] float grappleDelayTime;
	[SerializeField] float overshootYAxis;

	private Vector3 grapplePoint;

	[Header("Cooldown")]
	[SerializeField] float grapplingCd;
	private float grapplingCdTimer;

	private bool m_bGrappling;

	private InputManager2 InputManager;


	private void OnEnable()
	{
		if (InputManager == null) InputManager = GetComponent<InputManager2>();
		if (PlayerController == null) PlayerController = GetComponent<PlayerController>();

		InputManager.OnInteractPressed += StartGrapple;
	}

	private void LateUpdate()
	{
		if (m_bGrappling)
			lr.SetPosition(0, gunTip.position);
	}

	private void FixedUpdate()
	{
		if (grapplingCdTimer > 0)
			grapplingCdTimer -= Time.deltaTime;
	}

	void StartGrapple()
	{
		if (grapplingCdTimer > 0) return;

		m_bGrappling = true;
		Debug.Log(m_bGrappling);

		PlayerController.m_bFreeze = true;

		RaycastHit hit;
		if (Physics.Raycast(Cam.position, Cam.forward, out hit, maxGrappleDistance, m_lGrappable))
		{
			grapplePoint = hit.point;

			Invoke(nameof(ExecuteGrapple), grappleDelayTime);
		}
		else
		{
			grapplePoint = Cam.position + Cam.forward * maxGrappleDistance;
			Invoke(nameof(StopGrapple), grappleDelayTime);
		}

		lr.enabled = true;
		lr.SetPosition(1, grapplePoint);
	}

	void ExecuteGrapple()
	{
		PlayerController.m_bFreeze = false;

		Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);

		float grapplePointRelativeYPos = grapplePoint.y - lowestPoint.y;
		float highestPointOnArc = grapplePointRelativeYPos + overshootYAxis;

		if (grapplePointRelativeYPos < 0) highestPointOnArc = overshootYAxis;

		PlayerController.JumpToPosition(grapplePoint, highestPointOnArc);

		Invoke(nameof(StopGrapple), 1f);
	}

	public void StopGrapple()
	{
		Debug.Log("Stopping Grapple");
		PlayerController.m_bFreeze = false;

		m_bGrappling = false;
		grapplingCdTimer  = grapplingCd;

		lr.enabled = false;
	}
}
