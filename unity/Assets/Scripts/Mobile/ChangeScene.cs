using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    public void ReloadLevel()
    {
        SceneManager.LoadScene("SampleScene");
    }
}