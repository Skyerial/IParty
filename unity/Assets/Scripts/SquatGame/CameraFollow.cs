using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Transform target;

    [SerializeField] private float followLerpFactor = 10f;
    [SerializeField] private float yScreenOffset = 0f;
    [SerializeField] private float minY = 0f;
    [SerializeField] private float maxY = 100f;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        Debug.Log("Camera target set to: " + target.name);
    }

    void LateUpdate()
    {
        if (target == null) return;

        float targetY = Mathf.Clamp(target.position.y + yScreenOffset, minY, maxY);
        Vector3 current = transform.position;
        Vector3 desired = new Vector3(current.x, targetY, current.z);

        transform.position = Vector3.Lerp(current, desired, followLerpFactor * Time.deltaTime);
    }
}
