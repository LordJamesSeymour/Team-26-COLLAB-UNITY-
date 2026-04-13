using UnityEngine;

public class EmptyCustomLevelLoader : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private PrefabDatabase prefabDatabase;
	[SerializeField] private Transform levelRoot;
	[SerializeField] private Transform runtimePlayer;

	[Header("Options")]
	[SerializeField] private bool clearLevelRootOnLoad = true;

	private void Awake()
	{
		if (levelRoot == null)
		{
			GameObject root = new GameObject("LevelRoot");
			levelRoot = root.transform;
		}
	}

	private void Start()
	{
		LoadCurrentSessionMap();
	}

	public void LoadCurrentSessionMap()
	{
		if (!LevelEditorSession.HasOpenMap)
		{
			Debug.LogError("EmptyCustomLevelLoader: No map is currently open in LevelEditorSession.");
			return;
		}

		if (prefabDatabase == null)
		{
			Debug.LogError("EmptyCustomLevelLoader: PrefabDatabase reference is missing.");
			return;
		}

		if (clearLevelRootOnLoad)
			ClearLevelRootChildren();

		CustomMapData mapData = LevelEditorSession.CurrentMapData;

		if (mapData == null)
		{
			Debug.LogError("EmptyCustomLevelLoader: CurrentMapData is null.");
			return;
		}

		if (runtimePlayer != null)
			runtimePlayer.position = mapData.playerSpawn;

		if (mapData.placedObjects == null)
		{
			Debug.LogWarning("EmptyCustomLevelLoader: Map has no placed objects list.");
			return;
		}

		for (int i = 0; i < mapData.placedObjects.Count; i++)
		{
			PlacedObjectData objectData = mapData.placedObjects[i];

			if (objectData == null)
				continue;

			GameObject prefab = prefabDatabase.GetPrefab(objectData.prefabId);

			if (prefab == null)
			{
				Debug.LogWarning("EmptyCustomLevelLoader: Missing prefab for prefabId: " + objectData.prefabId);
				continue;
			}

			GameObject instance = Instantiate(prefab, levelRoot);
			instance.transform.localPosition = objectData.position;
			instance.transform.localRotation = Quaternion.Euler(objectData.rotationEuler);
			instance.transform.localScale = objectData.scale;

			PlacedLevelObject placedObject = instance.GetComponent<PlacedLevelObject>();
			if (placedObject == null)
				placedObject = instance.AddComponent<PlacedLevelObject>();

			placedObject.Initialize(objectData.prefabId);
		}

		Debug.Log("Loaded map into EmptyCustomLevel: " + mapData.mapName);
	}

	private void ClearLevelRootChildren()
	{
		for (int i = levelRoot.childCount - 1; i >= 0; i--)
			Destroy(levelRoot.GetChild(i).gameObject);
	}
}