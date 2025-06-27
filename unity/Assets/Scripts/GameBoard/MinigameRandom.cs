using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/**
 * @brief Handles the swipeable UI menu for minigame selection with spin animation and scene loading.
 */
public class swipe_menu : MonoBehaviour {
    /**
     * @brief The scrollbar UI element controlling the scroll position.
     */
    public GameObject scrollbar;

    /**
     * @brief Reference to the Scrollbar component.
     */
    private Scrollbar scroll;

    /**
     * @brief Array storing normalized scroll positions for each card.
     */
    private float[] pos;

    /**
     * @brief Total number of cards in the menu.
     */
    private int cardCount;

    /**
     * @brief Distance between adjacent scroll positions (normalized).
     */
    private float distance;

    /**
     * @brief Duration of the spin animation in seconds.
     */
    public float spinDuration = 3f;

    /**
     * @brief Speed of the spin animation.
     */
    public float spinSpeed = 3f;

    /**
     * @brief Indicates whether the spin animation is currently running.
     */
    private bool spinning = false;

    /**
     * @brief Indicates whether the spin animation has completed.
     */
    private bool hasSpun = false;

    /**
     * @brief Initializes scroll positions and starts the spin coroutine.
     */
    void Start() {
        scroll = scrollbar.GetComponent<Scrollbar>();
        cardCount = transform.childCount;
        pos = new float[cardCount];
        distance = 1f / (cardCount - 1);

        for (int i = 0; i < cardCount; i++) {
            pos[i] = distance * i;
        }

        StartCoroutine(SpinToRandom());
    }

    /**
     * @brief Updates card scaling based on current scroll position for visual feedback.
     */
    void Update() {
        if (spinning) return;

        float scroll_pos = scroll.value;

        for (int i = 0; i < pos.Length; i++) {
            if (scroll_pos < pos[i] + (distance / 2) && scroll_pos > pos[i] - (distance / 2)) {
                transform.GetChild(i).localScale = Vector2.Lerp(transform.GetChild(i).localScale, new Vector2(1f, 1f), 0.1f);

                for (int a = 0; a < pos.Length; a++) {
                    if (a != i) {
                        transform.GetChild(a).localScale = Vector2.Lerp(transform.GetChild(a).localScale, new Vector2(0.8f, 0.8f), 0.1f);
                    }
                }
            }
        }
    }

    /**
     * @brief Spins the scroll menu to a random card, highlights it, and loads the corresponding scene.
     * @return Coroutine yielding control during animation and scene loading.
     */
    IEnumerator SpinToRandom() {
        spinning = true;

        float time = 0;
        while (time < spinDuration) {
            scroll.value += Time.deltaTime * spinSpeed;
            if (scroll.value > 1f) scroll.value = 0f;
            time += Time.deltaTime;
            yield return null;
        }

        int chosenIndex = Random.Range(1, cardCount - 1);
        float targetPos = pos[chosenIndex];

        while (Mathf.Abs(scroll.value - targetPos) > 0.001f) {
            scroll.value = Mathf.Lerp(scroll.value, targetPos, 0.1f);
            yield return null;
        }

        scroll.value = targetPos;
        spinning = false;
        hasSpun = true;

        string cardName = transform.GetChild(chosenIndex).name.Replace(" clone", "").Trim();
        Debug.Log(cardName);
        yield return new WaitForSeconds(2f);
        string selectedScene;
        switch (cardName)
        {
            case "SpleefCard": selectedScene = "Spleef"; break;
            case "TankCard": selectedScene = "TankGame"; break;
            case "TurfCard": selectedScene = "Turf"; break;
            case "WhackCard": selectedScene = "GYRO"; break;
            case "HotpotatoCard": selectedScene = "Hotpotato"; break;
            case "SkyglutesCard": selectedScene = "Skyglutes"; break;
            case "TypingCard": selectedScene = "TypingMinigame"; break;
            default: selectedScene = "MainMenu"; break;
        }

        PlayerManager.changeCurrentMinigame(selectedScene);

        SceneManager.LoadScene("MinigameStart");

        yield return new WaitForSeconds(2f);
        gameObject.SetActive(false);
    }

    /**
     * @brief Returns whether the spin has completed.
     * @return True if the spin animation finished; false otherwise.
     */
    public bool HasFinishedSpinning() => hasSpun;
}
