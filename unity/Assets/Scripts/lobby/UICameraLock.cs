using UnityEngine;

/**
  * @brief Locks the camera to a canvas in the UI.
  * This script ensures that the camera follows a specified canvas
  * with a defined offset, allowing for a consistent view of the UI elements.
  */
public class UICameraLock : MonoBehaviour
{
        public Transform canvasTransform;
        public Vector3 offset = new Vector3(0, 0, -10f);

        void LateUpdate()
        {
            transform.position = canvasTransform.position + offset;
            transform.LookAt(canvasTransform);
        }
}
