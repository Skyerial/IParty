using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem; // <-- 1. ADD THIS LINE

public class PlayerHammer : MonoBehaviour
{
    [Tooltip("Set this to 1, 2, 3, or 4 for each player instance.")]
    public int playerID;

    [Tooltip("Drag the GameManager object from the scene here.")]
    public GameManager gameManager;

    void Update()
    {
        if (playerID == 1 && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            TriggerSmack();
        }
    }

    public void TriggerSmack()
    {
        if (gameManager == null) return;

        Debug.Log($"Player {playerID} triggered a smack!");
        gameManager.PlayerAttemptedHit(playerID);
        StartCoroutine(SmackAnimation());
    }

    IEnumerator SmackAnimation()
    {
        Vector3 originalPosition = transform.position;
        Vector3 targetPosition = Vector3.zero;

        float duration = 0.05f;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            transform.position = Vector3.Lerp(originalPosition, targetPosition, t / duration);
            yield return null;
        }

        duration = 0.2f;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            transform.position = Vector3.Lerp(targetPosition, originalPosition, t / duration);
            yield return null;
        }

        transform.position = originalPosition;
    }
}