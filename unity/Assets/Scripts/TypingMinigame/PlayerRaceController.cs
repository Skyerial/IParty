using UnityEngine;
using System.Collections;

public class PlayerRaceController : MonoBehaviour
{
    public Transform finishLine;           // Set in Inspector
    public int totalWords = 10;            // Set this based on your game logic
    private Vector3 startPosition;
    private float stepDistance;
    private int wordsTyped = 0;
    private Animator animator;

    // private void Start()
    // {
    //     animator = GetComponent<Animator>();

    //     startPosition = transform.position;

    //     // Total distance from start to finish
    //     float totalDistance = Vector3.Distance(startPosition, finishLine.position);

    //     // How far to move per word typed
    //     stepDistance = totalDistance / totalWords;
    // }

    public void InitializeRace(int totalWordsCount)
    {
        animator = GetComponent<Animator>();
        totalWords = totalWordsCount;

        startPosition = transform.position;

        float totalDistance = Vector3.Distance(startPosition, finishLine.position);
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
        StartCoroutine(MovePlayer(targetPosition));
        // StartCoroutine(MovePlayerSmoothly(targetPosition));
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

    private IEnumerator MovePlayer(Vector3 targetPos)
    {
        Debug.Log($"raceController gameobject {gameObject}");
        Debug.Log($"raceController animator {animator}");
        animator.SetBool("IsRunning", true);
        gameObject.LeanMoveX(targetPos.x, 0.4f);

        while (LeanTween.isTweening(gameObject))
        {
            yield return null;
        }

        animator.SetBool("IsRunning", false);
    }

    public void WinningAnim()
    {
        // while (!gameEnd)
        // {
        Debug.Log("chekc");
        animator.SetTrigger("Jump");
        // StartCoroutine(WaitForAnimation(animator, "Jump"));
        // }
    }
    
    private IEnumerator WaitForAnimation(Animator animator, string stateName)
    {
        // Wait until the Animator is in the desired state
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
            yield return null;

        // Now wait for the animation to finish
        float length = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(length);
    }
}
