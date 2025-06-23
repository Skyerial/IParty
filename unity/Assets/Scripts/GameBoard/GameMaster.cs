using System.Collections.Generic;
using System.Collections;
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
    public List<GameObject> players = new List<GameObject>();
    public List<GameObject> Dice = new List<GameObject>();
    public TextMeshProUGUI numberText;
    public int press_random = 0;
    public Transform tileGroup;

    public bool numberShown = false;
    private bool waitingForDice = false;
    private Camera diceCam;
    private Camera currentPlayerCam;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        diceCam = GameObject.Find("DiceCamera").GetComponent<Camera>();
        currentPlayerCam = players[current_player].GetComponentInChildren<Camera>(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (current_player != change_player)
        {
            current_player = change_player;
            EnablePlayerCamera(current_player);
        }

        if (press_random == 1 && !waitingForDice)
        {
            waitingForDice = true;
            EnableDiceCamera();
            StartCoroutine(RollDiceForPlayer()); // Replace random logic with dice roll
        }
        if (press_random == 2 && players[current_player].GetComponent<PlayerMovement>().increment == 0)
        {
            change_player = (current_player + 1) % players.Count;
            EnablePlayerCamera(current_player);
            press_random = 0;

            // If the variabele = 0 after updating it means a full round has been played.
            if (change_player == 0)
            {
                LoadRandomMinigame();
            }
        }
    }

    private IEnumerator RollDiceForPlayer(int diceIndex = 0)
    {
        // Position dice above current player
        int totalAmount = 0;
        // int diceResult = 0;
        var device = players[current_player].GetComponent<PlayerInput>().devices[0];
        int throws = PlayerManager.playerStats[device].winner;
        Debug.Log("Dice found!");

        for (int i = 0; i <= throws; i++)
        {
            GameObject dice = Dice[0]; // Use specified dice

            // Transforms the dice above the players head.
            // Vector3 playerPos = players[current_player].transform.position;
            // dice.transform.position = playerPos + Vector3.up * dice.GetComponent<DiceThrow>().heightAbovePlayer;

            // Throw the dice
            Debug.Log("Throwing the dice");
            dice.GetComponent<DiceThrow>().ThrowDice();

            // Wait for dice to settle
            while (!dice.GetComponent<DiceThrow>().DiceSettled())
            {
                yield return new WaitForSeconds(0.1f); // Check every 0.1 seconds
            }

            // Get the result and apply it
            totalAmount += dice.GetComponent<DiceThrow>().SideUp();
            numberText.text = totalAmount.ToString();
        }

        EnablePlayerCamera(current_player);

        // Moving the player here
        // players[current_player].GetComponent<PlayerMovement>().increment = totalAmount;
        StartCoroutine(MoveMultipleSteps(players[current_player], totalAmount));
        // Updating the players position in PlayerManager
        PlayerManager.AddPosition(device, totalAmount);
    }

    public void RegisterPlayer(PlayerInput playerInput)
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
        // int index = Random.Range(5, 9);
        // SceneManager.LoadScene("TankGame");
    }

    void EnablePlayerCamera(int player)
    {
        diceCam.gameObject.SetActive(false);
        for (int i = 0; i < players.Count; i++)
        {
            Camera cam = players[i].GetComponentInChildren<Camera>(true);
            cam.gameObject.SetActive(i == player);
        }
    }

    void EnableDiceCamera()
    {
        for (int i = 0; i < players.Count; i++)
        {
            Camera cam = players[i].GetComponentInChildren<Camera>(true);
            cam.gameObject.SetActive(false);
        }
        diceCam.gameObject.SetActive(true);
    }

    private IEnumerator MovePlayerToTileMarker(GameObject player, int steps, int markerIndex = 0, float duration = 0.5f, float jumpHeight = 2f)
    {   
        PlayerMovement playerScript = player.GetComponent<PlayerMovement>();
        int tileNr = playerScript.current_pos;
        int tileIndex = tileNr + steps;
        // bool finished = false;
        if (tileGroup == null || tileGroup.childCount <= tileIndex)
        {
            // finished = true;
            tileIndex = tileGroup.childCount - 1;
        }

        Transform tile = tileGroup.GetChild(tileIndex);
        tileHandler tileScript = tile.GetComponent<tileHandler>();

        if (tileScript == null)
        {
            Debug.LogWarning("Tile at index " + tileIndex + " has no Tile script.");
            yield break;
        }

        Transform[] markers = tileScript.markers;

        if (markers == null || markers.Length <= markerIndex || markers[markerIndex] == null)
        {
            Debug.LogWarning("Invalid marker index or unassigned marker.");
            yield break;
        }

        Vector3 start = player.transform.position;
        Vector3 end = markers[markerIndex].position;
        end.y += 1;

        playerScript.current_pos = tileIndex;

        yield return StartCoroutine(playerScript.rotate_and_jump(start, end));
    }

    private IEnumerator MoveMultipleSteps(GameObject player, int steps)
    {
        for (int i = 0; i < steps; i++)
        {
            yield return StartCoroutine(MovePlayerToTileMarker(player, 1));
        }
        press_random = 2;
        waitingForDice = false;
    }
}
