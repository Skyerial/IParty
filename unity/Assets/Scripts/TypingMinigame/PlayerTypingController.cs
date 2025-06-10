using UnityEngine;
using TMPro;
using System.Collections;

public class PlayerTypingController : MonoBehaviour
{
    public TMP_InputField inputField;
    public GameObject spawner;
    public TextMeshProUGUI wordsLeftText;
    public PlayerRaceController raceController;
    private string currentTargetWord = null;

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

        if (currentTargetWord == null)
        {
            currentTargetWord = targetText.text;
        }

        string inputLower = input.Trim().ToLower();
        string originalLower = currentTargetWord.ToLower();

        UpdateWordHighlight(inputLower, originalLower, targetText);

        if (inputLower == originalLower)
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
        currentTargetWord = null;

        yield return null; // wait one frame for hierarchy update
        UpdateWordsLeftText();

        if (spawner.transform.childCount == 0)
        {
            Debug.Log($"{gameObject.name} finished!");
            GameManager.Instance?.OnPlayerFinished(this);
        }
    }

    private void UpdateWordHighlight(string userInput, string targetWord, TextMeshProUGUI wordText)
    {
        string result = "";
        int i = 0;

        for (; i < userInput.Length && i < targetWord.Length; i++)
        {
            if (userInput[i] == targetWord[i])
                result += $"<color=green>{targetWord[i]}</color>";
            else
                result += $"<color=red>{targetWord[i]}</color>";
        }

        if (i < targetWord.Length)
        {
            string remaining = targetWord.Substring(i);
            result += $"<color=white>{remaining}</color>";
        }

        wordText.text = result;
    }

}
