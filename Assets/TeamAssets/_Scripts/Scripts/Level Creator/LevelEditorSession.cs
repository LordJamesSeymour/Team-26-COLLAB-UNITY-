using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class LevelEditorSession
{
	public static string CurrentMapPath { get; private set; }
	public static CustomMapData CurrentMapData { get; private set; }

	public static bool HasOpenMap
	{
		get { return CurrentMapData != null; }
	}

	public static bool HasSavedFile
	{
		get { return !string.IsNullOrEmpty(CurrentMapPath); }
	}

	public static string CustomMapsFolderPath
	{
		get { return Path.Combine(Application.persistentDataPath, "CustomMaps"); }
	}

	public static void StartNewUnsavedMap()
	{
		CurrentMapPath = null;

		CurrentMapData = new CustomMapData
		{
			version = 1,
			mapName = "UnsavedMap",
			lastSavedUtc = "",
			playerSpawn = new Vector3(0f, 1f, 0f),
			placedObjects = new List<PlacedObjectData>()
		};

		Debug.Log("Started a new unsaved map in memory.");
	}

	public static void SaveCurrentMap(List<PlacedObjectData> placedObjects)
	{
		if (!HasOpenMap)
		{
			Debug.LogError("No open map exists. Cannot save.");
			return;
		}

		Directory.CreateDirectory(CustomMapsFolderPath);

		// First ever save for a new map:
		if (string.IsNullOrEmpty(CurrentMapPath))
		{
			CurrentMapPath = GetNextMapPath(CustomMapsFolderPath);
			CurrentMapData.mapName = Path.GetFileNameWithoutExtension(CurrentMapPath);
		}

		CurrentMapData.version = 1;
		CurrentMapData.lastSavedUtc = DateTime.UtcNow.ToString("o");
		CurrentMapData.placedObjects = placedObjects ?? new List<PlacedObjectData>();

		string json = JsonUtility.ToJson(CurrentMapData, true);
		File.WriteAllText(CurrentMapPath, json);

		Debug.Log("Saved map to disk: " + CurrentMapPath);
	}

	public static bool TryOpenExistingMap(string mapPath)
	{
		if (string.IsNullOrWhiteSpace(mapPath))
		{
			Debug.LogError("TryOpenExistingMap failed: path was empty.");
			return false;
		}

		if (!File.Exists(mapPath))
		{
			Debug.LogError("TryOpenExistingMap failed: file does not exist.\n" + mapPath);
			return false;
		}

		string json = File.ReadAllText(mapPath);
		CustomMapData loadedMap = JsonUtility.FromJson<CustomMapData>(json);

		if (loadedMap == null)
		{
			Debug.LogError("TryOpenExistingMap failed: JSON could not be parsed.");
			return false;
		}

		if (loadedMap.placedObjects == null)
			loadedMap.placedObjects = new List<PlacedObjectData>();

		CurrentMapPath = mapPath;
		CurrentMapData = loadedMap;

		Debug.Log("Opened saved map: " + CurrentMapPath);
		return true;
	}

	public static List<string> GetAllSavedMapPaths()
	{
		Directory.CreateDirectory(CustomMapsFolderPath);

		string[] files = Directory.GetFiles(CustomMapsFolderPath, "*.json");
		Array.Sort(files, StringComparer.OrdinalIgnoreCase);

		return new List<string>(files);
	}

	public static void CloseCurrentMap()
	{
		CurrentMapPath = null;
		CurrentMapData = null;
	}

	private static string GetNextMapPath(string folder)
	{
		int index = 1;

		while (true)
		{
			string fileName = $"Map{index}.json";
			string fullPath = Path.Combine(folder, fileName);

			if (!File.Exists(fullPath))
				return fullPath;

			index++;
		}
	}
}