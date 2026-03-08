using UnityEngine;

public class GrapplingRope : MonoBehaviour
{
	[Header("References")]
	private Spring spring;
	private LineRenderer lr;
	private Vector3 currentGrapplePosition;
	[SerializeField] SwingGun SwingGun;
	[SerializeField] int m_iRopeResolution;
	[SerializeField] int m_fDamper;
	[SerializeField] int m_fStrength;
	[SerializeField] int m_fVelocity;
	[SerializeField] int m_fWaveCount;
	[SerializeField] int m_fWaveHeight;
	[SerializeField] AnimationCurve a_cAffectCurve;
	



	private void Awake()
	{
		lr = GetComponent<LineRenderer>();
		spring = new Spring();
		spring.SetTarget(0);
	}

	private void LateUpdate()
	{
		DrawRope();
	}


	private void DrawRope()
	{
		if (!SwingGun.joint)
		{
			currentGrapplePosition = SwingGun.gunTip.position;
			spring.Reset();

			if(lr.positionCount > 0)
				lr.positionCount = 0;
			return;
		}

		if (lr.positionCount == 0)
		{
			spring.SetVelocity(m_fVelocity);
			lr.positionCount = m_iRopeResolution + 1;
		}


		spring.SetDamper(m_fDamper);
		spring.SetStrength(m_fStrength);
		spring.Update(Time.deltaTime);

		var swingPoint = SwingGun.GetSwingPoint();
		var gunTipPos = SwingGun.gunTip.position;
		var up = Quaternion.LookRotation(swingPoint - gunTipPos).normalized * Vector3.up;

		currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, SwingGun.GetSwingPoint(), Time.deltaTime * 8f);

		for (int i = 0; i < m_iRopeResolution; i++) 
		{
			var delta = i / (float)m_iRopeResolution;
			var offset = up * m_fWaveHeight * Mathf.Sin(delta * m_fWaveCount * Mathf.PI) * spring.Value * a_cAffectCurve.Evaluate(delta);

			lr.SetPosition(i, Vector3.Lerp(gunTipPos, currentGrapplePosition, delta) + offset);
		}
	}
}