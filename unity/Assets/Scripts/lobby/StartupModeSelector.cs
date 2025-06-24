using UnityEngine;
using UnityEngine.SceneManagement;

/**
 * @brief Handles setting the network mode and transitioning to the Lobby.
 */
public class StartupModeSelector : MonoBehaviour
{
    /**
     * @brief Called by UI to start in local mode.
     */
    public void StartLocal()
    {
        ServerManager.useRemoteStatic = false;
        SceneManager.LoadScene("Lobby");
    }

    /**
     * @brief Called by UI to start in remote mode.
     */
    public void StartRemote()
    {
        ServerManager.useRemoteStatic = true;
        SceneManager.LoadScene("Lobby");
    }
}
