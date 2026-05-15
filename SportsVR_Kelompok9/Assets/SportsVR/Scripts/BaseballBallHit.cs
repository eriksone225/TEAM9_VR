using UnityEngine;

public class BaseballBallHit : MonoBehaviour
{
    public SportsVRGameManager gameManager;
    public float minimumHitPower = 1.25f;
    public AudioSource audioSource;
    public AudioClip hitClip;
    public float destroyAfterSeconds = 6f;
    public float hitForwardBoost = 1.2f;

    bool counted;

    void Start()
    {
        Destroy(gameObject, destroyAfterSeconds);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (counted) return;

        BaseballBatMarker batMarker = collision.collider.GetComponentInParent<BaseballBatMarker>();
        BatSpeedTracker tracker = collision.collider.GetComponentInParent<BatSpeedTracker>();
        string objectName = collision.collider.transform.root.name.ToLowerInvariant();
        bool looksLikeBat = objectName.Contains("bat");

        if (batMarker == null && tracker == null && !looksLikeBat) return;

        float power = collision.relativeVelocity.magnitude;
        if (tracker != null)
            power = Mathf.Max(power, tracker.currentVelocity.magnitude);

        if (batMarker != null)
            power *= Mathf.Max(0.1f, batMarker.hitMultiplier);

        if (power < minimumHitPower) return;

        counted = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null && collision.contactCount > 0)
        {
            Vector3 away = (transform.position - collision.GetContact(0).point).normalized;
            rb.linearVelocity = away * Mathf.Max(power * hitForwardBoost, rb.linearVelocity.magnitude);
        }

        if (audioSource != null && hitClip != null)
            audioSource.PlayOneShot(hitClip);

        if (gameManager != null)
            gameManager.RegisterBaseballHit(power);

        SimpleHaptics.Instance?.PulseBoth(0.85f, 0.12f);
    }
}
