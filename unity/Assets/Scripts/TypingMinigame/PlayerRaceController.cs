using UnityEngine;
using System.Collections;


/**
* @brief Attached to player to move the player from start to finish
*/
public class PlayerRaceController : MonoBehaviour
{
    public Transform finishLine;           // Set in Inspector
    public int totalWords = 10;            // Set this based on your game logic
    private Vector3 startPosition;
    private float stepDistance;
    private int wordsTyped = 0;
    private Animator animator;

    /**
    * @brief Calculate step size based on the amount of words that need to be typed
    * @param[IN] totalWordsCount The amount of words that need to be typed
    */
    public void InitializeRace(int totalWordsCount)
    {
        animator = GetComponent<Animator>();
        totalWords = totalWordsCount;

        startPosition = transform.position;

        float totalDistance = Vector3.Distance(startPosition, finishLine.position);
        stepDistance = totalDistance / totalWords;
    }

    /**
    * @brief Call when a word has been typed correctly to move the player stepDistance ahead
    */
    public void OnWordTyped()
    {
        if (wordsTyped >= totalWords)
            return;

        wordsTyped++;

        // Calculate new target position
        Vector3 direction = (finishLine.position - startPosition).normalized;
        Vector3 targetPosition = startPosition + direction * (stepDistance * wordsTyped);

        StartCoroutine(MovePlayer(targetPosition));
    }

    /**
    * @brief Set the player to a winning animation
    */
    public void WinningAnim()
    {
        animator.SetTrigger("Jump");
    }

    /**
    * @brief Move the player to the target position and set the running animation
    * @param[IN] targetPos The position the player needs to travel to
    */
    private IEnumerator MovePlayer(Vector3 targetPos)
    {
        animator.SetBool("IsRunning", true);
        gameObject.LeanMoveX(targetPos.x, 0.4f);

        while (LeanTween.isTweening(gameObject))
        {
            yield return null;
        }

        animator.SetBool("IsRunning", false);
    }
}
