using JetBrains.Annotations;
using TMPro;
using UnityEngine;


// words come at the player, player 'kills' the words
// every word is its own object, gives easier control over movement and animations per word
// counter # remaining on the right
// words hop on moving
// have two text boxes, one what the player types, one what the player needs to type, this way you can do checks and maybe color the right or wrong letters as well


public class TextSpawner : MonoBehaviour
{

    public Transform spawner;

    private string[] wordList = new string[]
    {
        "apple", "banana", "grape", "orange", "melon",
        "cloud", "stream", "code", "unity", "keyboard",
        "game", "level", "score", "player", "speed"
    };
    // {
    //     "a"
    // };

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        TextMeshProUGUI textPlayer1 = GameObject.Find("SampleTextMeshPro").GetComponent<TextMeshProUGUI>();

        for (int word = 0; word < 10; word++)
        {
            TextMeshProUGUI obj = Instantiate(textPlayer1, spawner);
            obj.name = "Child_" + word;
            string randomWord = wordList[Random.Range(0, wordList.Length)];
            obj.text = randomWord;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
