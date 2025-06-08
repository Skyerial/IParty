using TMPro;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Gameplay : MonoBehaviour
{
    private bool _gameStart = false;
    private InputAction _enableAction;
    public TMP_InputField _inputField;
    private int _wordCount = 0;
    public GameObject _spawner;
    public HorizontalLayoutGroup _layoutGroup;

    private void Start()
    {
        _enableAction = InputSystem.actions.FindAction("GameStart");
        _inputField.onValueChanged.AddListener(HandleText);
    }

    void Update()
    {
        if (_enableAction.WasReleasedThisFrame() && !_gameStart)
        {
            _gameStart = true;
        }

        if (_wordCount == 5)
        {
            Debug.Log("won");
        }
    }

    void HandleText(string text)
    {
        string userInput = _inputField.text;
        Debug.Log("Player typed: " + userInput);
        TextMeshProUGUI currentText = GameObject.Find("Child_" + _wordCount).GetComponent<TextMeshProUGUI>();


        if (userInput == currentText.text)
        {
            Destroy(currentText);
            var child = _spawner.transform.GetChild(0);
            Destroy(child.gameObject);
            _wordCount += 1;
            _inputField.text = "";
        }
    }
}
