using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameMaster : MonoBehaviour
{
    public int current_player = -1;
    public int change_player = 0;
    static public List<GameObject> players = new List<GameObject>();
    public TextMeshProUGUI numberText;
    public int press_random = 0;

    public bool numberShown = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (current_player != change_player)
        {
            current_player = change_player;
            for (int i = 0; i < players.Count; i++)
            {
                // Get the Camera component from the child object
                Debug.Log(players[i]);
                Camera cam = players[i].GetComponentInChildren<Camera>(true); // true = include inactive
                if (cam != null)
                {
                    cam.gameObject.SetActive(i == current_player);
                }
            }
        }

        if (press_random == 1 && !numberShown)
        {
            int randomNumber = Random.Range(1, 6); // change range as needed
            //numberText.text = randomNumber.ToString();
            //numberShown = true; // Prevent multiple updates
            // Debug.Log(players[current_player]);
            // Debug.Log(players[current_player].GetComponent<PlayerManager>());
            players[current_player].GetComponent<PlayerMovement>().increment = randomNumber;
            numberText.text = randomNumber.ToString();
            press_random = 2;
        }
        if (press_random == 2 && players[current_player].GetComponent<PlayerMovement>().increment == 0)
        {
            change_player = (current_player + 1) % players.Count;
            press_random = 0;

            // If the variabele = 0 after updating it means a full round has been played.
            if (change_player == 0)
            {
                LoadRandomMinigame();
            }
        }
    }

    public static void RegisterPlayer(PlayerInput playerInput)
    {
        Debug.Log(playerInput.gameObject.name);
        var device = playerInput.devices[0];
        int index = PlayerManager.playerStats[device].playerID;
        Debug.Log(index);
        players.Insert(index, playerInput.gameObject);
    }

    void LoadRandomMinigame()
    {
        Debug.Log("Loading random minigame...");
        int index = Random.Range(5, 9);
        SceneManager.LoadScene(index);
    }
}
