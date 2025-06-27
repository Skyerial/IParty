using UnityEngine;

/**
 * @class QuitGame
 * @brief Quits the application when triggered by a UI event.
 */
public class QuitGame : MonoBehaviour
{

    /**
     * @brief Quits the game when this method is called.
     * Can be linked to a UI button via the Inspector.
     * @return void
     */
    public void OnQuitButtonClicked()
    {
        Debug.Log("Quit Game triggered");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
