// HexTile.cs
// Causes a hex tile to fall when stepped on, then respawn and grow back.

using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class HexTile : MonoBehaviour
{
    [Header("Falling & Respawn Settings")]
    [Tooltip("Seconds to wait after the player steps on before the tile falls.")]
    public float fallDelay = 0.8f;

    [Tooltip("Seconds to wait after the tile has started falling before it begins respawning.")]
    public float respawnDelay = 3f;

    [Tooltip("Time (in seconds) it takes for the tile to grow from zero scale to full scale.")]
    public float growDuration = 1f;

    // Rigidbody and Collider for physics control
    private Rigidbody rb;
    private Collider col;

    // Original transform state for respawn
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;
    private Transform originalParent;

    // Prevent multiple fall routines from stacking
    private bool isScheduledToFall = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        // Record the spawned transform exactly as instantiated
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalScale    = transform.localScale;
        originalParent   = transform.parent;

        // Start as a static tile: no gravity, kinematic
        rb.isKinematic = true;
        rb.useGravity  = false;
    }

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

    private IEnumerator FallAndRespawnRoutine()
    {
        // Wait for fallDelay before dropping
        yield return new WaitForSeconds(fallDelay);

        // Enable physics so the tile falls
        rb.isKinematic = false;
        rb.useGravity = true;

        // Wait for respawnDelay to allow tile to fall offscreen
        yield return new WaitForSeconds(respawnDelay);

        // Reset transform and physics, then grow back
        ResetTransformAndPhysics();
        StartCoroutine(GrowToFullSize());
    }

    private void ResetTransformAndPhysics()
    {
        // Stop all physics motion
        rb.linearVelocity  = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic     = true;
        rb.useGravity      = false;

        // Restore original parent, position, and rotation
        transform.SetParent(originalParent, worldPositionStays: true);
        transform.position = originalPosition;
        transform.rotation = originalRotation;

        // Shrink to zero so it can grow back in
        transform.localScale = Vector3.zero;

        // Ensure collider is enabled during growth
        col.enabled = true;

        // Delay briefly before allowing another fall
        StartCoroutine(ClearFallFlagAfterDelay(0.1f));
    }

    private IEnumerator ClearFallFlagAfterDelay(float delay)
    {
        // Give a small buffer (e.g., one frame) before allowing new falls
        yield return new WaitForSeconds(delay);
        isScheduledToFall = false;
    }

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

        // Ensure final scale matches exactly
        transform.localScale = originalScale;
    }
}
