using UnityEngine;

public class GrapplingRope : MonoBehaviour
{
	[Header("References")]
	[SerializeField] Transform gunTip;
	[SerializeField] Grappling grapplingGun;
	private LineRenderer lr;


	private void Awake()
	{
		lr = GetComponent<LineRenderer>();
	}

	private void LateUpdate()
	{
		DrawRope();
	}

	private Vector3 currentGrapplePosition;
	void DrawRope()
	{
		// If not grappling, dont draw rope

		if (!grapplingGun) return;

		currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, grapplingGun.GetGrapplePoint(), Time.deltaTime * 12f);

		lr.SetPosition(0, gunTip.position);
		lr.SetPosition(1, currentGrapplePosition);
	}
}
