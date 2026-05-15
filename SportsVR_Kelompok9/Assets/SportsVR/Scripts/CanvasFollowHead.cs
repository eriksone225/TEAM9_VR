using UnityEngine;

public class CanvasFollowHead : MonoBehaviour
{
    [Header("Camera / Player Head")]
    public Transform headCamera;

    [Header("Placement")]
    public float distanceFromHead = 1.8f;
    public float heightOffset = -0.15f;
    public bool followEveryFrame = true;
    public bool onlyYawRotation = true;

    void Start()
    {
        if (headCamera == null && Camera.main != null)
            headCamera = Camera.main.transform;

        SnapInFrontOfPlayer();
    }

    void LateUpdate()
    {
        if (followEveryFrame)
            SnapInFrontOfPlayer();
    }

    public void SnapInFrontOfPlayer()
    {
        if (headCamera == null) return;

        Vector3 forward = headCamera.forward;
        if (onlyYawRotation)
        {
            forward.y = 0f;
            forward.Normalize();
        }

        transform.position = headCamera.position + forward * distanceFromHead + Vector3.up * heightOffset;
        transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
    }
}