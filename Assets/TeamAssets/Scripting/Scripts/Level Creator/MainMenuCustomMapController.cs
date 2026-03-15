using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuCustomMapController : MonoBehaviour
{
	[Header("Scene Names")]
	[SerializeField] private string levelEditorSceneName = "LevelEditorTemplate";
	[SerializeField] private string runtimeLoadSceneName = "EmptyCustomLevel";

	[Header("Load Menu UI")]
	[SerializeField] private GameObject loadPanel;
	[SerializeField] private Transform loadButtonContainer;
	[SerializeField] private Button loadButtonTemplate;

	private void Start()
	{
		if (loadPanel != null)
			loadPanel.SetActive(false);

		if (loadButtonTemplate != null)
			loadButtonTemplate.gameObject.SetActive(false);
	}

	public void CreateNewLevelAndOpenEditor()
	{
		LevelEditorSession.StartNewUnsavedMap();
		SceneManager.LoadScene(levelEditorSceneName);
	}

	public void OpenLoadMenu()
	{
		Debug.Log("OpenLoadMenu called.");

		if (loadPanel != null)
			loadPanel.SetActive(true);

		RefreshLoadMenu();
	}

	public void CloseLoadMenu()
	{
		Debug.Log("CloseLoadMenu called.");

		if (loadPanel != null)
			loadPanel.SetActive(false);
	}

	public void RefreshLoadMenu()
	{
		Debug.Log("RefreshLoadMenu called.");

		if (loadButtonContainer == null)
		{
			Debug.LogError("Load Button Container is missing.");
			return;
		}

		if (loadButtonTemplate == null)
		{
			Debug.LogError("Load Button Template is missing.");
			return;
		}

		ClearSpawnedButtons();

		var savedMapPaths = LevelEditorSession.GetAllSavedMapPaths();

		Debug.Log("Saved maps found: " + savedMapPaths.Count);
		Debug.Log("Maps folder: " + LevelEditorSession.CustomMapsFolderPath);

		if (savedMapPaths.Count == 0)
		{
			Button emptyButton = SpawnVisibleClone("No saved maps found");
			emptyButton.interactable = false;
			return;
		}

		for (int i = 0; i < savedMapPaths.Count; i++)
		{
			string mapPath = savedMapPaths[i];
			string mapName = Path.GetFileNameWithoutExtension(mapPath);

			Button button = SpawnVisibleClone(mapName);
			button.interactable = true;

			button.onClick.RemoveAllListeners();
			button.onClick.AddListener(() => LoadSelectedMap(mapPath));

			Debug.Log("Spawned map button: " + mapName);
		}

		Canvas.ForceUpdateCanvases();

		RectTransform containerRect = loadButtonContainer as RectTransform;
		if (containerRect != null)
			LayoutRebuilder.ForceRebuildLayoutImmediate(containerRect);

		Debug.Log("Container child count after refresh: " + loadButtonContainer.childCount);
	}

	public void LoadSelectedMap(string mapPath)
	{
		bool opened = LevelEditorSession.TryOpenExistingMap(mapPath);

		if (!opened)
		{
			Debug.LogError("Failed to open map: " + mapPath);
			return;
		}

		SceneManager.LoadScene(runtimeLoadSceneName);
	}

	private Button SpawnVisibleClone(string labelText)
	{
		GameObject clone = Instantiate(loadButtonTemplate.gameObject, loadButtonContainer, false);
		clone.name = "LoadButton_" + labelText;
		clone.SetActive(true);
		clone.transform.SetAsLastSibling();

		RectTransform rt = clone.GetComponent<RectTransform>();
		if (rt != null)
		{
			rt.localScale = Vector3.one;
			rt.localRotation = Quaternion.identity;
			rt.sizeDelta = new Vector2(300f, 50f);
		}

		LayoutElement layout = clone.GetComponent<LayoutElement>();
		if (layout == null)
			layout = clone.AddComponent<LayoutElement>();

		layout.preferredWidth = 300f;
		layout.preferredHeight = 50f;
		layout.flexibleWidth = 0f;
		layout.flexibleHeight = 0f;

		Button button = clone.GetComponent<Button>();
		if (button == null)
		{
			Debug.LogError("Spawned clone is missing a Button component.");
			return null;
		}

		TMP_Text tmp = clone.GetComponentInChildren<TMP_Text>(true);
		if (tmp != null)
			tmp.text = labelText;
		else
			Debug.LogWarning("Spawned button clone has no TMP_Text child.");

		Image image = clone.GetComponent<Image>();
		if (image != null)
		{
			Color c = image.color;
			c.a = 1f;
			image.color = c;
		}

		return button;
	}

	private void ClearSpawnedButtons()
	{
		for (int i = loadButtonContainer.childCount - 1; i >= 0; i--)
		{
			Transform child = loadButtonContainer.GetChild(i);

			if (loadButtonTemplate != null && child == loadButtonTemplate.transform)
				continue;

			Destroy(child.gameObject);
		}
	}
}