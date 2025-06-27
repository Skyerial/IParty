using UnityEngine;
using System.Collections;

/**
 * @brief Controls the behavior of the OilBarrel enemy in the Whack-a-Mole minigame. Handles movement, effects, and screen splat feedback.
 */
public class OilBarrel : MonoBehaviour
{
    [Header("Movement")]
    /**
     * @brief Distance the barrel moves downward when hiding.
     */
    public float popDownDistance = 2f;

    /**
     * @brief Speed at which the barrel moves up and down.
     */
    public float moveSpeed = 5f;

    /**
     * @brief Duration the barrel stays visible before hiding again.
     */
    public float stayUpTime = 0.6f;

    [Header("Effects")]
    /**
     * @brief Visual splash effect spawned on hit.
     */
    public GameObject splashFX;

    /**
     * @brief Text effect spawned on hit.
     */
    public GameObject splashTextEffect;

    /**
     * @brief Position where effects are spawned.
     */
    public Transform effectSpawnPoint;

    [Header("Screen Effect")]
    /**
     * @brief Reference to the oil splat screen effect controller.
     */
    public OilSplatEffect oilSplatEffect;

    /**
     * @brief Delay before triggering the screen splat effect.
     */
    public float splatdelay = 0.6f;

    /**
     * @brief Target position when popped up.
     */
    private Vector3 upPosition;

    /**
     * @brief Target position when hidden.
     */
    private Vector3 downPosition;

    /**
     * @brief Currently active movement coroutine, if any.
     */
    private Coroutine currentRoutine;

    /**
     * @brief Initializes the starting positions of the barrel.
     */
    void Awake()
    {
        downPosition = transform.position;
        upPosition = downPosition + Vector3.up * Mathf.Abs(popDownDistance);
        transform.position = downPosition;
    }

    /**
     * @brief Coroutine handling full pop-up and return cycle of the barrel.
     * @return IEnumerator for coroutine execution.
     */
    public IEnumerator PopCycle()
    {
        yield return StartCoroutine(MoveTo(upPosition));
        yield return new WaitForSeconds(stayUpTime);
        yield return StartCoroutine(MoveTo(downPosition));
    }

    /**
     * @brief Called when the oil barrel is hit. Spawns effects and triggers splat animation.
     */
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

    /**
     * @brief Coroutine to delay the screen splat effect after being hit.
     * @return IEnumerator for coroutine execution.
     */
    private IEnumerator DelayedOilSplat()
    {
        yield return new WaitForSeconds(splatdelay);

        if (oilSplatEffect != null)
            oilSplatEffect.ShowSplat();
    }

    /**
     * @brief Moves the barrel toward a target position smoothly.
     * @param target The target position to move to.
     * @return IEnumerator for coroutine execution.
     */
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
