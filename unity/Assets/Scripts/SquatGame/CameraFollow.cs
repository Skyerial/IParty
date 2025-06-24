using UnityEngine;

/**
 * @brief Follows a target's vertical position with clamped movement and smooth interpolation.
 */
public class CameraFollow : MonoBehaviour
{
    private Transform target;

    [Header("Y Follow Settings")]
    [SerializeField]
    private float yScreenOffset = 0f;

    [SerializeField]
    private float minY = -1f;

    [SerializeField]
    private float maxY = 9999f;

    [Header("Follow Speed")]
    [SerializeField]
    private float followSpeed = 100f;

    /**
     * @brief Sets the transform the camera should follow.
     * @param newTarget The target to follow.
     */
    public void SetTarget(Transform newTarget)
    {
        if (newTarget == null)
        {
            return;
        }

        target = newTarget;
    }

    /**
     * @brief Unity callback called after all Update() calls.
     * Smoothly moves the camera to follow the target on the Y axis, within bounds.
     */
    void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 current = transform.position;
        float targetY = Mathf.Clamp(target.position.y + yScreenOffset, minY, maxY);
        Vector3 desired = new Vector3(current.x, targetY, current.z);
        transform.position = Vector3.MoveTowards(current, desired, followSpeed * Time.deltaTime);
    }
}
