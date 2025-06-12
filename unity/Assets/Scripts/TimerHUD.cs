using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TimerHUD : MonoBehaviour
{
    [SerializeField] private float totalTime = 10.0f;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject timerHUD;

    private float timeLeft;
    private bool timerRunning = true;

    void Start()
    {
        timeLeft = totalTime;
        timerHUD.SetActive(true);
    }

    void Update()
    {
        if (timerRunning)
        {
            timeLeft -= Time.deltaTime;
            timeLeft = Mathf.Max(0, timeLeft);

            if (timerText != null)
            {
                timerText.text = timeLeft.ToString("F1");

                float percentElapsed = 1f - (timeLeft / totalTime);

                // Change text color based on elapsed time
                if (percentElapsed >= 0.9f)
                {
                    timerText.color = Color.red;
                }
                else if (percentElapsed >= 0.7f)
                {
                    timerText.color = new Color(1f, 0.65f, 0f); // orange
                }
                else
                {
                    timerText.color = Color.white;
                }
            }

            if (timeLeft <= 0)
            {
                timerRunning = false;
                Debug.Log("Time's up!");
                timerHUD.SetActive(false);
                // TODO: Trigger end-of-game logic here
            }
        }
    }
}
