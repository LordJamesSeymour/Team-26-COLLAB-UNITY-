#if UNITY_EDITOR
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Group26.Player.Movement;

[CustomEditor(typeof(RailSpline))]
public class RailSplineBakerEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		EditorGUILayout.Space(12f);

		if (GUILayout.Button("Bake Generated Rail Meshes To Assets"))
		{
			BakeRailMeshes((RailSpline)target);
		}
	}

	private static void BakeRailMeshes(RailSpline rail)
	{
		if (rail == null)
			return;

		// Force a fresh rebuild before baking
		MethodInfo rebuildMethod = typeof(RailSpline).GetMethod(
			"RebuildAll",
			BindingFlags.Instance | BindingFlags.NonPublic);

		rebuildMethod?.Invoke(rail, null);

		string rootFolder = "Assets/GeneratedRails";
		EnsureFolderExists("Assets", "GeneratedRails");

		string sceneName = rail.gameObject.scene.IsValid() && !string.IsNullOrEmpty(rail.gameObject.scene.name)
			? SanitizeName(rail.gameObject.scene.name)
			: "UnsavedScene";

		string sceneFolder = Path.Combine(rootFolder, sceneName).Replace("\\", "/");
		EnsureFolderExists(rootFolder, sceneName);

		string railFolderName = SanitizeName(rail.gameObject.name + "_" + rail.GetInstanceID());
		string railFolder = Path.Combine(sceneFolder, railFolderName).Replace("\\", "/");
		EnsureFolderExists(sceneFolder, railFolderName);

		MeshFilter[] meshFilters = rail.GetComponentsInChildren<MeshFilter>(true);
		int bakedCount = 0;

		foreach (MeshFilter meshFilter in meshFilters)
		{
			if (meshFilter == null)
				continue;

			Mesh sourceMesh = meshFilter.sharedMesh;
			if (sourceMesh == null)
				continue;

			if (sourceMesh.vertexCount == 0)
				continue;

			string meshName = SanitizeName(meshFilter.gameObject.name);
			string assetPath = Path.Combine(railFolder, meshName + ".asset").Replace("\\", "/");

			Mesh meshCopy = Object.Instantiate(sourceMesh);
			meshCopy.name = meshName;

			if (AssetDatabase.LoadAssetAtPath<Mesh>(assetPath) != null)
				AssetDatabase.DeleteAsset(assetPath);

			AssetDatabase.CreateAsset(meshCopy, assetPath);

			Mesh bakedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
			meshFilter.sharedMesh = bakedMesh;
			EditorUtility.SetDirty(meshFilter);

			MeshCollider meshCollider = meshFilter.GetComponent<MeshCollider>();
			if (meshCollider != null)
			{
				meshCollider.sharedMesh = null;
				meshCollider.sharedMesh = bakedMesh;
				EditorUtility.SetDirty(meshCollider);
			}

			bakedCount++;
		}

		EditorUtility.SetDirty(rail.gameObject);
		PrefabUtility.RecordPrefabInstancePropertyModifications(rail.gameObject);
		EditorSceneManager.MarkSceneDirty(rail.gameObject.scene);

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();

		Debug.Log($"Baked {bakedCount} rail mesh assets for '{rail.gameObject.name}' into: {railFolder}");
	}

	private static void EnsureFolderExists(string parentFolder, string childFolder)
	{
		string combined = Path.Combine(parentFolder, childFolder).Replace("\\", "/");
		if (!AssetDatabase.IsValidFolder(combined))
		{
			AssetDatabase.CreateFolder(parentFolder, childFolder);
		}
	}

	private static string SanitizeName(string value)
	{
		foreach (char c in Path.GetInvalidFileNameChars())
			value = value.Replace(c, '_');

		value = value.Replace(" ", "_");
		return value;
	}
}
#endif