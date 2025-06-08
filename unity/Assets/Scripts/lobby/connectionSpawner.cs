using UnityEngine;
using System.Collections.Generic;

public class ConnectionSpawner : MonoBehaviour
{
    public GameObject avatarPrefab;
    public Transform[] playerCards;

    private Dictionary<string, GameObject> spawnedAvatars = new();
    private Dictionary<string, Transform> assignedSlots = new();

    void Update()
    {
        foreach (var entry in ServerManager.allControllers)
        {
            string ip = entry.Key;

            if (!spawnedAvatars.ContainsKey(ip))
            {
                Transform card = GetFreeSlot();
                if (card == null) return;

                Transform characterFrame = card.Find("PlayerCharacterFrame");
                if (characterFrame == null)
                {
                    Debug.LogWarning($"Missing 'PlayerCharacterFrame' in {card.name}");
                    continue;
                }

                // ✅ Show the character frame
                characterFrame.gameObject.SetActive(true);

                // ✅ Spawn the avatar inside the frame
                GameObject avatar = Instantiate(avatarPrefab, characterFrame);
                avatar.transform.localPosition = Vector3.zero;
                avatar.transform.localScale = Vector3.one;

                spawnedAvatars[ip] = avatar;
                assignedSlots[ip] = card;

                Debug.Log($"Spawned avatar for {ip} in {card.name}");
            }
        }

        // Handle disconnections
        List<string> toRemove = new();
        foreach (var ip in spawnedAvatars.Keys)
        {
            if (!ServerManager.allControllers.ContainsKey(ip))
                toRemove.Add(ip);
        }

        foreach (var ip in toRemove)
        {
            Transform card = assignedSlots[ip];
            GameObject avatar = spawnedAvatars[ip];

            Destroy(avatar);
            spawnedAvatars.Remove(ip);
            assignedSlots.Remove(ip);

            // ❌ Hide the character frame again
            Transform characterFrame = card.Find("PlayerCharacterFrame");
            if (characterFrame != null)
                characterFrame.gameObject.SetActive(false);

            Debug.Log($"Removed avatar for {ip} from {card.name}");
        }
    }

    Transform GetFreeSlot()
    {
        foreach (var slot in playerCards)
        {
            if (!assignedSlots.ContainsValue(slot))
                return slot;
        }
        return null;
    }
}
