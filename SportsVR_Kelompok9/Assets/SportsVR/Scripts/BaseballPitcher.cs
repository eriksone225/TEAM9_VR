using UnityEngine;

public class BaseballPitcher : MonoBehaviour
{
    [Header("References")]
    public GameObject baseballPrefab;
    public Transform spawnPoint;
    public Transform targetPoint;
    public SportsVRGameManager gameManager;

    [Header("Speed Settings")]
    public float slowSpeed = 8f;
    public float mediumSpeed = 14f;
    public float fastSpeed = 20f;
    public float pitchInterval = 3f;

    [Header("Runtime")]
    public float currentSpeed = 8f;
    public bool autoPitch;

    float nextPitchTime;

    void Start()
    {
        currentSpeed = slowSpeed;
    }

    void Update()
    {
        if (!autoPitch) return;

        if (Time.time >= nextPitchTime)
        {
            PitchOne();
            nextPitchTime = Time.time + pitchInterval;
        }
    }

    public void SetSlow()
    {
        currentSpeed = slowSpeed;
    }

    public void SetMedium()
    {
        currentSpeed = mediumSpeed;
    }

    public void SetFast()
    {
        currentSpeed = fastSpeed;
    }

    public void StartAutoPitch()
    {
        autoPitch = true;
        nextPitchTime = Time.time + 0.5f;
    }

    public void StopAutoPitch()
    {
        autoPitch = false;
    }

    public void PitchOne()
    {
        if (baseballPrefab == null || spawnPoint == null || targetPoint == null) return;

        GameObject ball = Instantiate(baseballPrefab, spawnPoint.position, spawnPoint.rotation);
        BaseballBallHit hit = ball.GetComponent<BaseballBallHit>();
        if (hit != null) hit.gameManager = gameManager;

        Rigidbody rb = ball.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 dir = (targetPoint.position - spawnPoint.position).normalized;
            rb.linearVelocity = dir * currentSpeed;
            rb.angularVelocity = Random.insideUnitSphere * 8f;
        }

        if (gameManager != null)
            gameManager.RegisterBaseballPitch();
    }
}