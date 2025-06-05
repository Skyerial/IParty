// DeathZone.cs
// Disables or reloads the player when they enter a death zone trigger.

using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathZone : MonoBehaviour
{
    [Tooltip("Optional: tag of the player GameObject. If left empty, player will be disabled on death.")]
    public string playerTag = "Player";

    [Tooltip("If true, reload the current scene when the player dies.")]
    public bool reloadSceneOnDeath = true;

    private void OnTriggerEnter(Collider other)
    {
        // Only respond if the collider matches the player tag
        if (!other.CompareTag(playerTag))
            return;

        // Disable the player's movement component to stop input
        PlayerMovement movement = other.GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.enabled = false;
        }

        // (Optional) Play a death animation, VFX, or sound here

        if (reloadSceneOnDeath)
        {
            // Reload the active scene immediately
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            // Disable the player GameObject instead of reloading
            other.gameObject.SetActive(false);
        }
    }
}
