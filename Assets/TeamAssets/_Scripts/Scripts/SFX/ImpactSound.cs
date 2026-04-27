using UnityEngine;

public class ImpactSound : MonoBehaviour
{
    [SerializeField] private GameObject m_ball;

    private void OnCollisionEnter(Collision other)
    {
        if (!m_ball.activeSelf) return;
        Debug.Log(Vector3.Angle(other.relativeVelocity, -other.contacts[0].normal));
        if (Vector3.Angle(other.relativeVelocity, -other.contacts[0].normal) < 140) return;

        AudioManager.instance.PlaySoundAtPoint(AudioManager.SoundType.LAND, other.contacts[0].point, 1, .1f, 1, .1f, .8f);
    }
}
