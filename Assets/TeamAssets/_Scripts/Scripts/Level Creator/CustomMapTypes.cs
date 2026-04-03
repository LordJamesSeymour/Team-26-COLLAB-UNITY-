using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlacedObjectData
{
	public string prefabId;
	public Vector3 position;
	public Vector3 rotationEuler;
	public Vector3 scale;
}

[Serializable]
public class CustomMapData
{
	public int version = 1;
	public string mapName;
	public string lastSavedUtc;
	public Vector3 playerSpawn = new Vector3(0f, 1f, 0f);
	public List<PlacedObjectData> placedObjects = new List<PlacedObjectData>();
}