// Mole.cs
using UnityEngine;
using System.Collections;

public class Mole : MonoBehaviour
{
    [Tooltip("How far (in units) the mole dips when hit")]
    public float popDownDistance = 0.5f;
    [Tooltip("Seconds to dip down, and the same to pop back up")]
    public float popTime = 0.2f;

    private Vector3 initialPosition;
    private Coroutine popRoutine;

    void Awake()
    {
        initialPosition = transform.position;
    }

    public void OnHit()
    {
        Debug.Log("Mole was hit");

        if (popRoutine != null)
            StopCoroutine(popRoutine);

        popRoutine = StartCoroutine(PopDownAndUp());
    }

    private IEnumerator PopDownAndUp()
    {
        Vector3 targetPos = initialPosition + Vector3.down * popDownDistance;
        float elapsed = 0f;

        while (elapsed < popTime)
        {
            transform.position = Vector3.Lerp(
                initialPosition,
                targetPos,
                elapsed / popTime
            );
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPos;

        elapsed = 0f;
        while (elapsed < popTime)
        {
            transform.position = Vector3.Lerp(
                targetPos,
                initialPosition,
                elapsed / popTime
            );
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = initialPosition;

        popRoutine = null;
    }
}
