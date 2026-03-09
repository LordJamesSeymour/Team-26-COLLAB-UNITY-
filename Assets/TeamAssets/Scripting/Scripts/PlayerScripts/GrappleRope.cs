using Group26.Player.Movement;
using UnityEngine;

namespace Group26.Player.Utility
{
    public class GrappleRope : MonoBehaviour
    {
        private enum RopeSource
        {
            None,
            Swing,
            Grapple
        }

        private Spring spring;
        private LineRenderer lineRenderer;
        private Vector3 currentGrapplePosition;

        [Header("Sources")]
        private SwingGun swingGun;
        private GrappleGun grappleGun;

        [Header("Rope Settings")]
        [SerializeField] private int quality = 20;
        [SerializeField] private float damper = 14f;
        [SerializeField] private float strength = 800f;
        [SerializeField] private float velocity = 15f;
        [SerializeField] private float waveCount = 2f;
        [SerializeField] private float waveHeight = 1f;
        [SerializeField] private AnimationCurve affectCurve;

        private RopeSource lastSource = RopeSource.None;

        private void Awake()
        {
            swingGun = GetComponent<SwingGun>();
            grappleGun = GetComponent<GrappleGun>();

            lineRenderer = GetComponent<LineRenderer>();

            spring = new Spring();
            spring.SetTarget(0);

            Transform tip = GetFallbackGunTip();
            if (tip != null)
                currentGrapplePosition = tip.position;
        }

        private void LateUpdate()
        {
            DrawRope();
        }

        private void DrawRope()
        {
            RopeSource activeSource = GetActiveSource();

            if (activeSource == RopeSource.None)
            {
                Transform fallbackTip = GetFallbackGunTip();
                if (fallbackTip != null)
                    currentGrapplePosition = fallbackTip.position;

                spring.Reset();

                if (lineRenderer.positionCount > 0)
                    lineRenderer.positionCount = 0;

                lastSource = RopeSource.None;
                return;
            }

            Transform gunTip = GetGunTip(activeSource);
            if (gunTip == null)
                return;

            Vector3 targetPoint = GetTargetPoint(activeSource);

            // If we switched from swing -> grapple or grapple -> swing,
            // restart the rope cleanly from the new gun tip.
            if (activeSource != lastSource)
            {
                currentGrapplePosition = gunTip.position;
                spring.Reset();
                lineRenderer.positionCount = 0;
            }

            if (lineRenderer.positionCount == 0)
            {
                spring.SetVelocity(velocity);
                lineRenderer.positionCount = quality + 1;
            }

            spring.SetDamper(damper);
            spring.SetStrength(strength);
            spring.Update(Time.deltaTime);

            Vector3 gunTipPosition = gunTip.position;
            Vector3 ropeDirection = targetPoint - gunTipPosition;

            Vector3 up = ropeDirection.sqrMagnitude > 0.0001f
                ? Quaternion.LookRotation(ropeDirection.normalized) * Vector3.up
                : Vector3.up;

            currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, targetPoint, Time.deltaTime * 12f);

            for (int i = 0; i <= quality; i++)
            {
                float delta = i / (float)quality;

                Vector3 offset =
                    up *
                    waveHeight *
                    Mathf.Sin(delta * waveCount * Mathf.PI) *
                    spring.Value *
                    affectCurve.Evaluate(delta);

                lineRenderer.SetPosition(i, Vector3.Lerp(gunTipPosition, currentGrapplePosition, delta) + offset);
            }

            lastSource = activeSource;
        }

        private RopeSource GetActiveSource()
        {
            if (swingGun != null && swingGun.IsSwinging())
                return RopeSource.Swing;

            if (grappleGun != null && grappleGun.IsRopeActive())
                return RopeSource.Grapple;

            return RopeSource.None;
        }

        private Transform GetGunTip(RopeSource source)
        {
            switch (source)
            {
                case RopeSource.Swing:
                    return swingGun != null ? swingGun.gunTip : null;

                case RopeSource.Grapple:
                    return grappleGun != null ? grappleGun.GetGunTip() : null;

                default:
                    return null;
            }
        }

        private Vector3 GetTargetPoint(RopeSource source)
        {
            switch (source)
            {
                case RopeSource.Swing:
                    return swingGun != null ? swingGun.GetSwingPoint() : Vector3.zero;

                case RopeSource.Grapple:
                    return grappleGun != null ? grappleGun.GetGrapplePoint() : Vector3.zero;

                default:
                    return Vector3.zero;
            }
        }

        private Transform GetFallbackGunTip()
        {
            if (swingGun != null && swingGun.gunTip != null)
                return swingGun.gunTip;

            if (grappleGun != null && grappleGun.GetGunTip() != null)
                return grappleGun.GetGunTip();

            return null;
        }
    }
}