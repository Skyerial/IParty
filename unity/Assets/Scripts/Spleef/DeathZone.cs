// DeathZone.cs
// Disables or reloads the player when they enter a death zone trigger.
// Tracks elimination order, spawns a red-X marker at the death location (offset upward),
// plays a death sound on each individual death, and when only one player remains,
// does slow-motion + Finish UI (with its own sound), then clears all death markers before showing Finish UI,
// and finally changes or reloads the scene.

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class DeathZone : MonoBehaviour
{
    [Tooltip("Tag of the player GameObject.")]
    public string playerTag = "Player";

    [Tooltip("Optional: name of the scene to load once only one player remains. If blank, reloads current.")]
    public string nextSceneName = "";

    [Header("Slow-Motion & Finish UI")]
    [Tooltip("Drag in a UI GameObject (e.g. a centered Text) that displays “Finish.” It should start disabled.")]
    public GameObject finishUI;

    [Tooltip("When one player remains, slow the game to this timescale.")]
    [Range(0.01f, 1f)]
    public float slowMotionScale = 0.2f;

    [Tooltip("Total real-time seconds to stay in slow-motion before switching scenes.")]
    public float slowMotionDuration = 2f;

    [Tooltip("How many real-time seconds to wait (during slow-motion) before showing Finish UI.")]
    public float finishUIDelay = 1f;

    [Tooltip("Clip to play when the Finish UI appears.")]
    public AudioClip finishSound;

    [Header("Death Marker Prefab")]
    [Tooltip("Assign the red-X prefab. It will be spawned at each death location, offset upward.")]
    public GameObject deathMarkerPrefab;

    [Tooltip("How many seconds the death marker should remain before auto-destroying.")]
    public float deathMarkerDuration = 5f;

    [Tooltip("Vertical offset (in world units) to raise the death marker above the player's position.")]
    public float deathMarkerYOffset = 1f;

    [Header("Sound")]
    [Tooltip("AudioSource used to play the death sound and finish sound.")]
    public AudioSource audioSource;

    [Tooltip("Clip to play when a player dies.")]
    public AudioClip deathSound;

    // Prevents multiple end-game sequences from running
    private bool gameEnded = false;

    // Keep track of all instantiated death markers so we can clear them later
    private List<GameObject> deathMarkers = new List<GameObject>();

    private void Start()
    {
        // Clear any old elimination data
        EliminationManager.eliminationOrder.Clear();

        // Hide Finish UI at start
        if (finishUI != null)
            finishUI.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only handle if the collider is tagged as a player, and if the game hasn't already ended
        if (!other.CompareTag(playerTag) || gameEnded)
            return;

        // Disable that player's movement script so they stop moving
        PlayerMovementSpleef movement = other.GetComponent<PlayerMovementSpleef>();
        if (movement != null)
            movement.enabled = false;

        // Record this player's name as eliminated
        EliminationManager.eliminationOrder.Add(other.name);

        // Spawn the red-X marker at the player's death position with an upward offset,
        // then destroy it after a set duration
        if (deathMarkerPrefab != null)
        {
            Vector3 deathPos = other.transform.position + Vector3.up * deathMarkerYOffset;
            GameObject marker = Instantiate(deathMarkerPrefab, deathPos, Quaternion.identity);
            deathMarkers.Add(marker);
            Destroy(marker, deathMarkerDuration);
        }

        // Play the death sound (if assigned)
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        // Deactivate the player GameObject
        other.gameObject.SetActive(false);

        // Count how many players remain active
        GameObject[] all = GameObject.FindGameObjectsWithTag(playerTag);
        int activeCount = 0;
        GameObject lastAlive = null;
        foreach (GameObject go in all)
        {
            if (go.activeInHierarchy)
            {
                activeCount++;
                lastAlive = go;
            }
        }

        // If only one (or zero) players remain, trigger end-game sequence
        if (activeCount <= 1)
        {
            if (lastAlive != null)
            {
                EliminationManager.eliminationOrder.Add(lastAlive.name);
            }

            // Reverse so index 0 is winner
            EliminationManager.eliminationOrder.Reverse();

            StartCoroutine(EndGameCoroutine());
            gameEnded = true;
        }
    }

    private IEnumerator EndGameCoroutine()
    {
        // Immediately enter slow-motion
        Time.timeScale = slowMotionScale;
        Time.fixedDeltaTime = 0.02f * slowMotionScale;

        // Wait finishUIDelay real-seconds, then clear death markers and show Finish UI
        float delay = Mathf.Min(finishUIDelay, slowMotionDuration);
        yield return new WaitForSecondsRealtime(delay);

        // Destroy any remaining death markers immediately
        foreach (GameObject marker in deathMarkers)
        {
            if (marker != null)
                Destroy(marker);
        }
        deathMarkers.Clear();

        // Now show Finish UI
        if (finishUI != null)
        {
            finishUI.SetActive(true);

            // Play the Finish UI sound at the moment it appears
            if (audioSource != null && finishSound != null)
            {
                audioSource.PlayOneShot(finishSound);
            }
        }

        // Wait the remainder of slowMotionDuration (real-seconds)
        float remaining = slowMotionDuration - delay;
        if (remaining > 0f)
            yield return new WaitForSecondsRealtime(remaining);

        // Restore normal time
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        // Load next scene (or reload current)
        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
