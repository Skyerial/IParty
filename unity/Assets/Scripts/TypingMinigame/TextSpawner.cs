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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        TextMeshPro textPlayer1 = GameObject.Find("SampleText").GetComponent<TextMeshPro>();
        textPlayer1.text = "testing words";

        for (int word = 0; word < 5; word++)
        {
            TextMeshPro obj = Instantiate(textPlayer1, spawner);
            obj.name = "Child_" + word;
            obj.text = "test" + word;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
