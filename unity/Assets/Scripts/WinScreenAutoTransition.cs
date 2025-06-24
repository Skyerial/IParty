using UnityEngine;
using UnityEngine.SceneManagement;

public class WinScreenAutoTransition : MonoBehaviour
{
    [SerializeField] private float delayBeforeTransition = 2.5f; 

    private void Start()
    {
        Invoke(nameof(GoToGameBoard), delayBeforeTransition);
    }

    private void GoToGameBoard()
    {
        SceneManager.LoadScene("Game_Board");
    }
}
