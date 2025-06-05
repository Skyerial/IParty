using UnityEngine;

namespace Assets.Scripts.TNT
{
    public class Billboard : MonoBehaviour
    {
        Camera mainCam;
        void Start()
        {
            mainCam = Camera.main;
        }
        void LateUpdate()
        {
            // Make the Canvas face the camera every frame
            transform.LookAt(transform.position + mainCam.transform.rotation * Vector3.forward,
                             mainCam.transform.rotation * Vector3.up);
        }
    }
}