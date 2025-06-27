using UnityEngine;
using TMPro;
using System.Collections;


/**
* @brief Attached to a player and controls the text that the player needs to type,
* removing it on a correctly typed word, updating the words left counter
* and updating the cursor appropriately
*/
public class PlayerTypingController : MonoBehaviour
{
    /** @brief The input field that the user input will be put into */
    public TMP_InputField inputField;

    /** @brief Spawn location for the words */
    public GameObject spawner;

    /** @brief The TextMeshProUGUI object that shows how many words a player still needs to type */
    public TextMeshProUGUI wordsLeftText;

    /** @brief The PlayerRaceController used to move and animate the player */
    public PlayerRaceController raceController;

    /** @brief TextSpawner object that contains all words */
    public TextSpawner textSpawner;

    /** @brief Index to keep track of which player this is in the TMGameManager list */
    public int playerInputIndex;

    /** @brief The finish position of the player this script is attached to, initialized to be last */
    public int finishPostion = 5;

    private string currentTargetWord = null;
    private string cursor = $"<color=yellow>|</color>";

    /** @brief The index of the word that needs to be typed now */
    private int inputCounter = 0;

    /** @brief The index for the word that needs to be cleaned up */
    private int cleanupCounter = 0;
    private int wordsLeft = 0;

    /**
    * @brief Setup the row for the player with the words it needs to type, the cursor, setting the counter
                and setting up the listener for the inputfield
    */
    public void Initialize()
    {
        textSpawner = spawner.GetComponent<TextSpawner>();
        wordsLeft = textSpawner.spawnedWords.Count;
        inputField.onValueChanged.RemoveAllListeners(); // Prevent duplicate listeners
        inputField.onValueChanged.AddListener(HandleInput);
        UpdateWordsLeftText();
        UpdateCursorPosition(0);
    }

    /**
    * @brief It checks the given input to the current targetword and highlights it correctly
                and also start the deletion of the targetword on correct and updating it
    * @param[IN] input The input coming from the remote player, a part of the word or the whole word
    */
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

    /**
    * @brief Handle everything for a correct typed word, start animations, clear the input, update cursor,
                update counter and check if player is finished
    * @param[IN] wordObj The object that contains the current targetword
    */
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

    /**
    * @brief Update the words left to type to win
    */
    private void UpdateWordsLeftText()
    {
        wordsLeftText.text = wordsLeft.ToString();
    }

    /**
    * @brief Update the cursor position to the next word, basically doing a space
    * @param[IN] visualIndex The index of the word that the cursor needs to go to
    */
    private void UpdateCursorPosition(int visualIndex)
    {

        if (spawner.transform.childCount > visualIndex && visualIndex >= 0)
        {
            Transform wordObj = spawner.transform.GetChild(visualIndex);
            TextMeshProUGUI targetText = wordObj.GetComponent<TextMeshProUGUI>();
            targetText.text = cursor + textSpawner.spawnedWords[inputCounter];
        }
    }

    /**
    * @brief To be able to animate the words with a color it needs to be set back to white first
    */
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

    /**
    * @brief Call to destroy the given word with an animation
    * @param[IN] word The word to destroy
    */
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

    /**
    * @brief Color the targetword based on the given input, green on a correct letter and red on wrong letter.
                Characters more than the word will be shown in red. Cursor will be put after the last typed character
    * @param[IN] userInput The input coming from the remote user
    * @param[IN] targetWord The target word that needs to be typed
    * @param[IN] wordText The textbox of the targetword, used to color the text
    */
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
