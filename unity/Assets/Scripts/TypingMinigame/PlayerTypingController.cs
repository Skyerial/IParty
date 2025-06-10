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
    private TextSpawner textSpawner;
    private string currentTargetWord = null;
    private string cursor = $"<color=yellow>|</color>";
    private int inputCounter = 0;
    private int cleanupCounter = 0;

    private void Start()
    {
        StartCoroutine(Prepare());
    }

    private IEnumerator Prepare()
    {
        yield return new WaitUntil(() => spawner.transform.childCount >= 10);

        textSpawner = spawner.GetComponent<TextSpawner>();
        inputField.onValueChanged.AddListener(HandleInput);
        UpdateWordsLeftText();
        UpdateCursorPosition(0);
    }

    private void HandleInput(string input)
    {
        int visualIndex = inputCounter - cleanupCounter;
        if (visualIndex < 0 || visualIndex >= spawner.transform.childCount)
            return;

        //if (spawner.transform.childCount == 0) return;

        Transform wordObj = spawner.transform.GetChild(visualIndex);
        TextMeshProUGUI targetText = wordObj.GetComponent<TextMeshProUGUI>();

        if (currentTargetWord == null)
            // currentTargetWord = targetText.text.Substring(cursor.Length);
            currentTargetWord = textSpawner.spawnedWords[inputCounter];


        string inputLower = input.Trim().ToLower();
        string originalLower = currentTargetWord.ToLower();

        if (inputLower.Length > 0)
            UpdateWordHighlight(inputLower, originalLower, targetText);

        if (inputLower == originalLower)
        {
            inputField.text = "";
            raceController?.OnWordTyped();
            StartCoroutine(AnimateAndDestroy(wordObj.gameObject));

            inputCounter++;
            currentTargetWord = null;
            UpdateCursorPosition(inputCounter - cleanupCounter);


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

    private void UpdateCursorPosition(int visualIndex)
    {

        if (spawner.transform.childCount > visualIndex && visualIndex >= 0)
        {
            Transform wordObj = spawner.transform.GetChild(visualIndex);
            TextMeshProUGUI targetText = wordObj.GetComponent<TextMeshProUGUI>();
            targetText.text = cursor + textSpawner.spawnedWords[inputCounter];
        }
    }

    // own function, reset word to basic text so animation can color
    private void ResetWordToPlainText()
    {
        int visualIndex = inputCounter - cleanupCounter;
        if (visualIndex >= 0 && visualIndex < spawner.transform.childCount)
        {
            Transform wordObj = spawner.transform.GetChild(visualIndex);
            TextMeshProUGUI targetText = wordObj.GetComponent<TextMeshProUGUI>();
            targetText.text = textSpawner.spawnedWords[inputCounter];
        }
    }

    private IEnumerator AnimateAndDestroy(GameObject word)
    {
        // UpdateCursorPosition(1);
        ResetWordToPlainText();

        // // jank: but needed to type around animation, we need next word faster than the animation
        // currentTargetWord = null;
        // counter += 1;

        Animator animator = word.GetComponent<Animator>();

        if (animator != null)
        {
            animator.SetTrigger("Break");
            yield return new WaitForSeconds(0.5f);
        }

        Destroy(word);
        // counter -= 1;
        cleanupCounter++;
        
        // yield return null; // wait one frame for hierarchy update
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
