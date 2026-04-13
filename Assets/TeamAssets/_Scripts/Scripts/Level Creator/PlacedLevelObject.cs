using UnityEngine;

public class PlacedLevelObject : MonoBehaviour
{
	[SerializeField] private string prefabId;

	public string PrefabId
	{
		get { return prefabId; }
	}

	public void Initialize(string newPrefabId)
	{
		prefabId = newPrefabId;
	}
}