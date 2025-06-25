using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.InputSystem;

public class GridScript : MonoBehaviour {
    public static GridScript gridScript;
    public GameObject cardPrefab;
    public static List<SetCard> cardObjects = new List<SetCard>();

    void Awake() {
        gridScript = this;
        Debug.Log("Gird Awake");
    }

    // Moves 12 cards from the allCards to the DealtCards list.
    // Makes sure the dealt card contain a valid set,
    // if there are still cards available.
    public static void DealCards() {
        Debug.Log("Dealing Cards");
        for (int i = 0; SetGameManager.dealtCards.Count < 12; i++) {
            if (SetGameManager.allCards.Count <= 0) {
                break;
            }

            SetGameManager.dealtCards.Add(SetGameManager.allCards[0]);
            SetGameManager.allCards.RemoveAt(0);
        }

        if (SetGameManager.allCards.Count == 0 && !SetGameManager.hasSet()) {
            SetGameManager.EndGame();
        }

        while (SetGameManager.allCards.Count > 0 && !SetGameManager.hasSet()) {
            int i = Random.Range(0, SetGameManager.allCards.Count);
            int j = Random.Range(0, SetGameManager.dealtCards.Count);
            CardData temp = SetGameManager.allCards[i];
            SetGameManager.allCards[i] = SetGameManager.dealtCards[j];
            SetGameManager.dealtCards[j] = temp;
        }
    }

    // Initializes the cards in the DealtCards list.
    public void CreateCardGrid() {
        Debug.Log("Creating Grid");
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < SetGameManager.dealtCards.Count; i++) {
            GameObject cardGO = Instantiate(cardPrefab, transform);
            SetCard setCard = cardGO.GetComponent<SetCard>();
            CardData data = SetGameManager.dealtCards[i];
            setCard.Initialize(data);
            cardObjects.Add(setCard);
        }
    }

    // Adds or removes the selected ard from the selected list for that player,
    // depending on if it's already present or not.
    //
    // If the selectedSet list reaches 3 elements it checks if this is a valid set,
    // adding a point to the score when it is valid, and removing the cards from the dealtCards.
    // It clears the selectedCard list whenever it reaches 3 elements.
    public static void CardSelected(SetCard selected, PlayerInput player) {
        Debug.Log("Selected a Card");
        CardData selectedCard = selected.cardData;
        if (SetGameManager.playerSelection[player].Contains(selectedCard)) {
            SetGameManager.playerSelection[player].Remove(selectedCard);
        } else {
            SetGameManager.playerSelection[player].Add(selectedCard);
            if (SetGameManager.playerSelection[player].Count == 3) {
                if (SetGameManager.isSet(SetGameManager.playerSelection[player][0],
                                         SetGameManager.playerSelection[player][1],
                                         SetGameManager.playerSelection[player][2]))
                {
                    SetGameManager.scores[player]++;
                    SetGameManager.dealtCards.Remove(SetGameManager.playerSelection[player][0]);
                    SetGameManager.dealtCards.Remove(SetGameManager.playerSelection[player][1]);
                    SetGameManager.dealtCards.Remove(SetGameManager.playerSelection[player][2]);
                }

                foreach (PlayerInput p in SetGameManager.gamePlayers) {
                    SetGameManager.playerSelection[p].Clear();
                }
            }
        }

        DealCards();
        gridScript.CreateCardGrid();
    }
}
