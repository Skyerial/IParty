using UnityEngine;
using TMPro;

public class MinigameRanking : MonoBehaviour
{
    void Start()
    {
        DisplayRankings();
    }

    public void DisplayRankings()
    {
        Transform rankingGrid = GameObject.Find("MainGrid")?.transform;

        GameObject[] children = new GameObject[rankingGrid.childCount];

        for (int i = 0; i < rankingGrid.childCount; i++)
        {
            children[i] = rankingGrid.GetChild(i).gameObject;
        }

        for (int i = 0; i < PlayerManager.tempRanking.Count && i < children.Length; i++)
        {
            var player = PlayerManager.tempRanking[i];
            GameObject parentChild = children[i];

            Transform nameChild = parentChild.transform.Find("Name");

            if (nameChild != null)
            {
                TextMeshProUGUI nameText = nameChild.GetComponent<TextMeshProUGUI>();
                if (nameText != null)
                {
                    nameText.text = player.name;
                } 
            }
        }
    }
}

