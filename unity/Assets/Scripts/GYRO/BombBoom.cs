using UnityEngine;
using System.Collections;

/**
 * @brief Controls the behavior of a bomb enemy in the minigame, including popping up/down and handling hits.
 */
public class BombGyro : MonoBehaviour
{
    [Tooltip("How far the bomb dips when hiding")]
    /**
     * @brief Distance the bomb moves downward when hiding.
     */
    public float popDownDistance = 2f;

    [Tooltip("Seconds it takes to move down/up")]
    /**
     * @brief Speed at which the bomb moves between up and down positions.
     */
    public float moveSpeed = 5f;

    [Tooltip("How long the bomb stays visible after popping up")]
    /**
     * @brief Duration (in seconds) the bomb stays visible after rising.
     */
    public float stayUpTime = 0.6f;

    /**
     * @brief Particle effect played when the bomb explodes.
     */
    public GameObject explosionEffect;

    /**
     * @brief Text effect displayed when the bomb explodes (e.g. "Boom!").
     */
    public GameObject boomTextEffect;

    /**
     * @brief Position where visual effects are instantiated.
     */
    public Transform effectSpawnPoint;

    private Vector3 upPosition;
    private Vector3 downPosition;
    private Coroutine currentRoutine;

    /**
     * @brief Initializes bomb position values when the object awakens.
     */
    void Awake()
    {
        downPosition = transform.position;
        upPosition = downPosition + Vector3.up * Mathf.Abs(popDownDistance);
        transform.position = downPosition;
    }

    /**
     * @brief Coroutine that handles the bomb popping up, staying up, and returning down.
     * @return IEnumerator for coroutine execution.
     */
    public IEnumerator PopCycle()
    {
        yield return StartCoroutine(MoveTo(upPosition));
        yield return new WaitForSeconds(stayUpTime);
        yield return StartCoroutine(MoveTo(downPosition));
    }

    /**
     * @brief Called when the bomb is hit by a player; triggers effects and disables the bomb.
     */
    public void OnHit()
    {
        Debug.Log("Bomb hit!");

        var scoreDisplay = transform.root.GetComponentInChildren<ScoreDisplay>();
        if (scoreDisplay != null)
            scoreDisplay.RemoveMoleHit();

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        if (explosionEffect)
            Instantiate(explosionEffect, effectSpawnPoint.position, Quaternion.identity);

        if (boomTextEffect)
            Instantiate(boomTextEffect, effectSpawnPoint.position, Quaternion.identity);

        currentRoutine = StartCoroutine(MoveTo(downPosition));

        ScoreDisplay score = transform.root.GetComponentInChildren<ScoreDisplay>();
        if (score != null)
        {
            score.RemoveMoleHit();
        }
    }

    /**
     * @brief Coroutine to smoothly move the bomb to a target position with a timeout safeguard.
     * @param target The position to move the bomb to.
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

            if (elapsed > timeout)
            {
                Debug.LogWarning($"{gameObject.name} movement timed out aborting and resetting.");
                break;
            }

            yield return null;
        }

        transform.position = target;
    }
}
