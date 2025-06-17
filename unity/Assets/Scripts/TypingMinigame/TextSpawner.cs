using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using System.Collections.Generic;


// words come at the player, player 'kills' the words
// every word is its own object, gives easier control over movement and animations per word
// counter # remaining on the right
// words hop on moving
// have two text boxes, one what the player types, one what the player needs to type, this way you can do checks and maybe color the right or wrong letters as well

public class TextSpawner : MonoBehaviour
{
    public List<string> spawnedWords = new List<string>();

    public int words;

    public Transform spawner;

    private string[] wordList = new string[]
    {
    // 2-letter (5)
    "go", "up", "it", "no", "am",

    // 3-letter (15)
    "cat", "dog", "run", "sun", "box", "pen", "man", "bag", "red", "cup",
    "map", "top", "hat", "toy", "car",

    // 4-letter (30)
    "game", "play", "tree", "book", "fish", "door", "walk", "stop", "look", "jump",
    "cold", "warm", "milk", "baby", "bird", "hand", "sand", "rock", "road", "frog",
    "rain", "fire", "wind", "time", "fast", "slow", "blue", "gold", "ball", "ring",

    // 5-letter (25)
    "apple", "water", "light", "chair", "mouse", "happy", "house", "plant", "smile", "grass",
    "bread", "drink", "table", "sweet", "sleep", "crash", "clean", "cloud", "small", "brick",
    "grape", "snake", "truck", "brain", "sugar",

    // 6-letter (15)
    "banana", "people", "family", "yellow", "circle", "orange", "window", "animal", "button", "letter",
    "cookie", "planet", "friend", "rocket", "bubble",

    // 7-letter (7)
    "picture", "morning", "holiday", "monster", "teacher", "sandbox", "blanket",

    // 8-letter (3)
    "building", "elephant", "notebook"
    };
    // {
    //     "apple", "banana", "grape", "orange", "melon",
    //     "cloud", "stream", "code", "unity", "keyboard",
    //     "game", "level", "score", "player", "speed"
    // };
    // {
    //     "a"
    // };

    public void SpawnWords()
    {
        spawnedWords.Clear(); // Clear if previously used

        TextMeshProUGUI textPlayer1 = GameObject.Find("SampleTextMeshPro").GetComponent<TextMeshProUGUI>();

        for (int word = 0; word < words; word++)
        {
            TextMeshProUGUI obj = Instantiate(textPlayer1, spawner);
            obj.name = "Child_" + word;
            string randomWord = wordList[Random.Range(0, wordList.Length)];
            obj.text = randomWord;
            spawnedWords.Add(randomWord);
        }
    }
}
