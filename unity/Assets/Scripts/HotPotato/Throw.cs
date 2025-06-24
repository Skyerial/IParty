using UnityEngine;
using System.Collections;

/**
 * @brief Handles bomb throwing animation using a simple parabolic arc.
 */
public class Throw : MonoBehaviour
{
    public float throwDuration = 0.5f;
    public float arcHeight = 2f;

    /**
     * @brief Initiates a throw from the current position to the target with arc motion.
     * @param target The transform to throw the object to.
     * @param bombScript Reference to the Bomb script to update throw status.
     * @return void
     */
    public void ThrowToTarget(Transform target, Bomb bombScript)
    {
        StartCoroutine(ThrowArc(target, bombScript));
    }

    /**
     * @brief Coroutine that animates the throw over time in a parabolic arc.
     * @param target The target transform to throw to.
     * @param bombScript Reference to the Bomb script to reset throw state at the end.
     * @return IEnumerator Coroutine handle.
     */
    IEnumerator ThrowArc(Transform target, Bomb bombScript)
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = target.position + new Vector3(0, 2f, 0);
        float elapsed = 0f;

        while (elapsed < throwDuration)
        {
            float t = elapsed / throwDuration;

            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, t);
            currentPos.y += arcHeight * 4f * (t - t * t); // Parabola formula

            transform.position = currentPos;

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
        transform.SetParent(target);
        transform.localPosition = new Vector3(0, 2f, 0);
        transform.localRotation = Quaternion.identity;
        bombScript.isBeingThrown = false;
    }
}
