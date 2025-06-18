using UnityEngine;
using TMPro;
using System.Collections;

public class MinigameHUDController : MonoBehaviour
{
    [Header("Activation Toggles")]
    [SerializeField] private bool useCountdown = true;
    [SerializeField] private bool useTimer = true;

    [Header("Countdown Settings")]
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private GameObject countDownFrame;
    [SerializeField] private int countdownDuration = 3;

    [Header("Gameplay Timer Settings")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject timerFrame;
    [SerializeField] private int totalGameTime = 10;

    [Header("Gameplay Activation")]
    [SerializeField] private GameObject gameplayObjects;

    [Header("Custom Countdown Labels")]
    [SerializeField] private string goText = "GO!";
    [SerializeField] private string gameSetText = "Game Set!";

    [Header("End Text Display")]
    [SerializeField] private float finishTextDuration = 2f;

    private float countdownTimer;
    private float gameTimer;

    public event System.Action OnCountdownFinished;
    public event System.Action OnGameTimerFinished;

    public void ShowCountdown()
    {
        if (useCountdown)
        {
            countdownTimer = countdownDuration + 0.5f;
            StartCoroutine(DoCountdown());
        }
        else
        {
            countDownFrame.SetActive(false);
            timerFrame.SetActive(true);
            gameplayObjects.SetActive(true);
            OnCountdownFinished?.Invoke();
        }
    }

    private IEnumerator DoCountdown()
    {
        countDownFrame.SetActive(true);
        timerFrame.SetActive(false);
        gameplayObjects.SetActive(false);

        while (countdownTimer > 0)
        {
            countdownTimer -= Time.deltaTime;

            if (countdownTimer > 1f)
                countdownText.text = Mathf.FloorToInt(countdownTimer).ToString();
            else if (countdownTimer > 0f)
                countdownText.text = goText;

            yield return null;
        }

        countDownFrame.SetActive(false);
        timerFrame.SetActive(true);
        gameplayObjects.SetActive(true);

        OnCountdownFinished?.Invoke();
    }

    public void StartGameTimer()
    {
        if (useTimer)
        {
            gameTimer = totalGameTime;
            StartCoroutine(GameTimerRoutine());
        }
        else
        {
            timerFrame.SetActive(false);
            OnGameTimerFinished?.Invoke();
        }
    }

    private IEnumerator GameTimerRoutine()
    {
        while (gameTimer > 0)
        {
            gameTimer -= Time.deltaTime;
            int secondsLeft = Mathf.FloorToInt(gameTimer);
            timerText.text = secondsLeft.ToString();

            float percentElapsed = 1f - (gameTimer / totalGameTime);
            if (percentElapsed >= 0.9f)
                timerText.color = Color.red;
            else if (percentElapsed >= 0.7f)
                timerText.color = new Color(1f, 0.65f, 0f); // orange
            else
                timerText.color = Color.white;

            yield return null;
        }

        timerFrame.SetActive(false);
        countDownFrame.SetActive(true);
        countdownText.text = gameSetText;
        StartCoroutine(HideFinishTextAfterDelay(finishTextDuration));

        OnGameTimerFinished?.Invoke();
    }

    private IEnumerator HideFinishTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        countDownFrame.SetActive(false);
    }

    public void ShowFinishText()
    {
        StopAllCoroutines(); // cancel any ongoing timer
        timerFrame.SetActive(false);
        countDownFrame.SetActive(true);
        countdownText.text = gameSetText;
        StartCoroutine(HideFinishTextAfterDelay(finishTextDuration));
    }
}
