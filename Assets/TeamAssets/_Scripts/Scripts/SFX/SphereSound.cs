using Group26.Player.Movement;
using UnityEngine;

public class SphereSound : MonoBehaviour
{
    AudioSource source;
    Rigidbody rb;

    BallRollController ballRollController;

    float targetVol;
    float targetPitch;

    float velocity = 0f;
    float velocityb = 0f;

    private void Awake()
    {
        rb = transform.parent.GetComponent<Rigidbody>();
        ballRollController = transform.parent.GetComponent<BallRollController>();
    }

    private void OnEnable()
    {
        source = AudioManager.instance.PlaySoundFromObjectOnLoop(AudioManager.SoundType.ROLL, transform, spatialBlend: .6f);
    }
    private void Update()
    {
        if (!gameObject.activeSelf || !source) return;

        if (!ballRollController.IsGrounded()) { MoveToTargetSound(source, 0, 0); return; }

        float vel = rb.linearVelocity.magnitude;
        source.volume = Mathf.Clamp(vel / 32, 0, 5);
        source.pitch = Mathf.Clamp(vel / 30, .4f, 5);

        //MoveToTargetSound(source, targetVol, targetPitch);
    }

    private void MoveToTargetSound(AudioSource source, float tagetV, float targetP)
    {
        source.volume = Mathf.SmoothDamp(source.volume, tagetV, ref velocity, .3f);
        source.pitch = Mathf.SmoothDamp(source.pitch, targetP, ref velocityb, .3f);
    }
}
