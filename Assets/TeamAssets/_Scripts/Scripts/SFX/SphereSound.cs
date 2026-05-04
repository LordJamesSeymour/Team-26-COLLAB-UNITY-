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
        targetVol = Mathf.Clamp(vel / 30, 0, 1.5f);
        targetPitch = Mathf.Clamp(vel / 30, .4f, .7f);

        MoveToTargetSound(source, targetVol, targetPitch);
    }
    private void OnDisable()
    {
        AudioManager.instance.EndLoopingSound(source);
    }

    private void MoveToTargetSound(AudioSource source, float tagetV, float targetP)
    {
        source.volume = Mathf.SmoothDamp(source.volume, tagetV, ref velocity, .3f);
        source.pitch = Mathf.SmoothDamp(source.pitch, targetP, ref velocityb, .3f);
    }
}
