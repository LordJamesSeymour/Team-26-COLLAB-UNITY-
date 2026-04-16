using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace Group26.Player.Movement
{
	[ExecuteAlways]
	[RequireComponent(typeof(SplineContainer))]
	public class RailSpline : MonoBehaviour
	{
		[Header("Source Spline")]
		[SerializeField] private SplineContainer sourceSplineContainer;

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

		[Header("Smoothing")]
		[SerializeField] private bool generateSmoothedSpline = true;
		[SerializeField] private float sourceSamplesPerUnit = 2.5f;
		[SerializeField] private int chaikinIterations = 2;
		[SerializeField, Range(0f, 1f)] private float autoSmoothTension = 0.65f;
		[SerializeField] private int minGeneratedKnots = 8;
		[SerializeField] private int maxGeneratedKnots = 96;

		[Header("Top / Bottom Rail Meshes")]
		[SerializeField] private bool generateRailMeshes = true;
		[SerializeField] private Material railMaterial;
		[SerializeField] private float railHalfSeparation = 0.45f;
		[SerializeField] private float railVerticalOffset = 0.12f;
		[SerializeField] private float railRadius = 0.06f;
		[SerializeField] private int railSides = 10;
		[SerializeField] private float meshSegmentsPerUnit = 3f;
		[SerializeField] private bool generateRailColliders = false;

		[Header("Wall Height")]
		[Range(0f, 100f)]
		[SerializeField] private float wallHeight = 50f;

		[Header("Side Wall Meshes")]
		[SerializeField] private bool generateSideWallMeshes = true;
		[SerializeField] private Material sideWallMaterial;
		[SerializeField] private bool generateSideWallColliders = false;
		[SerializeField] private bool sideWallsDoubleSided = true;

		[Header("Bridge Plane")]
		[SerializeField] private bool generateBridgePlane = true;
		[SerializeField] private Material bridgeMaterial;
		[SerializeField] private float bridgeVerticalOffset = 0f;
		[SerializeField] private bool bridgeDoubleSided = true;

		[Header("UV Tiling")]
		[SerializeField] private float tileEveryUnits = 50f;
		[SerializeField] private bool flipWallUVVertical = false;

		[Header("Auto Trigger Generation")]
		[SerializeField] private Vector3 triggerSize = new Vector3(2f, 2f, 2f);
		[SerializeField] private string generatedPathName = "GeneratedPath_Auto";
		[SerializeField] private string leftRailName = "LeftRailMesh_Auto";
		[SerializeField] private string rightRailName = "RightRailMesh_Auto";
		[SerializeField] private string leftBottomRailName = "LeftBottomRailMesh_Auto";
		[SerializeField] private string rightBottomRailName = "RightBottomRailMesh_Auto";
		[SerializeField] private string leftWallName = "LeftWallMesh_Auto";
		[SerializeField] private string rightWallName = "RightWallMesh_Auto";
		[SerializeField] private string bridgePlaneName = "BridgePlane_Auto";
		[SerializeField] private string entryTriggerName = "EntryTrigger";
		[SerializeField] private string exitTriggerName = "ExitTrigger";
		[SerializeField] private bool autoGenerateTriggers = true;

		private SplineContainer generatedSplineContainer;
		private Transform leftRailTransform;
		private Transform rightRailTransform;
		private Transform leftBottomRailTransform;
		private Transform rightBottomRailTransform;
		private Transform leftWallTransform;
		private Transform rightWallTransform;
		private Transform bridgePlaneTransform;

		public SplineContainer SplineContainer => generatedSplineContainer != null ? generatedSplineContainer : sourceSplineContainer;
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
			AutoAssignSourceSpline();
			RebuildAll();
		}

		private void Awake()
		{
			AutoAssignSourceSpline();
			RebuildAll();
		}

		private void OnEnable()
		{
			AutoAssignSourceSpline();
			RebuildAll();
		}

		private void OnValidate()
		{
			AutoAssignSourceSpline();
			ClampSettings();
			RebuildAll();
		}

#if UNITY_EDITOR
		private void LateUpdate()
		{
			if (Application.isPlaying)
				return;

			AutoAssignSourceSpline();
			RebuildAll();
		}
#endif

		private void AutoAssignSourceSpline()
		{
			if (sourceSplineContainer == null)
				sourceSplineContainer = GetComponent<SplineContainer>();
		}

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

			sourceSamplesPerUnit = Mathf.Max(0.25f, sourceSamplesPerUnit);
			chaikinIterations = Mathf.Max(0, chaikinIterations);
			minGeneratedKnots = Mathf.Max(4, minGeneratedKnots);
			maxGeneratedKnots = Mathf.Max(minGeneratedKnots, maxGeneratedKnots);

			railHalfSeparation = Mathf.Max(0.01f, railHalfSeparation);
			railVerticalOffset = Mathf.Max(0f, railVerticalOffset);
			railRadius = Mathf.Max(0.01f, railRadius);
			railSides = Mathf.Max(3, railSides);
			meshSegmentsPerUnit = Mathf.Max(0.5f, meshSegmentsPerUnit);

			wallHeight = Mathf.Max(0f, wallHeight);
			tileEveryUnits = Mathf.Max(0.01f, tileEveryUnits);

			triggerSize.x = Mathf.Max(0.01f, triggerSize.x);
			triggerSize.y = Mathf.Max(0.01f, triggerSize.y);
			triggerSize.z = Mathf.Max(0.01f, triggerSize.z);
		}

		private void RebuildAll()
		{
			if (sourceSplineContainer == null)
				return;

			EnsureGeneratedObjects();
			BuildRuntimeSpline();
			SyncGeneratedTriggers();
			BuildGeneratedGeometry();
		}

		private void EnsureGeneratedObjects()
		{
			generatedSplineContainer = GetOrCreateSplineChild(generatedPathName);
			leftRailTransform = GetOrCreateChild(leftRailName);
			rightRailTransform = GetOrCreateChild(rightRailName);
			leftBottomRailTransform = GetOrCreateChild(leftBottomRailName);
			rightBottomRailTransform = GetOrCreateChild(rightBottomRailName);
			leftWallTransform = GetOrCreateChild(leftWallName);
			rightWallTransform = GetOrCreateChild(rightWallName);
			bridgePlaneTransform = GetOrCreateChild(bridgePlaneName);

			generatedSplineContainer.transform.SetParent(transform, false);
			generatedSplineContainer.transform.localPosition = Vector3.zero;
			generatedSplineContainer.transform.localRotation = Quaternion.identity;
			generatedSplineContainer.transform.localScale = Vector3.one;

			ResetChildTransform(leftRailTransform);
			ResetChildTransform(rightRailTransform);
			ResetChildTransform(leftBottomRailTransform);
			ResetChildTransform(rightBottomRailTransform);
			ResetChildTransform(leftWallTransform);
			ResetChildTransform(rightWallTransform);
			ResetChildTransform(bridgePlaneTransform);
		}

		private void ResetChildTransform(Transform t)
		{
			if (t == null) return;

			t.SetParent(transform, false);
			t.localPosition = Vector3.zero;
			t.localRotation = Quaternion.identity;
			t.localScale = Vector3.one;
		}

		private SplineContainer GetOrCreateSplineChild(string childName)
		{
			Transform child = transform.Find(childName);
			if (child == null)
			{
				GameObject go = new GameObject(childName);
				go.transform.SetParent(transform, false);
				return go.AddComponent<SplineContainer>();
			}

			SplineContainer container = child.GetComponent<SplineContainer>();
			if (container == null)
				container = child.gameObject.AddComponent<SplineContainer>();

			return container;
		}

		private Transform GetOrCreateChild(string childName)
		{
			Transform child = transform.Find(childName);
			if (child != null)
				return child;

			GameObject go = new GameObject(childName);
			go.transform.SetParent(transform, false);
			return go.transform;
		}

		private void BuildRuntimeSpline()
		{
			if (sourceSplineContainer == null || generatedSplineContainer == null)
				return;

			Spline sourceSpline = sourceSplineContainer.Spline;
			if (sourceSpline == null || sourceSpline.Count < 2)
				return;

			List<Vector3> localSamplePoints = SampleSourceSplineToLocalPoints();
			if (localSamplePoints.Count < 2)
				return;

			if (generateSmoothedSpline)
			{
				for (int i = 0; i < chaikinIterations; i++)
					localSamplePoints = ApplyChaikinOpen(localSamplePoints);

				int targetKnotCount = EstimateGeneratedKnotCount();
				localSamplePoints = ResamplePolyline(localSamplePoints, targetKnotCount);
			}

			RebuildGeneratedSplineFromLocalPoints(localSamplePoints);
		}

		private List<Vector3> SampleSourceSplineToLocalPoints()
		{
			float worldLength = Mathf.Max(0.01f, sourceSplineContainer.CalculateLength());
			int rawSampleCount = Mathf.Max(2, Mathf.CeilToInt(worldLength * sourceSamplesPerUnit) + 1);

			List<Vector3> points = new List<Vector3>(rawSampleCount);

			for (int i = 0; i < rawSampleCount; i++)
			{
				float t = rawSampleCount == 1 ? 0f : i / (float)(rawSampleCount - 1);
				Vector3 worldPos = (Vector3)sourceSplineContainer.EvaluatePosition(t);
				Vector3 localPos = transform.InverseTransformPoint(worldPos);
				points.Add(localPos);
			}

			return points;
		}

		private int EstimateGeneratedKnotCount()
		{
			float worldLength = Mathf.Max(0.01f, sourceSplineContainer.CalculateLength());
			int estimated = Mathf.CeilToInt(worldLength * 1.25f) + 2;
			return Mathf.Clamp(estimated, minGeneratedKnots, maxGeneratedKnots);
		}

		private void RebuildGeneratedSplineFromLocalPoints(List<Vector3> localPoints)
		{
			if (generatedSplineContainer == null || localPoints == null || localPoints.Count < 2)
				return;

			Spline targetSpline = generatedSplineContainer.Spline;
			targetSpline.Clear();
			targetSpline.Closed = false;

			for (int i = 0; i < localPoints.Count; i++)
			{
				float3 current = localPoints[i];

				float3 previous = i == 0
					? (float3)(localPoints[0] - (localPoints[1] - localPoints[0]))
					: (float3)localPoints[i - 1];

				float3 next = i == localPoints.Count - 1
					? (float3)(localPoints[i] + (localPoints[i] - localPoints[i - 1]))
					: (float3)localPoints[i + 1];

				BezierKnot knot = SplineUtility.GetAutoSmoothKnot(
					current,
					previous,
					next,
					math.up(),
					autoSmoothTension);

				targetSpline.Add(knot);
			}
		}

		private void SyncGeneratedTriggers()
		{
			if (!autoGenerateTriggers)
				return;

			SplineContainer runtimeSpline = SplineContainer;
			if (runtimeSpline == null || runtimeSpline.Spline == null || runtimeSpline.Spline.Count < 2)
				return;

			Transform entryTransform = GetOrCreateChild(entryTriggerName);
			Transform exitTransform = GetOrCreateChild(exitTriggerName);

			ConfigureTrigger(entryTransform.gameObject, true);
			ConfigureTrigger(exitTransform.gameObject, false);

			UpdateTriggerFromSpline(entryTransform, 0f);
			UpdateTriggerFromSpline(exitTransform, 1f);
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

		private void UpdateTriggerFromSpline(Transform triggerTransform, float t)
		{
			SplineContainer runtimeSpline = SplineContainer;
			if (runtimeSpline == null)
				return;

			Vector3 worldPos = (Vector3)runtimeSpline.EvaluatePosition(t);
			Vector3 tangent = ((Vector3)runtimeSpline.EvaluateTangent(t)).normalized;
			Vector3 up = useSplineUp
				? ((Vector3)runtimeSpline.EvaluateUpVector(t)).normalized
				: Vector3.up;

			triggerTransform.position = worldPos;

			if (tangent.sqrMagnitude > 0.0001f)
				triggerTransform.rotation = Quaternion.LookRotation(tangent, up);
		}

		private void BuildGeneratedGeometry()
		{
			SplineContainer runtimeSpline = SplineContainer;
			if (runtimeSpline == null || runtimeSpline.Spline == null || runtimeSpline.Spline.Count < 2)
				return;

			int ringCount = Mathf.Max(4, Mathf.CeilToInt(Mathf.Max(0.01f, runtimeSpline.CalculateLength()) * meshSegmentsPerUnit) + 1);

			Vector3[] leftUpper = new Vector3[ringCount];
			Vector3[] rightUpper = new Vector3[ringCount];
			Vector3[] leftLower = new Vector3[ringCount];
			Vector3[] rightLower = new Vector3[ringCount];
			Vector3[] railForwards = new Vector3[ringCount];
			Vector3[] railUps = new Vector3[ringCount];

			for (int i = 0; i < ringCount; i++)
			{
				float t = ringCount == 1 ? 0f : i / (float)(ringCount - 1);

				Vector3 worldPos = (Vector3)runtimeSpline.EvaluatePosition(t);
				Vector3 worldForward = ((Vector3)runtimeSpline.EvaluateTangent(t)).normalized;
				Vector3 worldUp = useSplineUp
					? ((Vector3)runtimeSpline.EvaluateUpVector(t)).normalized
					: Vector3.up;

				if (worldForward.sqrMagnitude < 0.0001f)
					worldForward = transform.forward;

				if (worldUp.sqrMagnitude < 0.0001f)
					worldUp = Vector3.up;

				Vector3 worldRight = Vector3.Cross(worldUp, worldForward).normalized;
				if (worldRight.sqrMagnitude < 0.0001f)
					worldRight = transform.right;

				Vector3 topLeftWorld = worldPos + worldUp * railVerticalOffset - worldRight * railHalfSeparation;
				Vector3 topRightWorld = worldPos + worldUp * railVerticalOffset + worldRight * railHalfSeparation;

				Vector3 bottomOffset = Vector3.down * wallHeight;
				Vector3 bottomLeftWorld = topLeftWorld + bottomOffset;
				Vector3 bottomRightWorld = topRightWorld + bottomOffset;

				leftUpper[i] = transform.InverseTransformPoint(topLeftWorld);
				rightUpper[i] = transform.InverseTransformPoint(topRightWorld);
				leftLower[i] = transform.InverseTransformPoint(bottomLeftWorld);
				rightLower[i] = transform.InverseTransformPoint(bottomRightWorld);

				railForwards[i] = transform.InverseTransformDirection(worldForward).normalized;
				railUps[i] = transform.InverseTransformDirection(worldUp).normalized;
			}

			if (generateRailMeshes)
			{
				BuildRailTubeObject(leftRailTransform, leftUpper, railForwards, railUps, "LeftRailMesh");
				BuildRailTubeObject(rightRailTransform, rightUpper, railForwards, railUps, "RightRailMesh");
				BuildRailTubeObject(leftBottomRailTransform, leftLower, railForwards, railUps, "LeftBottomRailMesh");
				BuildRailTubeObject(rightBottomRailTransform, rightLower, railForwards, railUps, "RightBottomRailMesh");
			}
			else
			{
				ClearGeneratedMesh(leftRailTransform, true);
				ClearGeneratedMesh(rightRailTransform, true);
				ClearGeneratedMesh(leftBottomRailTransform, true);
				ClearGeneratedMesh(rightBottomRailTransform, true);
			}

			if (generateSideWallMeshes)
			{
				BuildRibbonObject(
					leftWallTransform,
					leftLower,
					leftUpper,
					"LeftWallMesh",
					sideWallMaterial,
					sideWallsDoubleSided,
					generateSideWallColliders,
					true,
					false,
					flipWallUVVertical,
					false);

				BuildRibbonObject(
					rightWallTransform,
					rightUpper,
					rightLower,
					"RightWallMesh",
					sideWallMaterial,
					sideWallsDoubleSided,
					generateSideWallColliders,
					true,
					false,
					flipWallUVVertical,
					true);
			}
			else
			{
				ClearGeneratedMesh(leftWallTransform, true);
				ClearGeneratedMesh(rightWallTransform, true);
			}

			if (generateBridgePlane)
			{
				Vector3[] bridgeLeft = new Vector3[ringCount];
				Vector3[] bridgeRight = new Vector3[ringCount];

				for (int i = 0; i < ringCount; i++)
				{
					Vector3 offset = railUps[i] * bridgeVerticalOffset;
					bridgeLeft[i] = leftUpper[i] + offset;
					bridgeRight[i] = rightUpper[i] + offset;
				}

				BuildRibbonObject(
					bridgePlaneTransform,
					bridgeLeft,
					bridgeRight,
					"BridgePlane",
					bridgeMaterial,
					bridgeDoubleSided,
					false,
					false,
					false,
					false,
					false);
			}
			else
			{
				ClearGeneratedMesh(bridgePlaneTransform, true);
			}
		}

		private void BuildRailTubeObject(
			Transform target,
			Vector3[] centers,
			Vector3[] forwards,
			Vector3[] ups,
			string meshName)
		{
			if (target == null)
				return;

			MeshFilter meshFilter = target.GetComponent<MeshFilter>();
			if (meshFilter == null)
				meshFilter = target.gameObject.AddComponent<MeshFilter>();

			MeshRenderer meshRenderer = target.GetComponent<MeshRenderer>();
			if (meshRenderer == null)
				meshRenderer = target.gameObject.AddComponent<MeshRenderer>();

			if (railMaterial != null)
				meshRenderer.sharedMaterial = railMaterial;

			Mesh mesh = meshFilter.sharedMesh;
			if (mesh == null)
			{
				mesh = new Mesh { name = meshName };
				meshFilter.sharedMesh = mesh;
			}
			else
			{
				mesh.Clear();
			}

			RailMeshUtility.BuildTubeMesh(
				mesh,
				centers,
				forwards,
				ups,
				railRadius,
				railSides,
				tileEveryUnits);

			if (generateRailColliders)
			{
				MeshCollider collider = target.GetComponent<MeshCollider>();
				if (collider == null)
					collider = target.gameObject.AddComponent<MeshCollider>();

				collider.sharedMesh = null;
				collider.sharedMesh = mesh;
				collider.convex = false;
			}
			else
			{
				RemoveMeshCollider(target.gameObject);
			}
		}

		private void BuildRibbonObject(
			Transform target,
			Vector3[] edgeA,
			Vector3[] edgeB,
			string meshName,
			Material material,
			bool doubleSided,
			bool addCollider,
			bool rotateUVs90,
			bool flipU,
			bool flipV,
			bool reverseAcrossUVDirection)
		{
			if (target == null)
				return;

			MeshFilter meshFilter = target.GetComponent<MeshFilter>();
			if (meshFilter == null)
				meshFilter = target.gameObject.AddComponent<MeshFilter>();

			MeshRenderer meshRenderer = target.GetComponent<MeshRenderer>();
			if (meshRenderer == null)
				meshRenderer = target.gameObject.AddComponent<MeshRenderer>();

			if (material != null)
				meshRenderer.sharedMaterial = material;

			Mesh mesh = meshFilter.sharedMesh;
			if (mesh == null)
			{
				mesh = new Mesh { name = meshName };
				meshFilter.sharedMesh = mesh;
			}
			else
			{
				mesh.Clear();
			}

			RailMeshUtility.BuildRibbonMesh(
				mesh,
				edgeA,
				edgeB,
				doubleSided,
				rotateUVs90,
				flipU,
				flipV,
				tileEveryUnits,
				tileEveryUnits,
				reverseAcrossUVDirection);

			if (addCollider)
			{
				MeshCollider collider = target.GetComponent<MeshCollider>();
				if (collider == null)
					collider = target.gameObject.AddComponent<MeshCollider>();

				collider.sharedMesh = null;
				collider.sharedMesh = mesh;
				collider.convex = false;
			}
			else
			{
				RemoveMeshCollider(target.gameObject);
			}
		}

		private void ClearGeneratedMesh(Transform target, bool removeCollider)
		{
			if (target == null)
				return;

			MeshFilter meshFilter = target.GetComponent<MeshFilter>();
			if (meshFilter != null && meshFilter.sharedMesh != null)
				meshFilter.sharedMesh.Clear();

			if (removeCollider)
				RemoveMeshCollider(target.gameObject);
		}

		private void RemoveMeshCollider(GameObject go)
		{
			MeshCollider collider = go.GetComponent<MeshCollider>();
			if (collider == null)
				return;

#if UNITY_EDITOR
			if (!Application.isPlaying)
				DestroyImmediate(collider);
			else
				Destroy(collider);
#else
            Destroy(collider);
#endif
		}

		private static List<Vector3> ApplyChaikinOpen(List<Vector3> input)
		{
			if (input == null || input.Count < 3)
				return input;

			List<Vector3> result = new List<Vector3>(input.Count * 2);
			result.Add(input[0]);

			for (int i = 0; i < input.Count - 1; i++)
			{
				Vector3 p0 = input[i];
				Vector3 p1 = input[i + 1];

				Vector3 q = Vector3.Lerp(p0, p1, 0.25f);
				Vector3 r = Vector3.Lerp(p0, p1, 0.75f);

				result.Add(q);
				result.Add(r);
			}

			result.Add(input[input.Count - 1]);
			return result;
		}

		private static List<Vector3> ResamplePolyline(List<Vector3> points, int targetCount)
		{
			List<Vector3> result = new List<Vector3>(targetCount);

			if (points == null || points.Count == 0)
				return result;

			if (points.Count == 1 || targetCount <= 1)
			{
				result.Add(points[0]);
				return result;
			}

			float[] cumulative = new float[points.Count];
			cumulative[0] = 0f;

			for (int i = 1; i < points.Count; i++)
				cumulative[i] = cumulative[i - 1] + Vector3.Distance(points[i - 1], points[i]);

			float totalLength = cumulative[cumulative.Length - 1];
			if (totalLength <= 0.0001f)
			{
				for (int i = 0; i < targetCount; i++)
					result.Add(points[0]);

				return result;
			}

			for (int i = 0; i < targetCount; i++)
			{
				float targetDistance = totalLength * (i / (float)(targetCount - 1));
				result.Add(GetPointAtDistance(points, cumulative, targetDistance));
			}

			return result;
		}

		private static Vector3 GetPointAtDistance(List<Vector3> points, float[] cumulative, float distance)
		{
			if (distance <= 0f)
				return points[0];

			if (distance >= cumulative[cumulative.Length - 1])
				return points[points.Count - 1];

			for (int i = 1; i < cumulative.Length; i++)
			{
				if (distance <= cumulative[i])
				{
					float segmentStart = cumulative[i - 1];
					float segmentEnd = cumulative[i];
					float range = Mathf.Max(0.0001f, segmentEnd - segmentStart);
					float t = (distance - segmentStart) / range;
					return Vector3.Lerp(points[i - 1], points[i], t);
				}
			}

			return points[points.Count - 1];
		}
	}
}