using UnityEngine;

public class LocomotionSettings : MonoBehaviour
{
    [Header("Drag locomotion objects/components here")]
    public GameObject teleportSystemRoot;
    public GameObject continuousMoveSystemRoot;
    public GameObject vignetteRoot;

    public void UseTeleport()
    {
        if (teleportSystemRoot != null) teleportSystemRoot.SetActive(true);
        if (continuousMoveSystemRoot != null) continuousMoveSystemRoot.SetActive(false);
    }

    public void UseContinuous()
    {
        if (teleportSystemRoot != null) teleportSystemRoot.SetActive(false);
        if (continuousMoveSystemRoot != null) continuousMoveSystemRoot.SetActive(true);
    }

    public void SetVignette(bool enabled)
    {
        if (vignetteRoot != null) vignetteRoot.SetActive(enabled);
    }
}