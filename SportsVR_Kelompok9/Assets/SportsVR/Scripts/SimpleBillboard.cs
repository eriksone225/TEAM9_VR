using UnityEngine;

public class SimpleBillboard : MonoBehaviour
{
    public Transform target;

    void LateUpdate()
    {
        if (target == null && Camera.main != null)
            target = Camera.main.transform;

        if (target == null) return;

        Vector3 direction = transform.position - target.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(direction);
    }
}