using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Group26.Player.Utility
{
	[DisallowMultipleComponent]
	public class GrappleHighlightTarget : MonoBehaviour
	{
		[Header("Outline Setup")]
		[SerializeField] private Material m_outlineMaterial;
		[SerializeField] private bool m_includeInactiveChildren = false;
		[SerializeField] private bool m_buildOnAwake = true;
		[SerializeField] private bool m_startHighlighted = false;

		[Header("Outline Mesh Fix")]
		[SerializeField] private bool m_useSmoothedNormalsForOutline = true;

		[Header("Shader Property Names")]
		[SerializeField] private string m_outlineColorPropertyName = "_OutlineColor";

		private const string OutlineSuffix = "__GrappleOutline";

		private readonly List<Renderer> m_outlineRenderers = new List<Renderer>();
		private readonly Dictionary<Mesh, Mesh> m_outlineMeshCache = new Dictionary<Mesh, Mesh>();
		private readonly List<Mesh> m_generatedOutlineMeshes = new List<Mesh>();

		private MaterialPropertyBlock m_propertyBlock;
		private int m_outlineColorPropertyId = -1;
		private string m_resolvedOutlineColorPropertyName = string.Empty;
		private Color m_currentHighlightColor = Color.white;

		public Material OutlineMaterial => m_outlineMaterial;

		private void Awake()
		{
			m_propertyBlock = new MaterialPropertyBlock();
			ResolveOutlineColorProperty();

			if (m_buildOnAwake)
				RebuildOutlineObjects();

			SetHighlighted(m_startHighlighted);
		}

		private void OnValidate()
		{
			ResolveOutlineColorProperty();
		}

		private void OnDisable()
		{
			SetHighlighted(false);
		}

		private void OnDestroy()
		{
			DestroyGeneratedOutlineMeshes();
		}

		[ContextMenu("Rebuild Outline Objects")]
		public void RebuildOutlineObjects()
		{
			DestroyExistingOutlineObjects();
			DestroyGeneratedOutlineMeshes();

			m_outlineRenderers.Clear();
			m_outlineMeshCache.Clear();

			if (m_outlineMaterial == null)
			{
				Debug.LogWarning($"[{nameof(GrappleHighlightTarget)}] No outline material assigned on {name}.");
				return;
			}

			ResolveOutlineColorProperty();

			BuildStaticMeshOutlines();
			BuildSkinnedMeshOutlines();
			ApplyCurrentColorToRenderers();
			SetHighlighted(false);
		}

		public void SetHighlighted(bool highlighted)
		{
			EnsureBuilt();

			for (int i = 0; i < m_outlineRenderers.Count; i++)
			{
				if (m_outlineRenderers[i] != null)
					m_outlineRenderers[i].enabled = highlighted;
			}

			if (highlighted)
				ApplyCurrentColorToRenderers();
		}

		public void SetHighlightColor(Color color)
		{
			m_currentHighlightColor = color;
			ApplyCurrentColorToRenderers();
		}

		private void EnsureBuilt()
		{
			if (m_outlineRenderers.Count == 0 && m_outlineMaterial != null)
				RebuildOutlineObjects();
		}

		private void ResolveOutlineColorProperty()
		{
			m_outlineColorPropertyId = -1;
			m_resolvedOutlineColorPropertyName = string.Empty;

			if (m_outlineMaterial == null)
				return;

			List<string> candidates = new List<string>();

			if (!string.IsNullOrWhiteSpace(m_outlineColorPropertyName))
			{
				candidates.Add(m_outlineColorPropertyName);

				if (!m_outlineColorPropertyName.StartsWith("_"))
					candidates.Add("_" + m_outlineColorPropertyName);
				else
					candidates.Add(m_outlineColorPropertyName.TrimStart('_'));
			}

			candidates.Add("_OutlineColor");
			candidates.Add("_OutlineColour");
			candidates.Add("OutlineColor");
			candidates.Add("OutlineColour");

			for (int i = 0; i < candidates.Count; i++)
			{
				string candidate = candidates[i];
				if (string.IsNullOrWhiteSpace(candidate))
					continue;

				if (m_outlineMaterial.HasProperty(candidate))
				{
					m_resolvedOutlineColorPropertyName = candidate;
					m_outlineColorPropertyId = Shader.PropertyToID(candidate);
					return;
				}
			}

			Debug.LogWarning(
				$"[{nameof(GrappleHighlightTarget)}] Could not find a valid outline color property on material '{m_outlineMaterial.name}' " +
				$"for object '{name}'. Tried: {string.Join(", ", candidates)}");
		}

		private void ApplyCurrentColorToRenderers()
		{
			if (m_outlineColorPropertyId == -1)
				return;

			if (m_propertyBlock == null)
				m_propertyBlock = new MaterialPropertyBlock();

			for (int i = 0; i < m_outlineRenderers.Count; i++)
			{
				Renderer renderer = m_outlineRenderers[i];
				if (renderer == null) continue;

				m_propertyBlock.Clear();
				m_propertyBlock.SetColor(m_outlineColorPropertyId, m_currentHighlightColor);
				renderer.SetPropertyBlock(m_propertyBlock);
			}
		}

		private void BuildStaticMeshOutlines()
		{
			MeshRenderer[] sourceRenderers = GetComponentsInChildren<MeshRenderer>(m_includeInactiveChildren);

			foreach (MeshRenderer sourceRenderer in sourceRenderers)
			{
				if (sourceRenderer == null) continue;
				if (sourceRenderer.gameObject.name.EndsWith(OutlineSuffix)) continue;

				MeshFilter sourceFilter = sourceRenderer.GetComponent<MeshFilter>();
				if (sourceFilter == null || sourceFilter.sharedMesh == null) continue;

				GameObject outlineObject = new GameObject(sourceRenderer.gameObject.name + OutlineSuffix);
				outlineObject.transform.SetParent(sourceRenderer.transform, false);
				outlineObject.layer = sourceRenderer.gameObject.layer;

				MeshFilter outlineFilter = outlineObject.AddComponent<MeshFilter>();
				outlineFilter.sharedMesh = GetOutlineMesh(sourceFilter.sharedMesh);

				MeshRenderer outlineRenderer = outlineObject.AddComponent<MeshRenderer>();
				outlineRenderer.sharedMaterials = BuildMaterialArray(sourceRenderer.sharedMaterials);

				ApplyCommonRendererSettings(outlineRenderer);
				m_outlineRenderers.Add(outlineRenderer);
			}
		}

		private void BuildSkinnedMeshOutlines()
		{
			SkinnedMeshRenderer[] sourceRenderers = GetComponentsInChildren<SkinnedMeshRenderer>(m_includeInactiveChildren);

			foreach (SkinnedMeshRenderer sourceRenderer in sourceRenderers)
			{
				if (sourceRenderer == null) continue;
				if (sourceRenderer.gameObject.name.EndsWith(OutlineSuffix)) continue;
				if (sourceRenderer.sharedMesh == null) continue;

				GameObject outlineObject = new GameObject(sourceRenderer.gameObject.name + OutlineSuffix);
				outlineObject.transform.SetParent(sourceRenderer.transform, false);
				outlineObject.layer = sourceRenderer.gameObject.layer;

				SkinnedMeshRenderer outlineRenderer = outlineObject.AddComponent<SkinnedMeshRenderer>();
				outlineRenderer.sharedMesh = GetOutlineMesh(sourceRenderer.sharedMesh);
				outlineRenderer.rootBone = sourceRenderer.rootBone;
				outlineRenderer.bones = sourceRenderer.bones;
				outlineRenderer.localBounds = sourceRenderer.localBounds;
				outlineRenderer.updateWhenOffscreen = true;
				outlineRenderer.sharedMaterials = BuildMaterialArray(sourceRenderer.sharedMaterials);

				ApplyCommonRendererSettings(outlineRenderer);
				m_outlineRenderers.Add(outlineRenderer);
			}
		}

		private Mesh GetOutlineMesh(Mesh sourceMesh)
		{
			if (sourceMesh == null)
				return null;

			if (!m_useSmoothedNormalsForOutline)
				return sourceMesh;

			if (m_outlineMeshCache.TryGetValue(sourceMesh, out Mesh cachedMesh) && cachedMesh != null)
				return cachedMesh;

			Mesh outlineMesh = Instantiate(sourceMesh);
			outlineMesh.name = sourceMesh.name + "_SmoothedOutline";

			Vector3[] smoothedNormals = CalculateSmoothedNormals(sourceMesh);
			outlineMesh.SetNormals(smoothedNormals);
			outlineMesh.RecalculateBounds();

			m_outlineMeshCache[sourceMesh] = outlineMesh;
			m_generatedOutlineMeshes.Add(outlineMesh);

			return outlineMesh;
		}

		private Vector3[] CalculateSmoothedNormals(Mesh sourceMesh)
		{
			Vector3[] vertices = sourceMesh.vertices;
			Vector3[] sourceNormals = sourceMesh.normals;

			if (sourceNormals == null || sourceNormals.Length != vertices.Length)
			{
				Mesh tempMesh = Instantiate(sourceMesh);
				tempMesh.RecalculateNormals();
				sourceNormals = tempMesh.normals;

				if (Application.isPlaying)
					Destroy(tempMesh);
				else
					DestroyImmediate(tempMesh);
			}

			Dictionary<Vector3, List<int>> vertexGroups = new Dictionary<Vector3, List<int>>();

			for (int i = 0; i < vertices.Length; i++)
			{
				Vector3 vertex = vertices[i];

				if (!vertexGroups.TryGetValue(vertex, out List<int> group))
				{
					group = new List<int>();
					vertexGroups.Add(vertex, group);
				}

				group.Add(i);
			}

			Vector3[] smoothedNormals = new Vector3[vertices.Length];

			foreach (KeyValuePair<Vector3, List<int>> pair in vertexGroups)
			{
				List<int> indices = pair.Value;
				Vector3 averagedNormal = Vector3.zero;

				for (int i = 0; i < indices.Count; i++)
					averagedNormal += sourceNormals[indices[i]];

				if (averagedNormal.sqrMagnitude <= 0.000001f)
					averagedNormal = Vector3.up;
				else
					averagedNormal.Normalize();

				for (int i = 0; i < indices.Count; i++)
					smoothedNormals[indices[i]] = averagedNormal;
			}

			return smoothedNormals;
		}

		private Material[] BuildMaterialArray(Material[] sourceMaterials)
		{
			int count = (sourceMaterials != null && sourceMaterials.Length > 0) ? sourceMaterials.Length : 1;
			Material[] mats = new Material[count];

			for (int i = 0; i < count; i++)
				mats[i] = m_outlineMaterial;

			return mats;
		}

		private void ApplyCommonRendererSettings(Renderer renderer)
		{
			renderer.enabled = false;
			renderer.shadowCastingMode = ShadowCastingMode.Off;
			renderer.receiveShadows = false;
			renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
			renderer.lightProbeUsage = LightProbeUsage.Off;
			renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
			renderer.allowOcclusionWhenDynamic = false;
		}

		private void DestroyExistingOutlineObjects()
		{
			Transform[] allChildren = GetComponentsInChildren<Transform>(true);

			for (int i = allChildren.Length - 1; i >= 0; i--)
			{
				Transform t = allChildren[i];
				if (t == null || t == transform) continue;

				if (t.gameObject.name.EndsWith(OutlineSuffix))
				{
					if (Application.isPlaying)
						Destroy(t.gameObject);
					else
						DestroyImmediate(t.gameObject);
				}
			}
		}

		private void DestroyGeneratedOutlineMeshes()
		{
			for (int i = 0; i < m_generatedOutlineMeshes.Count; i++)
			{
				if (m_generatedOutlineMeshes[i] == null) continue;

				if (Application.isPlaying)
					Destroy(m_generatedOutlineMeshes[i]);
				else
					DestroyImmediate(m_generatedOutlineMeshes[i]);
			}

			m_generatedOutlineMeshes.Clear();
		}
	}
}