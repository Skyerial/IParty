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
        Debug.Log("Length of dealtCards " + SetGameManager.dealtCards.Count);
        Debug.Log("Length of cardObjects " + cardObjects.Count);
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

        Debug.Log("Length of dealtCards " + SetGameManager.dealtCards.Count);
        Debug.Log("Length of cardObjects " + cardObjects.Count);
    }

    // Adds or removes the selected ard from the selected list for that player,
    // depending on if it's already present or not.
    //
    // If the selectedSet list reaches 3 elements it checks if this is a valid set,
    // adding a point to the score when it is valid, and removing the cards from the dealtCards.
    // It clears the selectedCard list whenever it reaches 3 elements.
    public static void CardSelected(SetCard selected, PlayerInput player) {
        CardData selectedCard = selected.cardData;
        Debug.Log("Card properties shape = " + selectedCard.shape);
        Debug.Log("Card properties color = " + selectedCard.color);
        Debug.Log("Card properties number = " + selectedCard.number);
        Debug.Log("Card properties filling = " + selectedCard.filling);
        if (SetGameManager.playerSelection[player].Contains(selected)) {
            Debug.Log("Unselected a Card");
            SetGameManager.playerSelection[player].Remove(selected);
        } else {
            Debug.Log("Selected a Card");
            SetGameManager.playerSelection[player].Add(selected);
            if (SetGameManager.playerSelection[player].Count >= 3) {
                if (SetGameManager.playerSelection[player].Count == 3 &&
                    SetGameManager.isSet(SetGameManager.playerSelection[player][0].cardData,
                                         SetGameManager.playerSelection[player][1].cardData,
                                         SetGameManager.playerSelection[player][2].cardData))
                {
                    Debug.Log("YOU FOUND A SET!!!");
                    SetGameManager.scores[player]++;

                    var card1 = SetGameManager.playerSelection[player][0];
                    SetGameManager.dealtCards.Remove(card1.cardData);
                    cardObjects.Remove(card1);

                    var card2 = SetGameManager.playerSelection[player][1];
                    SetGameManager.dealtCards.Remove(card2.cardData);
                    cardObjects.Remove(card2);

                    var card3 = SetGameManager.playerSelection[player][2];
                    SetGameManager.dealtCards.Remove(card3.cardData);
                    cardObjects.Remove(card3);

                    foreach (PlayerInput p in SetGameManager.gamePlayers) {
                        SetGameManager.playerSelection[p].Clear();
                    }

                    DealCards();
                    gridScript.CreateCardGrid();
                }
                else
                {
                    SetGameManager.playerSelection[player].Clear();
                    Debug.Log("You didn't find a set.");
                }
            }
        }
    }
}
