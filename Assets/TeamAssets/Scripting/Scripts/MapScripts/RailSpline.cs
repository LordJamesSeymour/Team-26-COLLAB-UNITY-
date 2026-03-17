using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

namespace Group26.Player.Movement
{
    [ExecuteAlways]
    [RequireComponent(typeof(SplineContainer))]
    public class RailSpline : MonoBehaviour
    {
        [Header("Spline")]
        [SerializeField] private SplineContainer splineContainer;

        [Header("Rail Movement")]
        [SerializeField] private float entrySpeed = 8f;
        [SerializeField] private float minSpeed = 4f;
        [SerializeField] private float maxSpeed = 20f;
        [SerializeField] private float acceleration = 16f;
        [SerializeField] private float brakeDeceleration = 22f;
        [SerializeField] private float passiveDeceleration = 3f;

        [Header("Positioning")]
        [SerializeField] private float rideOffset = 1f;
        [SerializeField] private bool useSplineUp = true;

        [Header("Exit")]
        [SerializeField] private bool autoExitAtEnd = true;

        [Header("Attach Accuracy")]
        [SerializeField] private int nearestPointResolution = 8;
        [SerializeField] private int nearestPointIterations = 3;

        [Header("Auto Trigger Generation")]
        [SerializeField] private Vector3 triggerSize = new Vector3(2f, 2f, 2f);
        [SerializeField] private string entryTriggerName = "EntryTrigger";
        [SerializeField] private string exitTriggerName = "ExitTrigger";
        [SerializeField] private bool autoGenerateTriggers = true;

        public SplineContainer SplineContainer => splineContainer;
        public float EntrySpeed => entrySpeed;
        public float MinSpeed => minSpeed;
        public float MaxSpeed => maxSpeed;
        public float Acceleration => acceleration;
        public float BrakeDeceleration => brakeDeceleration;
        public float PassiveDeceleration => passiveDeceleration;
        public float RideOffset => rideOffset;
        public bool UseSplineUp => useSplineUp;
        public bool AutoExitAtEnd => autoExitAtEnd;
        public int NearestPointResolution => nearestPointResolution;
        public int NearestPointIterations => nearestPointIterations;

        private void Reset()
        {
            AutoAssignSplineContainer();
            SyncGeneratedTriggers();
        }

        private void Awake()
        {
            AutoAssignSplineContainer();
            SyncGeneratedTriggers();
        }

        private void OnEnable()
        {
            AutoAssignSplineContainer();
            SyncGeneratedTriggers();
        }

        private void OnValidate()
        {
            AutoAssignSplineContainer();
            ClampSettings();
            SyncGeneratedTriggers();
        }

#if UNITY_EDITOR
        private void LateUpdate()
        {
            if (Application.isPlaying) return;

            AutoAssignSplineContainer();
            SyncGeneratedTriggers();
        }
#endif

        private void ClampSettings()
        {
            minSpeed = Mathf.Max(0f, minSpeed);
            maxSpeed = Mathf.Max(minSpeed, maxSpeed);
            entrySpeed = Mathf.Clamp(entrySpeed, minSpeed, maxSpeed);
            acceleration = Mathf.Max(0f, acceleration);
            brakeDeceleration = Mathf.Max(0f, brakeDeceleration);
            passiveDeceleration = Mathf.Max(0f, passiveDeceleration);
            rideOffset = Mathf.Max(0f, rideOffset);

            nearestPointResolution = Mathf.Max(2, nearestPointResolution);
            nearestPointIterations = Mathf.Max(1, nearestPointIterations);

            triggerSize.x = Mathf.Max(0.01f, triggerSize.x);
            triggerSize.y = Mathf.Max(0.01f, triggerSize.y);
            triggerSize.z = Mathf.Max(0.01f, triggerSize.z);
        }

        private void AutoAssignSplineContainer()
        {
            if (splineContainer == null)
                splineContainer = GetComponent<SplineContainer>();
        }

        private void SyncGeneratedTriggers()
        {
            if (!autoGenerateTriggers) return;
            if (splineContainer == null) return;

            Spline spline = splineContainer.Spline;
            if (spline == null || spline.Count == 0) return;

            Transform entryTransform = GetOrCreateChild(entryTriggerName);
            Transform exitTransform = GetOrCreateChild(exitTriggerName);

            ConfigureTrigger(entryTransform.gameObject, true);
            ConfigureTrigger(exitTransform.gameObject, false);

            UpdateTriggerFromKnot(entryTransform, 0);
            UpdateTriggerFromKnot(exitTransform, spline.Count - 1);
        }

        private Transform GetOrCreateChild(string childName)
        {
            Transform child = transform.Find(childName);
            if (child != null)
                return child;

            GameObject childObject = new GameObject(childName);
            childObject.transform.SetParent(transform, false);
            return childObject.transform;
        }

        private void ConfigureTrigger(GameObject target, bool isEntry)
        {
            target.transform.localScale = Vector3.one;

            BoxCollider box = target.GetComponent<BoxCollider>();
            if (box == null)
                box = target.AddComponent<BoxCollider>();

            box.isTrigger = true;
            box.size = triggerSize;
            box.center = Vector3.zero;

            if (isEntry)
            {
                RailEntryTrigger entry = target.GetComponent<RailEntryTrigger>();
                if (entry == null)
                    entry = target.AddComponent<RailEntryTrigger>();

                RailExitTrigger wrongExit = target.GetComponent<RailExitTrigger>();
                if (wrongExit != null)
                {
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                        DestroyImmediate(wrongExit);
                    else
                        Destroy(wrongExit);
#else
                    Destroy(wrongExit);
#endif
                }

                entry.AutoAssignFromParent();
            }
            else
            {
                RailExitTrigger exit = target.GetComponent<RailExitTrigger>();
                if (exit == null)
                    exit = target.AddComponent<RailExitTrigger>();

                RailEntryTrigger wrongEntry = target.GetComponent<RailEntryTrigger>();
                if (wrongEntry != null)
                {
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                        DestroyImmediate(wrongEntry);
                    else
                        Destroy(wrongEntry);
#else
                    Destroy(wrongEntry);
#endif
                }
            }
        }

        private void UpdateTriggerFromKnot(Transform triggerTransform, int knotIndex)
        {
            if (splineContainer == null) return;

            Spline spline = splineContainer.Spline;
            if (spline == null || spline.Count == 0) return;
            if (knotIndex < 0 || knotIndex >= spline.Count) return;

            BezierKnot knot = spline[knotIndex];

            triggerTransform.SetParent(transform, false);
            triggerTransform.localPosition = (Vector3)knot.Position;
            triggerTransform.localScale = Vector3.one;

            float t = knotIndex == 0 ? 0f : 1f;
            Vector3 tangent = ((Vector3)splineContainer.EvaluateTangent(t)).normalized;
            Vector3 up = useSplineUp
                ? ((Vector3)splineContainer.EvaluateUpVector(t)).normalized
                : Vector3.up;

            if (tangent.sqrMagnitude > 0.0001f)
                triggerTransform.rotation = Quaternion.LookRotation(tangent, up);
        }
    }
}