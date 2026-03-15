using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlaceablePrefabEntry
{
	public string prefabId;
	public GameObject prefab;
	public Vector3 placementOffset;
}

public class PrefabDatabase : MonoBehaviour
{
	[SerializeField] private List<PlaceablePrefabEntry> entries = new List<PlaceablePrefabEntry>();

	private Dictionary<string, PlaceablePrefabEntry> lookup;

	private void Awake()
	{
		RebuildLookup();
	}

	private void OnValidate()
	{
		RebuildLookup();
	}

	public bool HasPrefab(string prefabId)
	{
		if (lookup == null)
			RebuildLookup();

		return !string.IsNullOrWhiteSpace(prefabId) && lookup.ContainsKey(prefabId);
	}

	public GameObject GetPrefab(string prefabId)
	{
		if (lookup == null)
			RebuildLookup();

		if (lookup.TryGetValue(prefabId, out PlaceablePrefabEntry entry))
			return entry.prefab;

		return null;
	}

	public Vector3 GetPlacementOffset(string prefabId)
	{
		if (lookup == null)
			RebuildLookup();

		if (lookup.TryGetValue(prefabId, out PlaceablePrefabEntry entry))
			return entry.placementOffset;

		return Vector3.zero;
	}

	private void RebuildLookup()
	{
		lookup = new Dictionary<string, PlaceablePrefabEntry>();

		for (int i = 0; i < entries.Count; i++)
		{
			PlaceablePrefabEntry entry = entries[i];

			if (entry == null)
				continue;

			if (string.IsNullOrWhiteSpace(entry.prefabId))
				continue;

			if (entry.prefab == null)
				continue;

			if (lookup.ContainsKey(entry.prefabId))
			{
				Debug.LogWarning("Duplicate prefabId found in PrefabDatabase: " + entry.prefabId);
				continue;
			}

			lookup.Add(entry.prefabId, entry);
		}
	}
}