using UnityEngine;

public class Collectable_Tracker : MonoBehaviour
{
    [SerializeField] public static int m_pickupsCollected;

    public void CollectUSB()
    {
        m_pickupsCollected += 1;
        Debug.Log(m_pickupsCollected);
    }
}