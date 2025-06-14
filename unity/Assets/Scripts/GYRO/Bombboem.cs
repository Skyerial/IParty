using System.Collections;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    [Tooltip("How far the bomb dips when hiding")]
    public float popDownDistance = 2f;
    [Tooltip("Seconds it takes to move down/up")]
    public float moveSpeed = 5f;
    [Tooltip("How long the bomb stays visible after popping up")]
    public float stayUpTime = 0.6f;
    [Tooltip("Min time before bomb pops back up again")]
    public float minDelay = 0.5f;
    [Tooltip("Max time before bomb pops back up again")]
    public float maxDelay = 2.5f;

    private Vector3 upPosition;
    private Vector3 downPosition;
    private Coroutine activeRoutine;

    void Awake()
    {
        upPosition = transform.position + Vector3.up * Mathf.Abs(popDownDistance);
        downPosition = transform.position;
        transform.position = downPosition;
    }


    void Start()
    {
        StartPopLoop();
    }

    public void OnHit()
    {
        Debug.Log("💥 Bomb hit!");
        if (activeRoutine != null)
            StopCoroutine(activeRoutine);

        // Immediately hide and schedule next pop
        activeRoutine = StartCoroutine(HideThenWaitAndPop());
    }

    private void StartPopLoop()
    {
        float delay = Random.Range(minDelay, maxDelay);
        activeRoutine = StartCoroutine(WaitThenPop(delay));
    }

    private IEnumerator WaitThenPop(float delay)
    {
        yield return new WaitForSeconds(delay);
        yield return StartCoroutine(PopThenWaitAndHide());
    }


    private IEnumerator PopThenWaitAndHide()
    {
        yield return StartCoroutine(MoveTo(upPosition));
        yield return new WaitForSeconds(stayUpTime);
        yield return StartCoroutine(MoveTo(downPosition));
        yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));
        StartPopLoop();
    }

    private IEnumerator HideThenWaitAndPop()
    {
        yield return StartCoroutine(MoveTo(downPosition));
        yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));
        StartPopLoop();
    }

    private IEnumerator MoveTo(Vector3 target)
    {
        while (Vector3.Distance(transform.position, target) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = target;
    }
}
