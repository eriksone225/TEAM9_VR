using UnityEngine;

public class BallResetter : MonoBehaviour
{
    public Transform resetPoint;
    public float resetIfBelowY = -3f;
    public bool resetAfterScore = true;
    public float resetDelayAfterScore = 1.0f;

    Rigidbody rb;
    BasketballBallInfo basketballInfo;
    Vector3 startPosition;
    Quaternion startRotation;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        basketballInfo = GetComponent<BasketballBallInfo>();

        if (resetPoint == null)
        {
            startPosition = transform.position;
            startRotation = transform.rotation;
        }
        else
        {
            startPosition = resetPoint.position;
            startRotation = resetPoint.rotation;
        }
    }

    void Update()
    {
        if (transform.position.y < resetIfBelowY)
            ResetNow();

        if (resetAfterScore && basketballInfo != null && basketballInfo.hasScored)
        {
            Invoke(nameof(ResetNow), resetDelayAfterScore);
            resetAfterScore = false;
        }
    }

    public void ResetNow()
    {
        CancelInvoke();

        transform.position = resetPoint != null ? resetPoint.position : startPosition;
        transform.rotation = resetPoint != null ? resetPoint.rotation : startRotation;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.Sleep();
        }

        if (basketballInfo != null)
            basketballInfo.ResetBallState();

        resetAfterScore = true;
    }
}