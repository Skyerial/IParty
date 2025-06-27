using UnityEngine;
using System.Collections;

/**
 * @brief Camera Movement for tank game introduction.
 */
public class CameraMovement : MonoBehaviour
{
    public Transform startPoint;
    public Transform targetPoint;
    public float moveDuration = 3f;
    public float arcHeight = 3f; // Adjust for how high the arc is

    /**
     * @brief Initializes the camera movement coroutine when the script starts.
     */
    void Start()
    {
        StartCoroutine(MoveAlongArc());
    }

    /**
     * @brief Moves the camera along a quadratic Bezier curve from startPoint to targetPoint.
     * The camera will arc up to a specified height before descending to the target point.
     */
    IEnumerator MoveAlongArc()
    {
        float elapsed = 0f;

        Vector3 p0 = startPoint.position;
        Vector3 p2 = targetPoint.position;

        // Midpoint elevated for arc
        Vector3 midPoint = (p0 + p2) * 0.5f;
        midPoint.y += arcHeight;

        Quaternion startRot = startPoint.rotation;
        Quaternion endRot = targetPoint.rotation;

        while (elapsed < moveDuration)
        {
            float t = elapsed / moveDuration;

            // Quadratic Bezier curve
            Vector3 p1 = midPoint;
            Vector3 position = Mathf.Pow(1 - t, 2) * p0 +
                               2 * (1 - t) * t * p1 +
                               Mathf.Pow(t, 2) * p2;

            transform.position = position;

            // Smooth rotation
            transform.rotation = Quaternion.Slerp(startRot, endRot, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Snap to final position and rotation
        transform.position = p2;
        transform.rotation = endRot;
        GameManager gm = FindAnyObjectByType<GameManager>();
        gm.StartGame();
    }
}

