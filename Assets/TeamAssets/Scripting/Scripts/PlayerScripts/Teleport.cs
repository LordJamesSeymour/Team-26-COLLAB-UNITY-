using UnityEngine;

public class Teleport : MonoBehaviour
{
	//[SerializeField] Transform Start;
	[SerializeField] Transform TPPOsition;
	[SerializeField] GameObject playerRef;

	private void OnTriggerEnter(Collider other)
	{
		playerRef.transform.position = TPPOsition.position;
	}
}
