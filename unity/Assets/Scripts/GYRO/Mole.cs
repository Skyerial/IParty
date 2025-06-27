using UnityEngine;
using System.Collections;
using UnityEngine.SocialPlatforms.Impl;

/**
 * @brief Controls the behavior of the Mole in the Whack-a-Mole minigame, including popping up, hiding, and responding to hits.
 */
public class Mole : MonoBehaviour
{
    /**
     * @brief How far the mole dips below its original position when hiding.
     */
    [Tooltip("How far the mole dips when hiding")]
    public float popDownDistance = 2f;

    /**
     * @brief The speed at which the mole moves up and down.
     */
    [Tooltip("Seconds it takes to move down/up")]
    public float moveSpeed = 5f;

    /**
     * @brief Duration the mole stays visible above ground.
     */
    [Tooltip("How long the mole stays visible after popping up")]
    public float stayUpTime = 0.6f;

    /**
     * @brief Visual effect spawned when the mole is hit.
     */
    public GameObject hitEffect;

    /**
     * @brief Location where the hit effect will be spawned.
     */
    public Transform effectSpawnPoint;

    /**
     * @brief Reference to the player camera for positioning hit effects.
     */
    public Camera playerCamera;

    /**
     * @brief Sound played when the mole is hit.
     */
    public AudioClip hitSound;

    /**
     * @brief AudioSource used to play the hit sound.
     */
    private AudioSource audioSource;

    /**
     * @brief Cached position when the mole is popped up.
     */
    private Vector3 upPosition;

    /**
     * @brief Cached position when the mole is hidden.
     */
    private Vector3 downPosition;

    /**
     * @brief Coroutine currently running for the mole's movement.
     */
    private Coroutine currentRoutine;

    /**
     * @brief Initializes positions and audio source at startup.
     */
    void Awake()
    {
        downPosition = transform.position;
        upPosition = downPosition + Vector3.up * Mathf.Abs(popDownDistance);
        transform.position = downPosition;
        audioSource = GetComponent<AudioSource>();
    }

    /**
     * @brief Handles a full pop-up cycle: move up, wait, then move down.
     * @return IEnumerator for coroutine handling.
     */
    public IEnumerator PopCycle()
    {
        yield return StartCoroutine(MoveTo(upPosition));
        yield return new WaitForSeconds(stayUpTime);
        yield return StartCoroutine(MoveTo(downPosition));
    }

    /**
     * @brief Called when the mole is hit by a player; plays sound, effect, and updates score.
     */
    public void OnHit()
    {
        Debug.Log("Mole was hit!");

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        if (hitEffect && playerCamera)
        {
            Vector3 centerPos = playerCamera.transform.position + playerCamera.transform.forward * 5f;
            GameObject fx = Instantiate(hitEffect, centerPos, Quaternion.identity);
        }

        if (audioSource && hitSound)
            audioSource.PlayOneShot(hitSound);

        currentRoutine = StartCoroutine(MoveTo(downPosition));

        ScoreDisplay score = transform.root.GetComponentInChildren<ScoreDisplay>();
        if (score != null)
        {
            score.AddMoleHit();
        }
    }

    /**
     * @brief Moves the mole smoothly to the given target position.
     * @param target The position to move the mole toward.
     * @return IEnumerator for coroutine handling.
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
