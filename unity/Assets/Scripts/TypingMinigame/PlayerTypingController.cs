using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using NUnit.Framework.Interfaces;


/**
* @brief Attached to a player and controls the text that the player needs to type,
* removing it on a correctly typed word, updating the words left counter
* and updating the cursor appropriately
*/
public class PlayerTypingController : MonoBehaviour
{
    public TMP_InputField inputField;
    public GameObject spawner;
    public TextMeshProUGUI wordsLeftText;
    public PlayerRaceController raceController;
    public TextSpawner textSpawner;
    public int playerInputIndex;
    public int finishPostion;
    
    private string currentTargetWord = null;
    private string cursor = $"<color=yellow>|</color>";
    private int inputCounter = 0;
    private int cleanupCounter = 0;
    private int wordsLeft = 0;


    public void Initialize()
    {
        textSpawner = spawner.GetComponent<TextSpawner>();
        wordsLeft = textSpawner.spawnedWords.Count;
        inputField.onValueChanged.RemoveAllListeners(); // Prevent duplicate listeners
        inputField.onValueChanged.AddListener(HandleInput);
        UpdateWordsLeftText();
        UpdateCursorPosition(0);
    }

    public void HandleInput(string input)
    {
        int visualIndex = inputCounter - cleanupCounter;
        if (visualIndex < 0 || visualIndex >= spawner.transform.childCount)
            return;

        Transform wordObj = spawner.transform.GetChild(visualIndex);
        TextMeshProUGUI targetText = wordObj.GetComponent<TextMeshProUGUI>();

        if (currentTargetWord == null)
            currentTargetWord = textSpawner.spawnedWords[inputCounter];


        string inputLower = input.Trim().ToLower();
        string originalLower = currentTargetWord.ToLower();

        UpdateWordHighlight(inputLower, originalLower, targetText);

        if (inputLower == originalLower)
        {
            InputWordCorrect(wordObj);
        }
    }

    private void InputWordCorrect(Transform wordObj)
    {
        TM_MusicController.Instance.PlayCorrectWordSFX();

        inputField.text = "";
        raceController?.OnWordTyped();
        StartCoroutine(AnimateAndDestroy(wordObj.gameObject));

        inputCounter++;
        currentTargetWord = null;

        wordsLeft--;
        UpdateWordsLeftText();

        UpdateCursorPosition(inputCounter - cleanupCounter);

        if (wordsLeft == 0)
        {
            raceController?.WinningAnim();
            Debug.Log($"{gameObject.name} finished!");
            TMGameManager.Instance?.OnPlayerFinished(this);
        }
    }

    private void UpdateWordsLeftText()
    {
        wordsLeftText.text = wordsLeft.ToString();
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
        ResetWordToPlainText();

        Animator animator = word.GetComponent<Animator>();

        if (animator != null)
        {
            animator.SetTrigger("Break");
            yield return new WaitForSeconds(0.5f);
        }

        Destroy(word);
        cleanupCounter++;
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
            result += $"{cursor}<color=white>{remaining}</color>";
        }

        // add all extra letters as mistakes, but this shows how much you missed typed
        if (userInput.Length >= targetWord.Length)
            result += $"<color=#800000ff>{userInput.Substring(targetWord.Length)}</color>{cursor}";

        wordText.text = result;
    }

}
