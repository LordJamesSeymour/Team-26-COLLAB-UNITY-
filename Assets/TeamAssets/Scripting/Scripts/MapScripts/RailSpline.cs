using UnityEngine;
using UnityEngine.Splines;

namespace Group26.Player.Movement
{
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
		[SerializeField] private float rideOffset = 1.0f;
		[SerializeField] private bool rotateToRail = true;
		[SerializeField] private bool useSplineUp = true;

		[Header("Exit")]
		[SerializeField] private bool autoExitAtEnd = true;

		[Header("Attach Accuracy")]
		[SerializeField] private int nearestPointResolution = 8;
		[SerializeField] private int nearestPointIterations = 3;

		public SplineContainer SplineContainer => splineContainer;
		public float EntrySpeed => entrySpeed;
		public float MinSpeed => minSpeed;
		public float MaxSpeed => maxSpeed;
		public float Acceleration => acceleration;
		public float BrakeDeceleration => brakeDeceleration;
		public float PassiveDeceleration => passiveDeceleration;
		public float RideOffset => rideOffset;
		public bool RotateToRail => rotateToRail;
		public bool UseSplineUp => useSplineUp;
		public bool AutoExitAtEnd => autoExitAtEnd;
		public int NearestPointResolution => nearestPointResolution;
		public int NearestPointIterations => nearestPointIterations;

		private void Reset()
		{
			splineContainer = GetComponent<SplineContainer>();
		}

		private void Awake()
		{
			if (splineContainer == null)
				splineContainer = GetComponent<SplineContainer>();
		}
	}
}	