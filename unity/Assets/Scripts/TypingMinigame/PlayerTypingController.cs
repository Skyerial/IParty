using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;

public class PlayerTypingController : MonoBehaviour
{
    public TMP_InputField inputField;
    public GameObject spawner;
    public TextMeshProUGUI wordsLeftText;
    public PlayerRaceController raceController;
    private string currentTargetWord = null;
    private string cursor = $"<color=yellow>|</color>";
    private int counter = 0;

    private void Start()
    {
        StartCoroutine(Prepare());
    }

    private IEnumerator Prepare()
    {
        yield return new WaitUntil(() => spawner.transform.childCount >= 10);
        inputField.onValueChanged.AddListener(HandleInput);
        UpdateWordsLeftText();
        UpdateCursorPosition(0);
    }

    private void HandleInput(string input)
    {
        if (spawner.transform.childCount == 0) return;

        Transform wordObj = spawner.transform.GetChild(counter);
        TextMeshProUGUI targetText = wordObj.GetComponent<TextMeshProUGUI>();

        if (currentTargetWord == null)
        {
            currentTargetWord = targetText.text.Substring(cursor.Length);
        }

        string inputLower = input.Trim().ToLower();
        string originalLower = currentTargetWord.ToLower();

        if (inputLower.Length > 0)
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

    private void UpdateCursorPosition(int child)
    {

        if (spawner.transform.childCount > child)
        {
            Transform wordObj = spawner.transform.GetChild(child);
            TextMeshProUGUI targetText = wordObj.GetComponent<TextMeshProUGUI>();
            targetText.text = cursor + targetText.text;
        }
    }

    // own function, reset word to basic text so animation can color
    private void ResetWordToPlainText()
    {
        Transform wordObj = spawner.transform.GetChild(0);
        TextMeshProUGUI targetText = wordObj.GetComponent<TextMeshProUGUI>();
        targetText.text = currentTargetWord;
    }

    private IEnumerator AnimateAndDestroy(GameObject word)
    {
        UpdateCursorPosition(1);
        ResetWordToPlainText();

        // jank: but needed to type around animation, we need next word faster than the animation
        currentTargetWord = null;
        counter += 1;

        Animator animator = word.GetComponent<Animator>();

        if (animator != null)
        {
            animator.SetTrigger("Break");
            yield return new WaitForSeconds(0.5f);
        }

        Destroy(word);
        counter -= 1;

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
                result += $"<color=yellow>{targetWord[i]}</color>";
            else
                result += $"<color=red>{targetWord[i]}</color>";
        }

        if (i < targetWord.Length)
        {
            string remaining = targetWord.Substring(i);
            result += $"<color=yellow>|</color><color=white>{remaining}</color>";
        }

        wordText.text = result;
    }

}
