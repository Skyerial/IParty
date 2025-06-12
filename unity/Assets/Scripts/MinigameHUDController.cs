using UnityEngine;
using TMPro;

public class MinigameHUDController : MonoBehaviour
{
    [Header("Countdown Settings")]
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private GameObject countDownFrame;
    [SerializeField] private int countdownDuration = 3; // Display 3, 2, 1

    [Header("Gameplay Timer Settings")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject timerFrame;
    [SerializeField] private int totalGameTime = 10;

    [Header("Gameplay Activation")]
    [SerializeField] private GameObject gameplayObjects;

    private float countdownTimer;
    private float gameTimer;
    private bool gameStarted = false;
    private bool gameEnded = false;

    void Start()
    {
        // Start slightly below full value so the first number switches fast (fixes long "3" issue)
        countdownTimer = countdownDuration + 0.5f;
        gameTimer = totalGameTime;

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
        else if (!gameEnded)
        {
            HandleGameTimer();
        }
    }

    private void HandleCountdown()
    {
        countdownTimer -= Time.deltaTime;

        if (countdownTimer > 1f)
        {
            int display = Mathf.FloorToInt(countdownTimer);
            countdownText.text = display.ToString();
        }
        else if (countdownTimer > 0f)
        {
            countdownText.text = "GO!";
        }
        else
        {
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
            int secondsLeft = Mathf.FloorToInt(gameTimer);
            timerText.text = secondsLeft.ToString();

            float percentElapsed = 1f - (gameTimer / totalGameTime);
            if (percentElapsed >= 0.9f)
                timerText.color = Color.red;
            else if (percentElapsed >= 0.7f)
                timerText.color = Color.orange;
            else
                timerText.color = Color.white;
        }

        if (gameTimer <= 0 && !gameEnded)
        {
            gameEnded = true;
            timerFrame.SetActive(false);
            countDownFrame.SetActive(true);
            countdownText.text = "Game Set!";
            Debug.Log("Game Set!");
        }
    }
}
