using UnityEngine;
using UnityEngine.SceneManagement;

public class WinScreenAutoTransition : MonoBehaviour
{
    [SerializeField] private float delayBeforeTransition = 2.5f;

    private void Start()
    {
        Invoke(nameof(TransitionBasedOnScene), delayBeforeTransition);
    }

    private void TransitionBasedOnScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "WinScreen")
        {
            SceneManager.LoadScene("Game_Board");
        }
        else if (currentScene == "WinScreen3D")
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}
