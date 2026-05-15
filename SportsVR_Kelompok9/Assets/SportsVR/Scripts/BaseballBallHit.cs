using UnityEngine;

public class BaseballBallHit : MonoBehaviour
{
    public SportsVRGameManager gameManager;
    public float minimumHitPower = 2.5f;
    public AudioSource audioSource;
    public AudioClip hitClip;
    public float destroyAfterSeconds = 6f;

    bool counted;

    void Start()
    {
        Destroy(gameObject, destroyAfterSeconds);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (counted) return;
        if (!collision.collider.CompareTag("Bat")) return;

        float power = collision.relativeVelocity.magnitude;
        if (power < minimumHitPower) return;

        counted = true;

        if (audioSource != null && hitClip != null)
            audioSource.PlayOneShot(hitClip);

        if (gameManager != null)
            gameManager.RegisterBaseballHit(power);

        SimpleHaptics.Instance?.PulseBoth(0.85f, 0.12f);
    }
}