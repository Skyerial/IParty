using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(PlayerInput))]
public class PlayerMash : MonoBehaviour
{
    public Animator animator;

    private int mashCounter = 0;
    private float baseSpeed = 1f;
    private float speedIncrement = 0.25f;
    private int pressesPerSpeedUp = 5;
    private bool isSquatting = false;

    private PlayerInput playerInput;
    private InputAction mashAction;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        mashAction = playerInput.actions.FindAction("Mash");

        if (mashAction == null)
        {
            Debug.LogError("Error");
        }
    }

    void OnEnable()
    {
        if (mashAction != null)
            mashAction.performed += OnMashPerformed;
    }

    void OnDisable()
    {
        if (mashAction != null)
            mashAction.performed -= OnMashPerformed;
    }

    private void OnMashPerformed(InputAction.CallbackContext context)
    {
        mashCounter++;

        animator.speed = baseSpeed + (mashCounter / pressesPerSpeedUp) * speedIncrement;

        if (!isSquatting)
        {
            animator.Play("Squat");
            isSquatting = true;
        }

        Debug.Log($"Mash count: {mashCounter}, Animator Speed: {animator.speed}");
    }

    public void StartNewRound()
    {
        mashCounter = 0;
        animator.speed = baseSpeed;
        animator.ResetTrigger("Float");
        isSquatting = false;
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
        animator.speed = 1f;
        animator.Play("Squat");
        yield return new WaitForSeconds(0.7f);

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
