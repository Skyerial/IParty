using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.InputSystem;

public class GridScript : MonoBehaviour {
    public GameObject cardPrefab;
    public GameObject PlayerScore;
    private List<CardData> allCards = new List<CardData>();
    private List<CardData> dealtCards = new List<CardData>();

    public static List<PlayerInput> gamePlayers = new List<PlayerInput>();
    private Dictionary<PlayerInput, int> scores = new Dictionary<PlayerInput, int>();
    private Dictionary<PlayerInput, List<CardData>> PlayerSelection = new Dictionary<PlayerInput, List<CardData>>();

    void Start() {
        if (ServerManager.allControllers != null) {
            foreach (var device in ServerManager.allControllers.Values.ToArray()) {
                Debug.Log("Spawning...");
                PlayerInputManager.instance.playerPrefab = PlayerScore;
                PlayerInput playerInput = PlayerInputManager.instance.JoinPlayer(-1, -1, null, device);

                if (playerInput != null) {
                    RegisterPlayerScore(playerInput);
                } else {
                    Debug.LogError("PlayerInput == NULL");
                }
            }
        }

        foreach (PlayerInput player in gamePlayers) {
            scores.Add(player, 0);
        }

        CreateCards();
        Shuffle();
        DealCards();
        CreateCardGrid();
        ActivateAllInput();
    }

    // Checks if cards c1, c2 and c3 form a valid set.
    // i.e. for each of the 4 attributes, it's either all the same, or all different.
    bool isSet(CardData c1, CardData c2, CardData c3) {
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
    bool hasSet() {
        for (int i = 0; i < dealtCards.Count; i++) {
            for (int j = 0; j < dealtCards.Count; j++) {
                if (i != j) {
                    for (int k = 0; k < dealtCards.Count; k++) {
                        if (i != k && j != k) {
                            if (isSet(dealtCards[i], dealtCards[j], dealtCards[k])) {
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
    void CreateCards() {
        allCards.Clear();
        for (int n = 0; n < 3; n++) {
            for (int f = 0; f < 3; f++) {
                for (int s = 0; s < 3; s++) {
                    for (int c = 0; c < 3; c++) {
                        allCards.Add(new CardData(n, f, s, c));
                    }
                }
            }
        }
    }

    // Shuffles the cards in the allCards list.
    void Shuffle() {
        for (int i = 0; i < allCards.Count; i++) {
            CardData temp = allCards[i];
            int randIndex = Random.Range(i, allCards.Count);
            allCards[i] = allCards[randIndex];
            allCards[randIndex] = temp;
        }
    }

    // Moves 12 cards from the allCards to the DealtCards list.
    // Makes sure the dealt card contain a valid set,
    // if there are still cards available.
    void DealCards() {
        for (int i = 0; dealtCards.Count < 12; i++) {
            if (allCards.Count <= 0) {
                break;
            }

            dealtCards.Add(allCards[0]);
            allCards.RemoveAt(0);
        }

        while (allCards.Count > 0 && !hasSet()) {
            int i = Random.Range(0, allCards.Count);
            int j = Random.Range(0, dealtCards.Count);
            CardData temp = allCards[i];
            allCards[i] = dealtCards[j];
            dealtCards[j] = temp;
        }

        UpdateScoreUI();
    }

    // Initializes the cards in the DealtCards list.
    void CreateCardGrid() {
        for (int i = 0; i < dealtCards.Count; i++) {
            GameObject cardGO = Instantiate(cardPrefab, transform);
            SetCard setCard = cardGO.GetComponent<SetCard>();
            CardData data = dealtCards[i];
            setCard.Initialize(data);
            setCard.SetGridScript(this);
        }
    }

    // Adds or removes the selected ard from the selected list for that player,
    // depending on if it's already present or not.
    //
    // If the selectedSet list reaches 3 elements it checks if this is a valid set,
    // adding a point to the score when it is valid, and removing the cards from the dealtCards.
    // It clears the selectedCard list whenever it reaches 3 elements.
    public void CardSelected(CardData selectedCard, PlayerInput player) {
        if (PlayerSelection[player].Contains(selectedCard)) {
            PlayerSelection[player].Remove(selectedCard);
        } else {
            PlayerSelection[player].Add(selectedCard);
            if (PlayerSelection[player].Count == 3) {
                if (isSet(PlayerSelection[player][0], PlayerSelection[player][1], PlayerSelection[player][2])) {
                    scores[player]++;
                    dealtCards.Remove(PlayerSelection[player][0]);
                    dealtCards.Remove(PlayerSelection[player][1]);
                    dealtCards.Remove(PlayerSelection[player][2]);
                }
                foreach (PlayerInput p in gamePlayers) {
                    PlayerSelection[p].Clear();
                }
            }
        }

        // Re-initialize the card grid.
        for (int i = 0; i < this.transform.childCount; i++) {
            Destroy(this.transform.GetChild(i).gameObject);
        }
        DealCards();
        CreateCardGrid();
    }

    void UpdateScoreUI() {

    }

    void EndGame() {
        Debug.Log("FINISHED GAME");
    }

    public static void RegisterPlayerScore(PlayerInput player) {
        Debug.Log(player);
        gamePlayers.Add(player);
        player.DeactivateInput();
    }

    void ActivateAllInput() {
        foreach (var player in gamePlayers) {
            player.ActivateInput();
        }
    }
}

public struct CardData {
    public int number;
    public int filling;
    public int shape;
    public int color;

    public CardData(int number, int filling, int shape, int color)
    {
        this.number = number;
        this.filling = filling;
        this.shape = shape;
        this.color = color;
    }
}
