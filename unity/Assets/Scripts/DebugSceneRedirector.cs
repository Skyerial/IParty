using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugSceneRedirector : MonoBehaviour
{
    private void Start()
    {
        if (DebugSandboxButton.DebugEnabled)
        {
            SceneManager.LoadScene("MobileSandbox");
        }
    }
}
