using UnityEngine;
using UnityEngine.InputSystem;

public class ResetInput : MonoBehaviour
{
    void Start()
    {   
        var devices = new System.Collections.Generic.List<InputDevice>(InputSystem.devices);
        foreach (var device in devices)
        {
            Debug.Log(device.name);
            if (device is Keyboard || device is Mouse)
                continue;

            InputSystem.RemoveDevice(device);
        }

        Debug.Log("All input devices removed on start.");
    }
}
