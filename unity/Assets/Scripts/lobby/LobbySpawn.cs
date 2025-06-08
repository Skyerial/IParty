using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore;
using UnityEngine.UI;

public class LobbySpawn : MonoBehaviour
{
  private int playerIndex = 0;
  private Transform[] playerSlots;

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        GameObject playersParent = GameObject.Find("Players");

        string cardName = $"PlayerCard ({playerIndex})";
        Transform playerCardTransform = playersParent.transform.Find(cardName);

        if (playerCardTransform == null)
        {
            return;
        }

        playerInput.transform.SetParent(playerCardTransform, false);
        float offsetY = -playerCardTransform.GetComponent<RectTransform>().rect.height * 0.30f;
        playerInput.transform.localPosition = new Vector3(0f, offsetY, 0f);
        playerInput.transform.localRotation = Quaternion.identity;
        playerInput.transform.localScale = Vector3.one * 150f;

        GameObject spawnObj = GameObject.Find("Spawn");
        playerIndex++;

        string nextCardName = $"PlayerCard ({playerIndex})";
        Transform nextCard = playersParent.transform.Find(nextCardName);
        if (nextCard != null)
        {
            spawnObj.transform.SetParent(nextCard, false);
            spawnObj.transform.localPosition = Vector3.zero;
        }
        else
        {
            spawnObj.SetActive(false);
        }
    }
}
