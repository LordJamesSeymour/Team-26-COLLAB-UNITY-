using UnityEngine;

public class ImpactSound : MonoBehaviour
{
    [SerializeField] private GameObject m_ball;

    private void OnCollisionEnter(Collision other)
    {
        if (!m_ball.activeSelf) return;
        //Debug.Log(Vector3.Angle(other.relativeVelocity, -other.contacts[0].normal));
        if (Vector3.Angle(other.relativeVelocity, -other.contacts[0].normal) < 130) return;

        AudioManager.instance.PlaySoundAtPoint(AudioManager.SoundType.CRASH, other.contacts[0].point, Mathf.Min(.5f, GetComponent<Rigidbody>().linearVelocity.magnitude / 20), .1f, 1, .1f, .8f);
    }
}
