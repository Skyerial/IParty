using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerMash : MonoBehaviour
{
    [SerializeField]
    private Animator animator;

    [SerializeField]
    private AudioSource squatSFX;

    private PlayerInput playerInput;
    private InputAction mashAction;

    private int mashCounter = 0;
    private float baseSpeed = 1f;
    private float speedIncrement = 0.25f;
    private int pressesPerSpeedUp = 5;
    private bool isSquatting = false;

    public bool IsFloatDone { get; private set; } = false;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        mashAction = playerInput.actions.FindAction("Mash");

        if (mashAction == null)
        {
            Debug.LogError("No input action named 'Mash' found!");
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
        if (!SquatManager.inputEnabled)
            return;

        mashCounter++;
        animator.speed = baseSpeed + (mashCounter / pressesPerSpeedUp) * speedIncrement;

        if (!isSquatting)
        {
            animator.Play("Squat");
            isSquatting = true;
        }
    }

    public void StartNewRound()
    {
        mashCounter = 0;
        animator.speed = baseSpeed;
        animator.ResetTrigger("Float");
        isSquatting = false;
    }

    public int GetMashCounter() => mashCounter;

    public void TriggerFloatAnimation(float height)
    {
        StartCoroutine(DoFinalSquatAndFloat(height));
    }

    private IEnumerator DoFinalSquatAndFloat(float height)
    {
        IsFloatDone = false;
        animator.speed = 1f;
        animator.Play("Squat");
        yield return new WaitForSeconds(0.7f);

        animator.SetTrigger("Float");

        float groundY = 19.4f;
        float targetY = groundY + height;
        float duration = 2f;
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
        IsFloatDone = true;
    }
}
