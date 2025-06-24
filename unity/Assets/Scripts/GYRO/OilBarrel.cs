using UnityEngine;
using System.Collections;

public class OilBarrel : MonoBehaviour
{
    [Header("Movement")]
    public float popDownDistance = 2f;
    public float moveSpeed = 5f;
    public float stayUpTime = 0.6f;

    [Header("Effects")]
    public GameObject splashFX;
    public GameObject splashTextEffect;
    public Transform effectSpawnPoint;

    [Header("Screen Effect")]
    public OilSplatEffect oilSplatEffect;

    public float splatdelay = 0.6f;

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
        Debug.Log("Oil barrel hit!");

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        if (splashFX && effectSpawnPoint)
            Instantiate(splashFX, effectSpawnPoint.position, Quaternion.identity);

        if (splashTextEffect && effectSpawnPoint)
            Instantiate(splashTextEffect, effectSpawnPoint.position, Quaternion.identity);


        StartCoroutine(DelayedOilSplat());


        currentRoutine = StartCoroutine(MoveTo(downPosition));
    }


    private IEnumerator DelayedOilSplat()
    {
        yield return new WaitForSeconds(splatdelay);

        if (oilSplatEffect != null)
            oilSplatEffect.ShowSplat();
    }


    private IEnumerator MoveTo(Vector3 target)
    {
        float timeout = 0.5f;
        float elapsed = 0f;

        while (Vector3.Distance(transform.position, target) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            elapsed += Time.deltaTime;
            if (elapsed > timeout) break;
            yield return null;
        }

        transform.position = target;
    }
}
