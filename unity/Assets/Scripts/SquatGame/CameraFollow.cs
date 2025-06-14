using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Transform target;

    public float followSpeed = 30f;
    public float yScreenOffset = 0f; // zet deze op 0 om speler in het midden te houden
    public float minY = 0f;
    public float maxY = 100f;

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

        transform.position = Vector3.MoveTowards(current, desired, followSpeed * Time.deltaTime);
    }
}
