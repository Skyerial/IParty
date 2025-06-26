using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.InputSystem;
using TMPro;

public class SetGameManager : MonoBehaviour
{
    public GameObject    playerUIPrefab;
    public RectTransform uiParent;
    public Vector2       uiOffset = new Vector2(10f, -10f);

    public static SetGameManager setGameManager;

    public static List<CardData> allCards = new List<CardData>();
    public static List<CardData> dealtCards = new List<CardData>();

    public static List<PlayerInput> gamePlayers = new List<PlayerInput>();
    public static Dictionary<PlayerInput, int> scores = new Dictionary<PlayerInput, int>();
    public static Dictionary<PlayerInput, List<SetCard>> playerSelection = new Dictionary<PlayerInput, List<SetCard>>();
    private static Dictionary<PlayerInput, GameObject> scoreUIs = new Dictionary<PlayerInput, GameObject>();

    private SwitchScene sceneSwitcher;
    public string nextSceneName;

    void Awake()
    {
        setGameManager = this;
        Debug.Log("Gird Awake");

        setGameManager.sceneSwitcher = GetComponent<SwitchScene>();
        if (setGameManager.sceneSwitcher == null)
        {
            Debug.Log("No sceneswitcher");
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Start manager");
        CreateCards();
        Shuffle();
        GridScript.DealCards();
        GridScript.gridScript.CreateCardGrid();
    }

    // Checks if cards c1, c2 and c3 form a valid set.
    // i.e. for each of the 4 attributes, it's either all the same, or all different.
    //
    // Returns true if the cards form a set,
    // False otherwise.
    public static bool isSet(CardData c1, CardData c2, CardData c3)
    {
        bool nmbr = (((c1.number == c2.number) && (c2.number == c3.number)) ||
        ((c1.number != c2.number) && (c2.number != c3.number) && (c1.number != c3.number)));

        bool fill = (((c1.filling == c2.filling) && (c2.filling == c3.filling)) ||
        ((c1.filling != c2.filling) && (c2.filling != c3.filling) && (c1.filling != c3.filling)));

        bool shpe = (((c1.shape == c2.shape) && (c2.shape == c3.shape)) ||
        ((c1.shape != c2.shape) && (c2.shape != c3.shape) && (c1.shape != c3.shape)));

        bool clr = (((c1.color == c2.color) && (c2.color == c3.color)) ||
        ((c1.color != c2.color) && (c2.color != c3.color) && (c1.color != c3.color)));

        return ((nmbr && fill) && (shpe && clr));

    }

    // Checks if the DealtCards list contains a valid set.
    //
    // Returns true if DealtCards contains a set,
    // False otherwise.
    public static bool hasSet() {
        for (int i = 0; i < dealtCards.Count; i++)
        {
            for (int j = 0; j < dealtCards.Count; j++)
            {
                if (i != j) {
                    for (int k = 0; k < dealtCards.Count; k++)
                    {
                        if (i != k && j != k)
                        {
                            if (isSet(dealtCards[i], dealtCards[j], dealtCards[k]))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
        }

        return false;
    }

    // Creates all 81 cards for the game.
    static void CreateCards()
    {
        Debug.Log("Creating Cards");
        allCards.Clear();
        for (int n = 0; n < 3; n++)
        {
            for (int f = 0; f < 3; f++)
            {
                for (int s = 0; s < 3; s++)
                {
                    for (int c = 0; c < 3; c++)
                    {
                        allCards.Add(new CardData(n, f, s, c));
                    }
                }
            }
        }
    }

    // Shuffles the cards in the allCards list.
    void Shuffle()
    {
        Debug.Log("Shuffling Cards");
        for (int i = 0; i < allCards.Count; i++)
        {
            CardData temp = allCards[i];
            int randIndex = Random.Range(i, allCards.Count);
            allCards[i] = allCards[randIndex];
            allCards[randIndex] = temp;
        }
    }

    public void RegisterPlayerGame(PlayerInput player)
    {
        Debug.Log("Registering player");
        if (!gamePlayers.Contains(player)) gamePlayers.Add(player);

        if (!scores.ContainsKey(player)) scores[player] = 0;

        if (!playerSelection.ContainsKey(player)) playerSelection[player] = new List<SetCard>();

        SelectorScript selector = player.GetComponent<SelectorScript>();
        Transform selectorTransform = selector.transform;

        selectorTransform.SetParent(GridScript.gridScript.transform.parent, false);

        var dev = selector.playerInput.devices[0];
        var mat = PlayerManager.findColor(dev);
        selector.SetColor(mat.color);
        OnPlayerJoined(player, mat.color);
    }

    void SetAnchor(RectTransform rt, int idx)
    {
        switch (idx)
        {
            case 0:
                rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
                rt.pivot = new Vector2(0, 1);
                break;
            case 1:
                rt.anchorMin = rt.anchorMax = new Vector2(1, 1);
                rt.pivot = new Vector2(1, 1);
                break;
            case 2:
                rt.anchorMin = rt.anchorMax = new Vector2(0, 0);
                rt.pivot = new Vector2(0, 0);
                break;
            default:
                rt.anchorMin = rt.anchorMax = new Vector2(1, 0);
                rt.pivot = new Vector2(1, 0);
                break;
        }
    }

    void OnPlayerJoined(PlayerInput player, Color color)
    {
        SelectorScript selector = player.GetComponent<SelectorScript>();
        var dev = selector.playerInput.devices[0];

        GameObject uiObj = Instantiate(playerUIPrefab, uiParent);
        var rect  = uiObj.GetComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;
        rect.localRotation = Quaternion.identity;
        rect.localScale = Vector3.one;
        int idx   = gamePlayers.Count;
        SetAnchor(rect, idx);

        Vector2 pos = uiOffset;
        if ((idx & 1) != 0) pos.x = -pos.x;
        if ((idx & 2) != 0) pos.y = -pos.y;
        rect.anchoredPosition = pos;

        var sw  = uiObj.transform.Find("Swatch").GetComponent<Image>();
        var txt = uiObj.transform.Find("ScoreText").GetComponent<TMP_Text>();
        sw.color = color;
        txt.text = scores[player].ToString();

        scoreUIs[player] = uiObj;
    }

    public static void UpdatescoreUI() {
        foreach (var player in gamePlayers)
        {
            var txt = scoreUIs[player].transform.Find("ScoreText").GetComponent<TMP_Text>();
            txt.text = scores[player].ToString();
        }
    }

    public static void EndGame()
    {
        Debug.Log("FINISHED GAME");

        var pm = Object.FindFirstObjectByType<PlayerManager>();
        pm.tempRankClear();

        var sortedScores = scores.OrderBy(score => score.Value);

        foreach (var score in sortedScores)
        {
            var dev = score.Key.GetComponent<SelectorScript>().playerInput.devices[0];
            pm.tempRankAdd(dev);
        }

        setGameManager.sceneSwitcher.LoadNewScene(setGameManager.nextSceneName);
    }
}
