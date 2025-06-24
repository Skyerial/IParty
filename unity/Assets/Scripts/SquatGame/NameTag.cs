using UnityEngine;

/**
 * @brief Rotates a name tag so it always faces the main camera horizontally.
 */
public class NameTag : MonoBehaviour
{
    /**
     * @brief Unity callback called after all Update() calls.
     * Rotates the object to face the camera, ignoring vertical tilt.
     */
    void LateUpdate()
    {
        if (Camera.main == null)
            return;

        Vector3 direction = transform.position - Camera.main.transform.position;
        direction.y = 0;
        transform.rotation = Quaternion.LookRotation(direction);
    }
}
