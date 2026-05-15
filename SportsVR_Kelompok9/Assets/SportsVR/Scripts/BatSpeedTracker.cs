using UnityEngine;

public class BatSpeedTracker : MonoBehaviour
{
    public Vector3 currentVelocity;
    Vector3 lastPosition;

    void Start()
    {
        lastPosition = transform.position;
    }

    void FixedUpdate()
    {
        currentVelocity = (transform.position - lastPosition) / Time.fixedDeltaTime;
        lastPosition = transform.position;
    }
}