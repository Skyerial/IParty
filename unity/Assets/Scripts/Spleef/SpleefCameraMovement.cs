// SpleefCameraMovement.cs
using UnityEngine;
using System.Collections;

 /**
  * @brief Moves the camera along a parabolic arc from a start to a target point, then signals game start.
  */
public class SpleefCameraMovement : MonoBehaviour
{
     /**
      * @brief Transform representing the start position of the camera.
      */
    public Transform startPoint;

     /**
      * @brief Transform representing the target position of the camera.
      */
    public Transform targetPoint;

     /**
      * @brief Duration (in seconds) for the camera to move along the arc.
      */
    public float moveDuration = 3f;

     /**
      * @brief Maximum height of the arc above the linear path.
      */
    public float arcHeight = 3f;

     /**
      * @brief Unity event called on Start; begins the MoveAlongArc coroutine.
      */
    void Start()
    {
        StartCoroutine(MoveAlongArc());
    }

     /**
      * @brief Coroutine that interpolates position and rotation along a quadratic Bézier curve, then starts the game.
      */
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
        SpleefGameManager gm = FindAnyObjectByType<SpleefGameManager>();
        gm.StartGame();
    }
}
