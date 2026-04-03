using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class LevelEditorManager : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private PrefabDatabase prefabDatabase;
	[SerializeField] private Camera editorCamera;
	[SerializeField] private Transform levelRoot;

	[Header("Scene Names")]
	[SerializeField] private string mainMenuSceneName = "LevelEditorMenu";

	[Header("Placement")]
	[SerializeField] private LayerMask placementLayers = ~0;
	[SerializeField] private float gridSize = 1f;
	[SerializeField] private float rotationStepDegrees = 90f;

	private string selectedPrefabId;
	private float currentYRotation;

	private void Awake()
	{
		if (editorCamera == null)
			editorCamera = Camera.main;

		if (levelRoot == null)
		{
			GameObject root = new GameObject("LevelRoot");
			levelRoot = root.transform;
		}
	}

	private void Start()
	{
		if (prefabDatabase == null)
		{
			Debug.LogError("LevelEditorManager is missing a PrefabDatabase reference.");
			enabled = false;
			return;
		}

		if (editorCamera == null)
		{
			Debug.LogError("LevelEditorManager could not find an editor camera.");
			enabled = false;
			return;
		}

		if (!LevelEditorSession.HasOpenMap)
		{
			Debug.LogWarning("No active map session was found. Creating a new unsaved blank map automatically.");
			LevelEditorSession.StartNewUnsavedMap();
		}

		RebuildSceneFromSessionData();
	}

	private void Update()
	{
		HandleHotkeys();
		HandleMouseInput();
	}

	public void SelectPrefabById(string prefabId)
	{
		if (string.IsNullOrWhiteSpace(prefabId))
		{
			Debug.LogWarning("SelectPrefabById failed: prefabId was empty.");
			return;
		}

		if (!prefabDatabase.HasPrefab(prefabId))
		{
			Debug.LogWarning("SelectPrefabById failed: prefabId not found in PrefabDatabase: " + prefabId);
			return;
		}

		selectedPrefabId = prefabId;
		Debug.Log("Selected prefab: " + selectedPrefabId);
	}

	public void ClearSelectedPrefab()
	{
		selectedPrefabId = null;
		Debug.Log("Cleared prefab selection.");
	}

	public void RotateSelectionLeft()
	{
		currentYRotation -= rotationStepDegrees;
		Debug.Log("Placement rotation: " + currentYRotation);
	}

	public void RotateSelectionRight()
	{
		currentYRotation += rotationStepDegrees;
		Debug.Log("Placement rotation: " + currentYRotation);
	}

	public void SaveLevel()
	{
		List<PlacedObjectData> placedObjects = CollectPlacedObjects();
		LevelEditorSession.SaveCurrentMap(placedObjects);
		Debug.Log("Saved level with " + placedObjects.Count + " placed objects.");
	}

	public void ReturnToMainMenu()
	{
		SceneManager.LoadScene(mainMenuSceneName);
	}

	private void HandleHotkeys()
	{
		if (Keyboard.current == null)
			return;

		if (Keyboard.current.qKey.wasPressedThisFrame)
			RotateSelectionLeft();

		if (Keyboard.current.eKey.wasPressedThisFrame)
			RotateSelectionRight();

		if (Keyboard.current.leftCtrlKey.isPressed && Keyboard.current.sKey.wasPressedThisFrame)
			SaveLevel();
	}

	private void HandleMouseInput()
	{
		if (Mouse.current == null)
			return;

		if (IsPointerOverUI())
			return;

		if (Mouse.current.leftButton.wasPressedThisFrame)
			TryPlaceSelectedPrefab();

		if (Mouse.current.rightButton.wasPressedThisFrame)
			TryDeleteObjectUnderCursor();
	}

	private void TryPlaceSelectedPrefab()
	{
		if (string.IsNullOrWhiteSpace(selectedPrefabId))
		{
			Debug.Log("No prefab selected. Select a prefab button first.");
			return;
		}

		if (!TryGetMouseRaycast(out RaycastHit hit))
			return;

		GameObject prefab = prefabDatabase.GetPrefab(selectedPrefabId);

		if (prefab == null)
		{
			Debug.LogWarning("Could not place object. Prefab not found for id: " + selectedPrefabId);
			return;
		}

		Quaternion spawnRotation = Quaternion.Euler(0f, currentYRotation, 0f);

		Vector3 snappedPoint = SnapToGrid(hit.point);
		Vector3 placementOffset = prefabDatabase.GetPlacementOffset(selectedPrefabId);
		Vector3 worldPosition = snappedPoint + (spawnRotation * placementOffset);

		GameObject instance = Instantiate(prefab, worldPosition, spawnRotation, levelRoot);

		PlacedLevelObject placedObject = instance.GetComponent<PlacedLevelObject>();
		if (placedObject == null)
			placedObject = instance.AddComponent<PlacedLevelObject>();

		placedObject.Initialize(selectedPrefabId);
	}

	private void TryDeleteObjectUnderCursor()
	{
		if (!TryGetMouseRaycast(out RaycastHit hit))
			return;

		PlacedLevelObject placedObject = hit.collider.GetComponentInParent<PlacedLevelObject>();
		if (placedObject == null)
			return;

		Destroy(placedObject.gameObject);
	}

	private bool TryGetMouseRaycast(out RaycastHit hit)
	{
		hit = default;

		if (editorCamera == null || Mouse.current == null)
			return false;

		Vector2 mousePosition = Mouse.current.position.ReadValue();
		Ray ray = editorCamera.ScreenPointToRay(mousePosition);

		return Physics.Raycast(ray, out hit, 1000f, placementLayers);
	}

	private Vector3 SnapToGrid(Vector3 worldPosition)
	{
		if (gridSize <= 0f)
			return worldPosition;

		worldPosition.x = Mathf.Round(worldPosition.x / gridSize) * gridSize;
		worldPosition.y = Mathf.Round(worldPosition.y / gridSize) * gridSize;
		worldPosition.z = Mathf.Round(worldPosition.z / gridSize) * gridSize;
		return worldPosition;
	}

	private bool IsPointerOverUI()
	{
		return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
	}

	private List<PlacedObjectData> CollectPlacedObjects()
	{
		List<PlacedObjectData> results = new List<PlacedObjectData>();

		PlacedLevelObject[] placedObjects = levelRoot.GetComponentsInChildren<PlacedLevelObject>(true);

		for (int i = 0; i < placedObjects.Length; i++)
		{
			PlacedLevelObject placedObject = placedObjects[i];
			if (placedObject == null)
				continue;

			Transform t = placedObject.transform;

			PlacedObjectData data = new PlacedObjectData
			{
				prefabId = placedObject.PrefabId,
				position = t.localPosition,
				rotationEuler = t.localEulerAngles,
				scale = t.localScale
			};

			results.Add(data);
		}

		return results;
	}

	private void RebuildSceneFromSessionData()
	{
		ClearLevelRootChildren();

		if (!LevelEditorSession.HasOpenMap)
			return;

		CustomMapData mapData = LevelEditorSession.CurrentMapData;
		if (mapData == null || mapData.placedObjects == null)
			return;

		for (int i = 0; i < mapData.placedObjects.Count; i++)
		{
			PlacedObjectData objectData = mapData.placedObjects[i];
			if (objectData == null)
				continue;

			GameObject prefab = prefabDatabase.GetPrefab(objectData.prefabId);
			if (prefab == null)
			{
				Debug.LogWarning("Skipping saved object because prefabId was not found: " + objectData.prefabId);
				continue;
			}

			Quaternion rotation = Quaternion.Euler(objectData.rotationEuler);

			GameObject instance = Instantiate(prefab, levelRoot);
			instance.transform.localPosition = objectData.position;
			instance.transform.localRotation = rotation;
			instance.transform.localScale = objectData.scale;

			PlacedLevelObject placedObject = instance.GetComponent<PlacedLevelObject>();
			if (placedObject == null)
				placedObject = instance.AddComponent<PlacedLevelObject>();

			placedObject.Initialize(objectData.prefabId);
		}

		Debug.Log("Loaded current session map into editor: " + LevelEditorSession.CurrentMapData.mapName);
	}

	private void ClearLevelRootChildren()
	{
		for (int i = levelRoot.childCount - 1; i >= 0; i--)
			Destroy(levelRoot.GetChild(i).gameObject);
	}
}