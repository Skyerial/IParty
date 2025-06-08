using UnityEngine;
using TMPro;
using System.Collections;

public class PlayerTypingController : MonoBehaviour
{
    public TMP_InputField inputField;
    public GameObject spawner;
    public TextMeshProUGUI wordsLeftText;
    public PlayerRaceController raceController;

    private void Start()
    {
        inputField.onValueChanged.AddListener(HandleInput);
        UpdateWordsLeftText();
    }

    private void HandleInput(string input)
    {
        if (spawner.transform.childCount == 0) return;

        Transform wordObj = spawner.transform.GetChild(0);
        TextMeshProUGUI targetText = wordObj.GetComponent<TextMeshProUGUI>();

        if (input.Trim().ToLower() == targetText.text.Trim().ToLower())
        {
            inputField.text = "";
            raceController?.OnWordTyped();
            StartCoroutine(AnimateAndDestroy(wordObj.gameObject));

            if (spawner.transform.childCount == 0)
            {
                Debug.Log($"{gameObject.name} finished!");
                GameManager.Instance?.OnPlayerFinished(this);
            }
        }
    }

    private void UpdateWordsLeftText()
    {
        wordsLeftText.text = spawner.transform.childCount.ToString();
    }

    private IEnumerator AnimateAndDestroy(GameObject word)
    {
        Animator animator = word.GetComponent<Animator>();

        if (animator != null)
        {
            animator.SetTrigger("Break");
            yield return new WaitForSeconds(0.5f);
        }

        Destroy(word);
        yield return null; // wait one frame for hierarchy update

        UpdateWordsLeftText();

        if (spawner.transform.childCount == 0)
        {
            Debug.Log($"{gameObject.name} finished!");
            GameManager.Instance?.OnPlayerFinished(this);
        }
    }
}
