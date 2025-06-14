using UnityEngine;
using System.Collections;

public class Mole : MonoBehaviour
{
    [Tooltip("How far the mole dips when hiding")]
    public float popDownDistance = 2f;
    [Tooltip("Seconds it takes to move down/up")]
    public float moveSpeed = 5f;
    [Tooltip("How long the mole stays visible after popping up")]
    public float stayUpTime = 0.6f;

    public GameObject hitEffect;
    public Transform effectSpawnPoint;


    private Vector3 upPosition;
    private Vector3 downPosition;
    private Coroutine currentRoutine;

    void Awake()
    {
        downPosition = transform.position;
        upPosition = downPosition + Vector3.up * Mathf.Abs(popDownDistance);
        transform.position = downPosition;
    }

    public IEnumerator PopCycle()
    {
        yield return StartCoroutine(MoveTo(upPosition));
        yield return new WaitForSeconds(stayUpTime);
        yield return StartCoroutine(MoveTo(downPosition));
    }



    public void OnHit()
    {
        Debug.Log("🐹 Mole was hit!");

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        // 🔥 Spawn hit effect (e.g., WHAM!) at the defined spawn point
        if (hitEffect && effectSpawnPoint)
        {
            GameObject fx = Instantiate(hitEffect, effectSpawnPoint.position, Quaternion.identity);
            // Destroy(fx, 2f); // Optional: destroy after 2 seconds
        }

        // 🕳 Immediately hide the mole
        currentRoutine = StartCoroutine(MoveTo(downPosition));
    }

    private IEnumerator MoveTo(Vector3 target)
    {
        float timeout = 0.5f; // Max time to try moving
        float elapsed = 0f;

        while (Vector3.Distance(transform.position, target) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            elapsed += Time.deltaTime;

            if (elapsed > timeout)
            {
                Debug.LogWarning($"{gameObject.name} movement timed out — aborting and resetting.");
                break;
            }

            yield return null;
        }

        transform.position = target;
    }

}
