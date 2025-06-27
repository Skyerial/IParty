// Billboard.cs
using UnityEngine;

/**
 * @brief Orients the GameObject to always face the main camera.
 */
public class Billboard : MonoBehaviour
{
    Camera mainCam;

    /**
     * @brief Caches the main camera reference on startup.
     */
    void Start()
    {
        mainCam = Camera.main;
    }

    /**
     * @brief Rotates the canvas to face the camera every frame.
     */
    void LateUpdate()
    {
        // Make the Canvas face the camera every frame
        transform.LookAt(transform.position + mainCam.transform.rotation * Vector3.forward,
                         mainCam.transform.rotation * Vector3.up);
    }
}
