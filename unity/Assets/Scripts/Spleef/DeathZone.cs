// DeathZone.cs (Simplified)
// Handles player elimination: disables the player,
// spawns a death marker, plays a death sound,
// and stops processing once one player remains.

using UnityEngine;
using System.Collections.Generic;

public class DeathZone : MonoBehaviour
{
    [Tooltip("Tag of the player GameObject.")]
    public string playerTag = "Player";

    [Header("Death Marker Prefab")]
    [Tooltip("Assign the red-X prefab. It will be spawned at each death location, offset upward.")]
    public GameObject deathMarkerPrefab;

    [Tooltip("How many seconds the death marker should remain before auto-destroying.")]
    public float deathMarkerDuration = 5f;

    [Tooltip("Vertical offset (in world units) to raise the death marker above the player's position.")]
    public float deathMarkerYOffset = 1f;

    [Header("Sound")]
    [Tooltip("AudioSource used to play the death sound.")]
    public AudioSource audioSource;

    [Tooltip("Clip to play when a player dies.")]
    public AudioClip deathSound;

    // Prevents multiple end-game sequences from running
    private bool gameEnded = false;

    private void OnTriggerEnter(Collider other)
    {
        // Only handle if the collider is tagged as a player, and if the game hasn't already ended
        if (!other.CompareTag(playerTag) || gameEnded)
            return;

        // Disable that player's movement script
        PlayerMovementSpleef movement = other.GetComponent<PlayerMovementSpleef>();
        if (movement != null)
            movement.enabled = false;

        // Spawn the red-X marker at the player's death position
        if (deathMarkerPrefab != null)
        {
            Vector3 deathPos = other.transform.position + Vector3.up * deathMarkerYOffset;
            GameObject marker = Instantiate(deathMarkerPrefab, deathPos, Quaternion.identity);
            Destroy(marker, deathMarkerDuration);
        }

        // Play the death sound
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        // Deactivate the player GameObject
        other.gameObject.SetActive(false);

        // Count how many players remain active
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag(playerTag);
        int activeCount = 0;
        foreach (GameObject go in allPlayers)
        {
            if (go.activeInHierarchy)
                activeCount++;
        }

        // When one (or zero) remains, mark game ended
        if (activeCount <= 0)
        {
            gameEnded = true;
        }
    }
}
