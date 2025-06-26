using UnityEngine;
using System.Collections;

public class TurfCameraMovement : MonoBehaviour
{
    public Transform startPoint;
    public Transform targetPoint;
    public float moveDuration = 3f;
    public float arcHeight = 3f;

    void Start()
    {
        StartCoroutine(MoveAlongArc());
    }

    IEnumerator MoveAlongArc()
    {
        float elapsed = 0f;

        Vector3 p0 = startPoint.position;
        Vector3 p2 = targetPoint.position;

        Vector3 midPoint = (p0 + p2) * 0.5f;
        midPoint.y += arcHeight;

        Quaternion startRot = startPoint.rotation;
        Quaternion endRot = targetPoint.rotation;

        while (elapsed < moveDuration)
        {
            float t = elapsed / moveDuration;

            Vector3 p1 = midPoint;
            Vector3 position = Mathf.Pow(1 - t, 2) * p0 +
                               2 * (1 - t) * t * p1 +
                               Mathf.Pow(t, 2) * p2;

            transform.position = position;

            transform.rotation = Quaternion.Slerp(startRot, endRot, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = p2;
        transform.rotation = endRot;
        TurfGameManager gm = FindAnyObjectByType<TurfGameManager>();
        gm.StartGame();
    }
}

