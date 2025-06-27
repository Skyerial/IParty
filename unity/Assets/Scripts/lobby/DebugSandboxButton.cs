using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/**
 * @brief Button to load a sandbox scene for debugging purposes in the lobby.
 * This script creates a button in the UI that, when clicked,
 * loads a specified sandbox scene for testing.
 */
public class DebugSandboxButton : MonoBehaviour
{
    [Header("Debug Toggle")]
    [SerializeField] private bool enableDebugButton = false;

    [Header("UI Settings")]
    [SerializeField] private Vector2 buttonPosition = new Vector2(-100, -50);
    [SerializeField] private Vector2 buttonSize = new Vector2(180, 40);
    [SerializeField] private string sandboxSceneName = "MobileSandbox";
    [SerializeField] private Font buttonFont; // <- Add this
    public static bool DebugEnabled { get; private set; } = false;


    /**
     * @brief Start is called before the first frame update.
     * This method checks if the debug button should be enabled and creates it if so only in the unity editor.
     */
    private void Start()
    {
#if UNITY_EDITOR
        if (!enableDebugButton) return;

        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene != "Lobby") return;

        CreateDebugButton();
#endif
    }

    /**
     * @brief Creates a debug button in the UI to load the sandbox scene in the lobby.
     * This method sets up the button's appearance and functionality.
     */
    private void CreateDebugButton()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("No Canvas found for DebugSandboxButton.");
            return;
        }

        GameObject buttonGO = new GameObject("DebugSandboxButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonGO.transform.SetParent(canvas.transform, false);

        RectTransform rect = buttonGO.GetComponent<RectTransform>();
        rect.anchorMax = new Vector2(1, 1);
        rect.anchorMin = new Vector2(1, 1);
        rect.pivot = new Vector2(1, 1);
        rect.anchoredPosition = buttonPosition;
        rect.sizeDelta = buttonSize;

        Image img = buttonGO.GetComponent<Image>();
        img.color = Color.gray;

        GameObject textGO = new GameObject("Text", typeof(RectTransform), typeof(Text));
        textGO.transform.SetParent(buttonGO.transform, false);
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        Text text = textGO.GetComponent<Text>();
        text.text = "Go Sandbox";
        text.alignment = TextAnchor.MiddleCenter;
        text.font = buttonFont != null ? buttonFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.color = Color.black;


        Button btn = buttonGO.GetComponent<Button>();
        btn.onClick.AddListener(() =>
        {
            DebugEnabled = true;
            Debug.Log("DEBUG button clicked: loading sandbox scene...");
            SceneManager.LoadScene(sandboxSceneName);
        });
    }
}
