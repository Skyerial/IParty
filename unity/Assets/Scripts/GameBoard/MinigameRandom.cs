using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class swipe_menu : MonoBehaviour {
    public GameObject scrollbar;
    private Scrollbar scroll;
    private float[] pos;
    private int cardCount;
    private float distance;
    public float spinDuration = 3f;
    public float spinSpeed = 3f;
    private bool spinning = false;
    private bool hasSpun = false;


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
            default: selectedScene = "MainMenu"; break;
        }

        SceneManager.LoadScene(selectedScene);

        yield return new WaitForSeconds(2f);
        gameObject.SetActive(false);
    }

    public bool HasFinishedSpinning() => hasSpun;
}
