using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingRope : MonoBehaviour
{
	private Spring spring;
	private LineRenderer lr;
	private Vector3 currentGrapplePosition;

	public SwingGun SwingGun;

	public int quality;
	public float damper;
	public float strength;
	public float velocity;
	public float waveCount;
	public float waveHeight;
	public AnimationCurve affectCurve;

	void Awake()
	{
		lr = GetComponent<LineRenderer>();
		spring = new Spring();
		spring.SetTarget(0);

		if (SwingGun != null && SwingGun.gunTip != null)
			currentGrapplePosition = SwingGun.gunTip.position;
	}

	// Called after Update
	void LateUpdate()
	{
		DrawRope();
	}

	void DrawRope()
	{
		if (SwingGun == null || SwingGun.gunTip == null)
			return;

		// If not grappling, don't draw rope
		if (!SwingGun.IsSwinging())
		{
			currentGrapplePosition = SwingGun.gunTip.position;
			spring.Reset();

			if (lr.positionCount > 0)
				lr.positionCount = 0;

			return;
		}

		if (lr.positionCount == 0)
		{
			spring.SetVelocity(velocity);
			lr.positionCount = quality + 1;
		}

		spring.SetDamper(damper);
		spring.SetStrength(strength);
		spring.Update(Time.deltaTime);

		var grapplePoint = SwingGun.GetSwingPoint();
		var gunTipPosition = SwingGun.gunTip.position;
		var up = Quaternion.LookRotation((grapplePoint - gunTipPosition).normalized) * Vector3.up;

		currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, grapplePoint, Time.deltaTime * 12f);

		for (var i = 0; i < quality + 1; i++)
		{
			var delta = i / (float)quality;
			var offset = up * waveHeight * Mathf.Sin(delta * waveCount * Mathf.PI) * spring.Value *
						 affectCurve.Evaluate(delta);

			lr.SetPosition(i, Vector3.Lerp(gunTipPosition, currentGrapplePosition, delta) + offset);
		}
	}
}