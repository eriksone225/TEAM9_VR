using UnityEngine;

public class CollisionAudioFeedback : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip collisionClip;
    public float minimumVelocity = 0.75f;
    public float cooldown = 0.15f;

    float lastPlayTime;

    void OnCollisionEnter(Collision collision)
    {
        if (audioSource == null || collisionClip == null) return;
        if (Time.time < lastPlayTime + cooldown) return;
        if (collision.relativeVelocity.magnitude < minimumVelocity) return;

        audioSource.PlayOneShot(collisionClip);
        lastPlayTime = Time.time;
    }
}