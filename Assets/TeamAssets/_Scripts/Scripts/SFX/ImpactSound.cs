using UnityEngine;

public class ImpactSound : MonoBehaviour
{
    [SerializeField] private GameObject m_ball;

    private void OnCollisionEnter(Collision other)
    {
        if (!m_ball.activeSelf) return;

        AudioManager.instance.PlaySoundAtPoint(AudioManager.SoundType.LAND, other.contacts[0].point, 1, .1f, 1, .1f, .8f);
    }
}
