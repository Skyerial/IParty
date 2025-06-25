using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.InputSystem;

public class SetGameManager : MonoBehaviour
{
    public static GameObject selectorPrefab;
    public static List<CardData> allCards = new List<CardData>();
    public static List<CardData> dealtCards = new List<CardData>();

    public static List<PlayerInput> gamePlayers = new List<PlayerInput>();
    public static Dictionary<PlayerInput, int> scores = new Dictionary<PlayerInput, int>();
    public static Dictionary<PlayerInput, List<CardData>> playerSelection = new Dictionary<PlayerInput, List<CardData>>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
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
    public static bool isSet(CardData c1, CardData c2, CardData c3) {
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
    static void CreateCards() {
        Debug.Log("Creating Cards");
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
        Debug.Log("Shuffling Cards");
        for (int i = 0; i < allCards.Count; i++) {
            CardData temp = allCards[i];
            int randIndex = Random.Range(i, allCards.Count);
            allCards[i] = allCards[randIndex];
            allCards[randIndex] = temp;
        }
    }

    public static void RegisterPlayerGame(PlayerInput player) {
        Debug.Log("Registering player");
        if (!gamePlayers.Contains(player)) gamePlayers.Add(player);

        if (!scores.ContainsKey(player)) scores[player] = 0;

        if (!playerSelection.ContainsKey(player)) playerSelection[player] = new List<CardData>();

        Canvas mainCanvas = FindObjectOfType<Canvas>();
        GameObject selectorObj = Instantiate(selectorPrefab, mainCanvas.transform);
        SelectorScript selector = selectorObj.GetComponent<SelectorScript>();
        selector.playerInput = player;
        var device = selector.playerInput.devices[0];
        selector.SetColor(PlayerManager.findColor(device).color);
    }

    public static void EndGame() {
        Debug.Log("FINISHED GAME");

    }
}
