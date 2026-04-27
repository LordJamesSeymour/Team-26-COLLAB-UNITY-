using Group26.Player.Movement;
using UnityEngine;

public class SphereSound : MonoBehaviour
{
    AudioSource source;
    Rigidbody rb;

    BallRollController ballRollController;

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

        if (!ballRollController.IsGrounded()) { source.volume = 0; return; }

        float vel = rb.linearVelocity.magnitude;

        source.volume = Mathf.Clamp(vel / 15, 0, 1);
        source.pitch = Mathf.Clamp(vel / 35, .3f, 5);
    }
    private void OnDisable()
    {
        AudioManager.instance.EndLoopingSound(source);
    }
}
