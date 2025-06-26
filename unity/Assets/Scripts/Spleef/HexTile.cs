// HexTile.cs
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
 /**
  * @brief Controls falling and respawning behavior of a hex tile when stepped on by a player.
  */
public class HexTile : MonoBehaviour
{
    [Header("Falling & Respawn Settings")]
     /**
      * @brief Seconds to wait after player contact before the tile falls.
      */
    [Tooltip("Seconds to wait after the player steps on before the tile falls.")]
    public float fallDelay = 0.8f;

     /**
      * @brief Seconds to wait after falling before beginning respawn.
      */
    [Tooltip("Seconds to wait after the tile has started falling before it begins respawning.")]
    public float respawnDelay = 3f;

     /**
      * @brief Duration (in seconds) for the tile to grow from zero to full scale on respawn.
      */
    [Tooltip("Time (in seconds) it takes for the tile to grow from zero scale to full scale.")]
    public float growDuration = 1f;

    private Rigidbody rb;
    private Collider col;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;
    private Transform originalParent;
    private bool isScheduledToFall = false;

     /**
      * @brief Unity event called when the script instance is loaded; caches components and original transform.
      */
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalScale = transform.localScale;
        originalParent = transform.parent;

        rb.isKinematic = true;
        rb.useGravity = false;
    }

     /**
      * @brief Detects player collision and schedules the fall-and-respawn routine.
      */
    private void OnCollisionEnter(Collision collision)
    {
        if (!isScheduledToFall
            && collision.collider.CompareTag("Player")
            && SpleefGameManager.Instance.TilesDroppingEnabled)
        {
            isScheduledToFall = true;
            StartCoroutine(FallAndRespawnRoutine());
        }
    }

     /**
      * @brief Continues to detect player presence and schedules fall if not already scheduled.
      */
    private void OnCollisionStay(Collision collision)
    {
        if (isScheduledToFall) return;

        if (collision.collider.CompareTag("Player")
        && SpleefGameManager.Instance.TilesDroppingEnabled)
        {
            isScheduledToFall = true;
            StartCoroutine(FallAndRespawnRoutine());
        }
    }

     /**
      * @brief Coroutine that handles waiting, dropping, resetting, and starting respawn growth.
      */
    private IEnumerator FallAndRespawnRoutine()
    {
        yield return new WaitForSeconds(fallDelay);

        rb.isKinematic = false;
        rb.useGravity = true;

        // Wait for respawnDelay to allow tile to fall offscreen
        yield return new WaitForSeconds(respawnDelay);

        ResetTransformAndPhysics();
        StartCoroutine(GrowToFullSize());
    }

     /**
      * @brief Resets transform and physics state, then starts the growth coroutine.
      */
    private void ResetTransformAndPhysics()
    {
        rb.linearVelocity  = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic     = true;
        rb.useGravity      = false;

        transform.SetParent(originalParent, worldPositionStays: true);
        transform.position = originalPosition;
        transform.rotation = originalRotation;

        transform.localScale = Vector3.zero;

        col.enabled = true;

        StartCoroutine(ClearFallFlagAfterDelay(0.1f));
    }

    private IEnumerator ClearFallFlagAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isScheduledToFall = false;
    }

     /**
      * @brief Coroutine that smoothly scales the tile from zero to its original size.
      */
    private IEnumerator GrowToFullSize()
    {
        float elapsed = 0f;

        while (elapsed < growDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / growDuration);
            transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, t);
            yield return null;
        }

        transform.localScale = originalScale;
    }
}
