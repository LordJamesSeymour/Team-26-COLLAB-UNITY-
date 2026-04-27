using UnityEngine;

public class ObjectSpawner : Interactable_Parent
{
    [Header("Parameter")]
    [SerializeField] private GameObject m_objectToSpawn;
    [SerializeField] private Transform m_spawnPos;

    public override void InteractImplementation()
    {
        if (m_objectToSpawn != null)
        {
            Vector3 position = Vector3.zero;
            if (m_spawnPos != null)
                position = m_spawnPos.position;
            else
                Debug.LogWarning("No spawnPos provided on " + this.name + ". Spawning at 0,0,0");
            GameObject spawnedObject = GameObject.Instantiate(m_objectToSpawn, position, Quaternion.Euler(0.0f, 0.0f, 0.0f));
        }
    }
}
