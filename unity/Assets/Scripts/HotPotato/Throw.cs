using UnityEngine;
using System.Collections;
public class Throw : MonoBehaviour
{
    public float throwDuration = 0.5f;
    public float arcHeight = 2f;

    public void ThrowToTarget(Transform target, Bomb bombScript)
    {
        StartCoroutine(ThrowArc(target, bombScript));
    }

    IEnumerator ThrowArc(Transform target, Bomb bombScript)
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = target.position + new Vector3(0, 2f, 0);
        float elapsed = 0f;

        while (elapsed < throwDuration)
        {
            float t = elapsed / throwDuration;

            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, t);
            currentPos.y += arcHeight * 4f * (t - t * t);

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
