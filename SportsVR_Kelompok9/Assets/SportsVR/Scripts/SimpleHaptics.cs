using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class SimpleHaptics : MonoBehaviour
{
    public static SimpleHaptics Instance;

    void Awake()
    {
        Instance = this;
    }

    public void PulseBoth(float amplitude, float duration)
    {
        Pulse(InputDeviceCharacteristics.Left, amplitude, duration);
        Pulse(InputDeviceCharacteristics.Right, amplitude, duration);
    }

    public void PulseLeft(float amplitude, float duration)
    {
        Pulse(InputDeviceCharacteristics.Left, amplitude, duration);
    }

    public void PulseRight(float amplitude, float duration)
    {
        Pulse(InputDeviceCharacteristics.Right, amplitude, duration);
    }

    void Pulse(InputDeviceCharacteristics hand, float amplitude, float duration)
    {
        var devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(hand | InputDeviceCharacteristics.Controller, devices);

        foreach (var device in devices)
        {
            if (device.isValid)
                device.SendHapticImpulse(0u, Mathf.Clamp01(amplitude), Mathf.Max(0.01f, duration));
        }
    }
}