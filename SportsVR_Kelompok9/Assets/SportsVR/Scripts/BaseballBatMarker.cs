using UnityEngine;

public class BaseballBatMarker : MonoBehaviour
{
    [Tooltip("Multiplier applied to hit power when this bat hits the baseball.")]
    public float hitMultiplier = 1f;

    void Reset()
    {
        gameObject.tag = "Bat";
    }

    void Awake()
    {
        if (gameObject.tag == "Untagged")
        {
            try
            {
                gameObject.tag = "Bat";
            }
            catch
            {
                Debug.LogWarning("Create a tag named 'Bat' in Unity if this warning appears, then assign it to the bat object.", this);
            }
        }
    }
}
