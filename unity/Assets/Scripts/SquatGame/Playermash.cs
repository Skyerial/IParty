using UnityEngine;
using System.Collections;


public class PlayerMash : MonoBehaviour
{
    public KeyCode mashKey = KeyCode.Space;
    public Animator animator;
    private int mashCounter = 0;

    public void StartNewRound()
    {
        mashCounter = 0;
        animator.ResetTrigger("Squat");
    }

    void Update()
    {
        if (Input.GetKeyDown(mashKey))
        {
            Debug.Log("Mash key pressed");
            animator.SetTrigger("Squat");
            mashCounter++;
            Debug.Log("Counter: " + mashCounter);
        }
    }

    // Get the current count of button presses
    public int GetMashCounter()
    {
        return mashCounter;
    }

    // Trigger the float animation when the game ends
    public void TriggerFloatAnimation(float height)
    {
        animator.SetTrigger("Float");

        float groundY = 19.4f;
        float targetY = groundY + height;

        StopAllCoroutines();
        StartCoroutine(FloatUpward(targetY, 2f));
    }

    private IEnumerator FloatUpward(float targetY, float duration)
    {
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(startPos.x, targetY, startPos.z);

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;
    }

}
