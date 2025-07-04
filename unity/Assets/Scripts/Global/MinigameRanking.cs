using UnityEngine;
using TMPro;
using UnityEngine.UI;

/**
 * @brief Displays player rankings and their faces at the end of the minigame.
 */
public class MinigameRanking : MonoBehaviour
{
    /**
     * @brief Called when the object is initialized. Triggers the ranking display.
     * @return void
     */
    void Start()
    {
        DisplayRankings();
    }

    /**
     * @brief Updates UI elements with player ranking names and face images.
     * @return void
     */
    public void DisplayRankings()
            {
                Transform rankingGrid = GameObject.Find("MainGrid")?.transform;

                if (rankingGrid == null) return;

                GameObject[] children = new GameObject[rankingGrid.childCount];

                for (int i = 0; i < rankingGrid.childCount; i++)
                {
                    children[i] = rankingGrid.GetChild(i).gameObject;
                }

                int usedSpots = Mathf.Min(PlayerManager.tempRanking.Count, children.Length);

                for (int i = 0; i < usedSpots; i++)
                {
                    var player = PlayerManager.tempRanking[i];
                    player.winner = 3 - i < 0 ? 0 : 3 - i;
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
                            image.sprite = Sprite.Create(
                                faceTexture,
                                new Rect(0, 0, faceTexture.width, faceTexture.height),
                                new Vector2(0.5f, 0.5f)
                            );
                        }
                    }

                    parentChild.SetActive(true); 
                }

                
                for (int i = usedSpots; i < children.Length; i++)
                {
                    children[i].SetActive(false);
        }
    }
}
