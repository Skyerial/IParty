using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

/**
 * @brief Manages player turns, dice rolling, and game state transitions.
 * Handles player camera switching and dice camera activation.
 * Coordinates player movements and triggers minigames after a full round.
 */


public class GameMaster : MonoBehaviour
{
    public int current_player = -1;
    public int change_player = 0;
    public List<GameObject> players = new List<GameObject>();
    public List<GameObject> Dice = new List<GameObject>();
    public TextMeshProUGUI numberText;
    public int press_random = 0;
    public Transform tileGroup;
    public GameObject Bird;

    public GameObject minigameSelector;
    private swipe_menu menu;


    public bool numberShown = false;
    private bool waitingForDice = false;
    private Camera diceCam;
    private Camera currentPlayerCam;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        diceCam = GameObject.Find("DiceCamera").GetComponent<Camera>();
        currentPlayerCam = players[current_player].GetComponentInChildren<Camera>(true);
        AudioManager audioHandler = FindAnyObjectByType<AudioManager>();
        audioHandler.PlayRandomMiniGameTrack();
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
        // Updating the players position in PlayerManager
        PlayerManager.AddPosition(device, totalAmount);
        StartCoroutine(MoveMultipleSteps(players[current_player], totalAmount));
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
        menu = minigameSelector.GetComponentInChildren<swipe_menu>();

        if (menu == null)
        {
            Debug.LogError("Swipe menu script not found on minigameSelector!");
            return;
        }

        minigameSelector.SetActive(true);
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
        landedTileHandler(player);
        press_random = 2;
        waitingForDice = false;
    }
    private void landedTileHandler(GameObject player)
    {
        var device = players[current_player].GetComponent<PlayerInput>().devices[0];
        int tile_nr = PlayerManager.playerStats[device].position;
        if (tile_nr >= tileGroup.childCount - 1)
        {
            List<int> positions = new List<int>();
            List<int> players_list = new List<int>();
            foreach (KeyValuePair<UnityEngine.InputSystem.InputDevice, PlayerManager.PlayerStats> pair in PlayerManager.playerStats)
            {
                players_list.Add(pair.Value.playerID);
                positions.Add(pair.Value.position);
            }
            finishGame(players_list, positions);
        }
        int player_nr = PlayerManager.playerStats[device].playerID;
        Transform tile = tileGroup.GetChild(tile_nr);
        tileHandler tileScript = tile.GetComponent<tileHandler>();
        if (tileScript.tileType == 2)
        {
            flyPlayer(player, player_nr);
        }
        // else if (tileScript.tileType == 1)
        // {
        //     scarePlayer(player, player_nr);
        // }
    }

    private void flyPlayer(GameObject player, int playerNr)
    {
        Vector3 offset = new Vector3(0, 15, -20); // e.g., 2 units above the player

        // Calculate spawn position relative to the player
        Vector3 spawnPosition = player.transform.position + offset;
        GameObject newBird = Instantiate(Bird, spawnPosition, Quaternion.identity, player.transform);
        StartCoroutine(MoveAlongParabola(spawnPosition, player.transform.position, newBird, player, playerNr));
        var device = players[current_player].GetComponent<PlayerInput>().devices[0];
        PlayerManager.AddPosition(device, 5);
        player.GetComponent<PlayerMovement>().current_pos += 5;
    }

    private Vector3 getTileMarkerPos(int tileNr, int marker_nr)
    {
        Transform tile = tileGroup.GetChild(tileNr);
        tileHandler tileScript = tile.GetComponent<tileHandler>();

        if (tileScript == null)
        {
            Debug.LogWarning("Tile at index " + tileNr + " has no Tile script.");
            return Vector3.negativeInfinity;
        }

        Transform[] markers = tileScript.markers;

        if (markers == null || markers.Length <= marker_nr || markers[marker_nr] == null)
        {
            Debug.LogWarning("Invalid marker index or unassigned marker.");
            return Vector3.negativeInfinity;
        }
        return markers[marker_nr].position;
    }

    private IEnumerator MoveAlongParabola(Vector3 start, Vector3 end, GameObject bird, GameObject player, int playerNr)
    {
        float arcDuration = 1.5f;
        float arcHeight = 5f;
        var device = players[current_player].GetComponent<PlayerInput>().devices[0];
        int tile_nr = PlayerManager.playerStats[device].position;

        for (float t = 0; t < 1; t += Time.deltaTime / arcDuration)
        {
            Vector3 point = Vector3.Lerp(start, end, t);
            point.y -= arcHeight * 4 * t * (1 - t); // Parabola: 4h * t(1 - t)

            bird.transform.position = point;
            yield return null;
        }

        bird.transform.position = end;
        Vector3 oldEnd = end;
        if (tile_nr + 5 >= tileGroup.childCount)
        {
            tile_nr = tileGroup.childCount - 1;
        }
        else
        {
            tile_nr = tile_nr + 5;
        }
        end = getTileMarkerPos(tile_nr, playerNr);
        end = new Vector3(end.x, end.y + 20, end.z);

        float elapsed = 0f;

        PlayerMovement playerScript = player.GetComponent<PlayerMovement>();
        StartCoroutine(playerScript.LinearMovement(oldEnd, end, arcDuration));

        while (elapsed < arcDuration)
        {
            float t = elapsed / arcDuration;
            bird.transform.position = Vector3.Lerp(oldEnd, end, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure final position
        bird.transform.position = end;
        Destroy(bird);
        playerScript.makeFall();
    }

    private void finishGame(List<int> players, List<int> positions)
    {
        var paired = positions
            .Select((value, index) => new { Key = value, Value = players[index] })
            .OrderBy(pair => pair.Key) // sort by positions values
            .ToList();

        // Extract the sorted values back
        positions = paired.Select(p => p.Key).ToList();
        players = paired.Select(p => p.Value).ToList();
        Debug.Log(positions);
        Debug.Log(players);
    }
}
