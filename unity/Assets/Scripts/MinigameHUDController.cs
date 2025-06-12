using UnityEngine;
using TMPro;
using System.Collections;

public class MinigameHUDController : MonoBehaviour
{
    [Header("Countdown Settings")]
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private GameObject countDownFrame;
    [SerializeField] private float countdownDuration = 4f; // 3..2..1..GO!

    [Header("Gameplay Timer Settings")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject timerFrame;
    [SerializeField] private float totalGameTime = 10f;

    [Header("Gameplay Activation")]
    [SerializeField] private GameObject gameplayObjects;

    private float countdownTimer;
    private float gameTimer;
    private bool gameStarted = false;

    void Start()
    {
        countdownTimer = countdownDuration;
        gameTimer = totalGameTime;

        // Initial state
        timerFrame.SetActive(false);
        gameplayObjects.SetActive(false);
        countDownFrame.SetActive(true);
    }

    void Update()
    {
        if (!gameStarted)
        {
            HandleCountdown();
        }
        else
        {
            HandleGameTimer();
        }
    }

    private void HandleCountdown()
    {
        countdownTimer -= Time.deltaTime;
        int display = Mathf.CeilToInt(countdownTimer);

        if (countdownTimer > 1f)
        {
            countdownText.text = display.ToString();
        }
        else if (countdownTimer > 0f)
        {
            countdownText.text = "GO!";
        }
        else
        {
            // Transition
            countDownFrame.SetActive(false);
            timerFrame.SetActive(true);
            gameplayObjects.SetActive(true);
            gameStarted = true;
        }
    }

    private void HandleGameTimer()
    {
        gameTimer -= Time.deltaTime;
        gameTimer = Mathf.Max(0, gameTimer);

        if (timerText != null)
        {
            timerText.text = gameTimer.ToString("F1");

            float percentElapsed = 1f - (gameTimer / totalGameTime);
            if (percentElapsed >= 0.9f)
                timerText.color = Color.red;
            else if (percentElapsed >= 0.7f)
                timerText.color = new Color(1f, 0.65f, 0f); // orange
            else
                timerText.color = Color.white;
        }

        if (gameTimer <= 0)
        {
            // TODO: End-game logic here
            Debug.Log("Time's up!");
        }
    }
}
