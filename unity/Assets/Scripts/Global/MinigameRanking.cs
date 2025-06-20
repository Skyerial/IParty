using UnityEngine;
using TMPro;
using UnityEngine.UI;

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

            Transform imageChild = parentChild.transform.Find("PlayerImage");
            if (imageChild != null)
            {
                Image image = imageChild.GetComponent<Image>();
                if (image != null && player.face != null)
                {
                    Texture2D faceTexture = new Texture2D(2, 2);
                    faceTexture.LoadImage(player.face);
                    image.sprite = Sprite.Create(faceTexture, new Rect(0, 0, faceTexture.width, faceTexture.height), new Vector2(0.5f, 0.5f));
                }
            }
        }
    }
}

