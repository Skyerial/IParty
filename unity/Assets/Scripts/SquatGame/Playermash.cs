using UnityEngine;
using System.Collections;

public class PlayerMash : MonoBehaviour
{
    public KeyCode mashKey = KeyCode.Space;
    public Animator animator;
    private int mashCounter = 0;

    private float baseSpeed = 1f;
    private float speedIncrement = 0.25f;
    private int pressesPerSpeedUp = 5;

    private bool isSquatting = false;

    public void StartNewRound()
    {
        mashCounter = Random.Range(20, 60);
        animator.speed = baseSpeed;
        animator.ResetTrigger("Float");
        isSquatting = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(mashKey))
        {
            mashCounter++;

            // Increase animation speed based on mash count
            animator.speed = baseSpeed + (mashCounter / pressesPerSpeedUp) * speedIncrement;

            // Start the squat animation only once
            if (!isSquatting)
            {
                animator.Play("Squat");
                isSquatting = true;
            }

            Debug.Log($"Mash count: {mashCounter}, Animator Speed: {animator.speed}");
        }
    }

    public int GetMashCounter()
    {
        return mashCounter;
    }

    public void TriggerFloatAnimation(float height)
    {
        StartCoroutine(DoFinalSquatAndFloat(height));
    }

    private IEnumerator DoFinalSquatAndFloat(float height)
    {
        // Final squat at normal speed
        animator.speed = 1f;
        animator.Play("Squat");
        yield return new WaitForSeconds(0.7f); // let the final squat loop once

        // Float upward
        animator.SetTrigger("Float");

        float groundY = 19.4f;
        float targetY = groundY + height;

        float elapsed = 0f;
        float duration = 2f;
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
