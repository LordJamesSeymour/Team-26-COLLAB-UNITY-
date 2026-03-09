// using Group26.Player.Movement;
// using UnityEngine;

// public class GrappleRope : MonoBehaviour
// {
// 	private enum RopeSource
// 	{
// 		None,
// 		Swing,
// 		Grapple
// 	}

// 	private Spring spring;
// 	private LineRenderer lr;
// 	private Vector3 currentGrapplePosition;

// 	[Header("Sources")]
// 	public SwingGun SwingGun;
// 	public GrappleGun Grappling;

// 	[Header("Rope Settings")]
// 	public int quality = 20;
// 	public float damper = 14f;
// 	public float strength = 800f;
// 	public float velocity = 15f;
// 	public float waveCount = 2f;
// 	public float waveHeight = 1f;
// 	public AnimationCurve affectCurve;

// 	private RopeSource lastSource = RopeSource.None;

// 	private void Awake()
// 	{
// 		lr = GetComponent<LineRenderer>();

// 		spring = new Spring();
// 		spring.SetTarget(0);

// 		Transform tip = GetFallbackGunTip();
// 		if (tip != null)
// 			currentGrapplePosition = tip.position;
// 	}

// 	private void LateUpdate()
// 	{
// 		DrawRope();
// 	}

// 	private void DrawRope()
// 	{
// 		RopeSource activeSource = GetActiveSource();

// 		if (activeSource == RopeSource.None)
// 		{
// 			Transform fallbackTip = GetFallbackGunTip();
// 			if (fallbackTip != null)
// 				currentGrapplePosition = fallbackTip.position;

// 			spring.Reset();

// 			if (lr.positionCount > 0)
// 				lr.positionCount = 0;

// 			lastSource = RopeSource.None;
// 			return;
// 		}

// 		Transform gunTip = GetGunTip(activeSource);
// 		if (gunTip == null)
// 			return;

// 		Vector3 targetPoint = GetTargetPoint(activeSource);

// 		// If we switched from swing -> grapple or grapple -> swing,
// 		// restart the rope cleanly from the new gun tip.
// 		if (activeSource != lastSource)
// 		{
// 			currentGrapplePosition = gunTip.position;
// 			spring.Reset();
// 			lr.positionCount = 0;
// 		}

// 		if (lr.positionCount == 0)
// 		{
// 			spring.SetVelocity(velocity);
// 			lr.positionCount = quality + 1;
// 		}

// 		spring.SetDamper(damper);
// 		spring.SetStrength(strength);
// 		spring.Update(Time.deltaTime);

// 		Vector3 gunTipPosition = gunTip.position;
// 		Vector3 ropeDirection = targetPoint - gunTipPosition;

// 		Vector3 up = ropeDirection.sqrMagnitude > 0.0001f
// 			? Quaternion.LookRotation(ropeDirection.normalized) * Vector3.up
// 			: Vector3.up;

// 		currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, targetPoint, Time.deltaTime * 12f);

// 		for (int i = 0; i <= quality; i++)
// 		{
// 			float delta = i / (float)quality;

// 			Vector3 offset =
// 				up *
// 				waveHeight *
// 				Mathf.Sin(delta * waveCount * Mathf.PI) *
// 				spring.Value *
// 				affectCurve.Evaluate(delta);

// 			lr.SetPosition(i, Vector3.Lerp(gunTipPosition, currentGrapplePosition, delta) + offset);
// 		}

// 		lastSource = activeSource;
// 	}

// 	private RopeSource GetActiveSource()
// 	{
// 		if (SwingGun != null && SwingGun.IsSwinging())
// 			return RopeSource.Swing;

// 		if (Grappling != null && Grappling.IsRopeActive())
// 			return RopeSource.Grapple;

// 		return RopeSource.None;
// 	}

// 	private Transform GetGunTip(RopeSource source)
// 	{
// 		switch (source)
// 		{
// 			case RopeSource.Swing:
// 				return SwingGun != null ? SwingGun.gunTip : null;

// 			case RopeSource.Grapple:
// 				return Grappling != null ? Grappling.GetGunTip() : null;

// 			default:
// 				return null;
// 		}
// 	}

// 	private Vector3 GetTargetPoint(RopeSource source)
// 	{
// 		switch (source)
// 		{
// 			case RopeSource.Swing:
// 				return SwingGun != null ? SwingGun.GetSwingPoint() : Vector3.zero;

// 			case RopeSource.Grapple:
// 				return Grappling != null ? Grappling.GetGrapplePoint() : Vector3.zero;

// 			default:
// 				return Vector3.zero;
// 		}
// 	}

// 	private Transform GetFallbackGunTip()
// 	{
// 		if (SwingGun != null && SwingGun.gunTip != null)
// 			return SwingGun.gunTip;

// 		if (Grappling != null && Grappling.GetGunTip() != null)
// 			return Grappling.GetGunTip();

// 		return null;
// 	}
// }