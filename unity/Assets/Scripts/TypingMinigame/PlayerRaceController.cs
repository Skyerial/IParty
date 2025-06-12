using UnityEngine;
using System.Collections;

public class PlayerRaceController : MonoBehaviour
{
    public Transform finishLine;           // Set in Inspector
    public int totalWords = 10;            // Set this based on your game logic
    private Vector3 startPosition;
    private float stepDistance;
    private int wordsTyped = 0;

    private void Start()
    {
        startPosition = transform.position;

        // Total distance from start to finish
        float totalDistance = Vector3.Distance(startPosition, finishLine.position);

        // How far to move per word typed
        stepDistance = totalDistance / totalWords;
    }

    public void OnWordTyped()
    {
        if (wordsTyped >= totalWords)
            return;

        wordsTyped++;

        // Calculate new target position
        Vector3 direction = (finishLine.position - startPosition).normalized;
        Vector3 targetPosition = startPosition + direction * (stepDistance * wordsTyped);

        // Move smoothly to the new position
        StartCoroutine(MovePlayerSmoothly(targetPosition));
    }

    private IEnumerator MovePlayerSmoothly(Vector3 targetPos)
    {
        float t = 0f;
        float duration = 0.4f; // Animation speed
        Vector3 initialPos = transform.position;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            transform.position = Vector3.Lerp(initialPos, targetPos, t);
            yield return null;
        }

        transform.position = targetPos;
    }
}
