using UnityEngine;

public class NameTag : MonoBehaviour
{
    void LateUpdate()
    {
        if (Camera.main == null)
            return;

        Vector3 direction = transform.position - Camera.main.transform.position;
        direction.y = 0;
        transform.rotation = Quaternion.LookRotation(direction);
    }
}
