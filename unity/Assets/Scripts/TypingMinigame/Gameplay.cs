using TMPro;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;

public class Gameplay : MonoBehaviour
{
    private bool _gameStart = false;
    private InputAction _enableAction;
    public TMP_InputField _inputField;
    private int _wordCount = 0;
    public GameObject _spawner;
    public TextMeshProUGUI wordsLeftText;

    private void Start()
    {
        _enableAction = InputSystem.actions.FindAction("GameStart");
        _inputField.onValueChanged.AddListener(HandleText);

        int wordsRemaining = _spawner.transform.childCount;
        wordsLeftText.text = wordsRemaining.ToString();
    }

    void Update()
    {
        if (_enableAction.WasReleasedThisFrame() && !_gameStart)
        {
            _gameStart = true;
        }
    }

    IEnumerator AnimateAndDestroy(GameObject word)
    {
        Animator animator = word.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Break"); // Or whatever your trigger is
            yield return new WaitForSeconds(0.5f); // Let the animation play
        }

        Destroy(word);
    }

    void HandleText(string text)
    {
        Transform firstChild = _spawner.transform.GetChild(0);
        TextMeshProUGUI currentText = firstChild.GetComponent<TextMeshProUGUI>();

        string userInput = _inputField.text;
        Debug.Log("Player typed: " + userInput);
        Debug.Log("Comparing: '" + userInput + "' to '" + currentText.text + "'");

        if (userInput.Trim().ToLower() == currentText.text.Trim().ToLower())
        {
            StartCoroutine(AnimateAndDestroy(firstChild.gameObject));
            // Destroy(firstChild.gameObject);
            _wordCount += 1;
            _inputField.text = "";

            int wordsRemaining = _spawner.transform.childCount - 1;
            wordsLeftText.text = wordsRemaining.ToString();

            if (wordsRemaining == 0)
            {
                Debug.Log("won");
                // handle winning
            }
        }
    }
}
