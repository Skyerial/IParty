using UnityEngine;

public class LobbyUIManager : MonoBehaviour
{
    public GameObject[] userPanels; // Drag UserPanel P1, P2, P3, P4 here in Inspector

    // Call this when a new player joins
    public void ActivatePlayerPanel(int index, string playerName)
    {
        if (index >= 0 && index < userPanels.Length)
        {
            userPanels[index].SetActive(true);

            // Optional: update name
            var nameText = userPanels[index].transform.Find("PlayerCharacterFrame/PlayerName")?.GetComponent<UnityEngine.UI.Text>();
            if (nameText != null)
                nameText.text = playerName;
        }
    }
}
