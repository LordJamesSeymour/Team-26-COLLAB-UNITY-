using UnityEngine;

public class Collectable_Tracker : MonoBehaviour
{
    [SerializeField] public static int m_pickupsCollected;

    public void CollectUSB()
    {
        m_pickupsCollected += 1;
        PlaySFX();
    }

    private void PlaySFX()
    {
        var audioManager = FindAnyObjectByType<AudioManager>();

        if (audioManager != null)        
        {
            audioManager.PlayOneShotSound(AudioManager.SoundType.COLLECTABLE, 0.3f);
        }
        else
        {
            Debug.LogWarning("AudioManager component not found on Collectable_Tracker.");
        }
    }

    public void ResetPickups()
    {
        m_pickupsCollected = 0;
    }
}