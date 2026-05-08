using UnityEngine;

public class ImpactSound : MonoBehaviour
{
    [SerializeField] private GameObject m_ball;

    private void OnCollisionEnter(Collision other)
    {
        if (!m_ball.activeSelf) return;
        //Debug.Log(Vector3.Angle(other.relativeVelocity, -other.contacts[0].normal));
        if (Vector3.Angle(other.relativeVelocity, -other.contacts[0].normal) < 130) return;

        //AudioManager.instance.PlaySoundAtPoint(AudioManager.SoundType.CRASH, other.contacts[0].point, Mathf.Clamp((other.relativeVelocity.magnitude - 15) / 90, .02f, .35f), .02f, .6f, .07f, 0);
        AudioManager.instance.PlaySoundAtPoint(AudioManager.SoundType.CRASH, other.contacts[0].point, Mathf.Clamp((other.relativeVelocity.magnitude - 10) / 30, .03f, 1), .01f, 1.8f, .07f, 0);
    }
}
