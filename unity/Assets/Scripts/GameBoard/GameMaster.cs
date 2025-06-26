using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

/**
 * @brief Manages player turns, dice rolling, and game state transitions.
 * Handles player camera switching and dice camera activation.
 * Coordinates player movements and triggers minigames after a full round.
 */


public class GameMaster : MonoBehaviour
{
    public int current_player = 0;
    public int change_player = 1;
    private List<GameObject> players = new List<GameObject>();
    public List<GameObject> Slots = new List<GameObject>();
    public List<GameObject> Dice = new List<GameObject>();
    public TextMeshProUGUI numberText;
    public TextMeshProUGUI turnText;
    public SwitchScene switchScene;

    public int press_random = 0;
    /**
    * @brief GameObject containing all tiles that make up the route
    */
    public Transform tileGroup;
    /**
    * @brief Bird GameObject used for flying the player from his tile to 5
    tiles further away.This happens when landing on special blue tiles.
    */
    public GameObject Bird;
    /**
    * @brief GameObject that appears when a minigame is selected randomly
    */
    public GameObject minigameSelector;
    public GameObject progressGroup;
    private swipe_menu menu;
    public bool numberShown = false;
    /**
    * @brief Starting Camera game object used to show a game board camera view
    at the start of the game (introduction camera)
    */
    public GameObject startingCam;
    private bool waitingForDice = false;
    /**
    * @brief Camera used when a dice is thrown
    */
    private Camera diceCam;
    private Dictionary<int, Slider> progressBars = new Dictionary<int, Slider>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ServerManager.SendtoAllSockets("mainboard");
        clearTurnText();
        clearDiceText();
        diceCam = GameObject.Find("DiceCamera").GetComponent<Camera>();
        GameObject.Find("DiceCamera").SetActive(false);
        AudioManager audioHandler = FindAnyObjectByType<AudioManager>();
        audioHandler.PlayRandomMiniGameTrack();
        if (PlayerManager.firstTimeBoard)
        {
            Debug.LogWarning("FIrst time board");
            StartingCameras sc = startingCam.GetComponent<StartingCameras>();
            sc.activateCamera(true);
            StartCoroutine(sc.SpiralCamera(afterCamera));
            PlayerManager.firstTimeBoard = false;
        }
        else
        {
            Debug.LogWarning("Second time board");
            afterCamera();
        }
    }

    /**
    * @brief This function runs after the spiralcamera in StartingCameras.cs
    * is called, it is called as a callback function.
    */
    void afterCamera()
    {
        Debug.LogWarning("afterCam");
        EnablePlayerCamera(current_player);
        updateTurnText();
    }

    // Update is called once per frame
    void Update()
    {
        if (current_player != change_player)
        {
            current_player = change_player;
            updateTurnText();
            EnablePlayerCamera(current_player);
        }

        if (press_random == 1 && !waitingForDice)
        {
            waitingForDice = true;
            EnableDiceCamera();
            clearTurnText();
            StartCoroutine(RollDiceForPlayer()); // Replace random logic with dice roll
        }
        if (press_random == 2 && players[current_player].GetComponent<PlayerMovement>().increment == 0)
        {
            clearDiceText();
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
            int throwResult = dice.GetComponent<DiceThrow>().SideUp();
            totalAmount += throwResult;
            updateDiceText(throwResult.ToString());
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
        activateProgressBar(device);
    }

    /**
    * @brief Function that causes the load of a random minigame and the switch
    of scene
    */
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

    /**
    * @brief Function that causes the player to move from its current position
    to the given tile.
    */
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

    /**
    * @brief Function that causes the player to move multiple steps, this involves
    calling MovePlayerToTileMarker multiple times (depending on the dice
    throw)
    */
    private IEnumerator MoveMultipleSteps(GameObject player, int steps)
    {
        for (int i = 0; i < steps; i++)
        {
            yield return StartCoroutine(MovePlayerToTileMarker(player, 1));
            if (player.GetComponent<PlayerMovement>().current_pos >= tileGroup.childCount - 1)
            {
                break;
            }
            updateProgressBar();
        }
        yield return StartCoroutine(landedTileHandler(player));
    }

    /**
    * @brief This is called after all steps by 1 player are taken. The landed
    tile is handled by checking the tile type and taking according action.
    */
    private IEnumerator landedTileHandler(GameObject player)
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
            yield return StartCoroutine(flyPlayer(player, player_nr));
            updateProgressBar();
        }
        // else if (tileScript.tileType == 1)
        // {
        //     scarePlayer(player, player_nr);
        // }

        press_random = 2;
        waitingForDice = false;
    }

    /**
    * @brief This is called if the tile type equals 2 indicating a blue tile
    that causes a bird to fly the player 5 tiles further.
    */
    private IEnumerator flyPlayer(GameObject player, int playerNr)
    {

        Transform child = player.transform.Find("Player - Image");
        yield return StartCoroutine(MoveAlongParabola(child.position, player, playerNr));
        var device = players[current_player].GetComponent<PlayerInput>().devices[0];
        PlayerManager.AddPosition(device, 5);
        player.GetComponent<PlayerMovement>().current_pos += 5;
    }

    /**
    * @brief This is called if the tile type equals 2 indicating a blue tile
    that causes a bird to fly the player 5 tiles further.
    */
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

    /**
    * @brief This is called if the tile type equals 2 indicating a blue tile
    that causes a bird to fly the player 5 tiles further.
    */
    private IEnumerator MoveAlongParabola(Vector3 middle, GameObject player, int playerNr)
    {
        float arcDuration = 1.5f;
        float arcHeight = 5f;
        var device = players[current_player].GetComponent<PlayerInput>().devices[0];
        int tile_nr = PlayerManager.playerStats[device].position;

        Vector3 end;
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

        PlayerMovement playerScript = player.GetComponent<PlayerMovement>();
        yield return StartCoroutine(playerScript.rotate(middle, end));

        Vector3 direction = end - middle;
        direction.y = 0f;
        Vector3 start = direction.normalized * -20f;
        start.y = 15;
        start = player.transform.position + start;
        GameObject bird = Instantiate(Bird);
        bird.transform.position = start;  // local offset behind player
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        bird.transform.localRotation = targetRotation;

        for (float t = 0; t < 1; t += Time.deltaTime / arcDuration)
        {
            Vector3 point = Vector3.Lerp(start, middle, t);
            point.y -= arcHeight * 4 * t * (1 - t); // Parabola: 4h * t(1 - t)

            bird.transform.position = point;
            yield return null;
        }

        float elapsed = 0f;
        StartCoroutine(playerScript.LinearMovement(middle, end, arcDuration));

        while (elapsed < arcDuration)
        {
            float t = elapsed / arcDuration;
            bird.transform.position = Vector3.Lerp(middle, end, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure final position
        bird.transform.position = end;
        Destroy(bird);
        yield return null;
        playerScript.makeFall();
        yield return new WaitForSeconds(3f);
    }

    /**
    * @brief This is called if the tile is the last one, meaning the game has
    finished. It handles the current player positions so that these can be
    represented on the podium in the winner scene.
    */
    private void finishGame(List<int> players, List<int> positions)
    {
        var paired = positions
            .Select((value, index) => new { Key = value, Value = players[index] })
            .OrderByDescending(pair => pair.Key)
            .ToList();

        var ranking = positions
        .Select((position, index) => new { Position = position, PlayerID = players[index] })
        .OrderByDescending(pair => pair.Position)
        .ToList();

        // Extract the sorted values back
        positions = paired.Select(p => p.Key).ToList();
        players = paired.Select(p => p.Value).ToList();
        List<int> rankedPlayerIDs = ranking.Select(p => p.PlayerID).ToList();
        PlayerManager.instance.rankGameboard = rankedPlayerIDs;
        switchScene.LoadNewScene("WinScreen3D");

    }

    // Finds and stores the progressbars.
    private void activateProgressBar(InputDevice device)
    {
        int id = PlayerManager.playerStats[device].playerID;
        int position = PlayerManager.playerStats[device].position;
        Material playerColor = PlayerManager.findColor(device);
        Transform barParent = progressGroup.transform.GetChild(id);

        // Setting the player color
        Debug.Log(barParent.name);
        Slider bar = barParent.Find("Slider").GetComponent<Slider>();
        Image handle = bar.transform.Find("Handle").GetComponent<Image>();
        handle.color = playerColor.color;

        // Adjusting the position
        Debug.Log(position / (float)tileGroup.childCount);
        bar.value = position / (float)tileGroup.childCount;

        // Storing the bar for easier access
        progressBars[id] = bar;
        barParent.gameObject.SetActive(true);
    }

    // Uses the stored progress bars
    private void updateProgressBar()
    {
        Slider bar = progressBars[current_player];
        int position = players[current_player].GetComponent<PlayerMovement>().current_pos;
        Debug.Log("The current position before updating is: " + position);
        bar.value = position / (float)tileGroup.childCount;
    }

    private void updateTurnText()
    {
        var device = players[current_player].GetComponent<PlayerInput>().devices[0];
        turnText.text = PlayerManager.playerStats[device].name + "'s turn";
    }

    private void clearTurnText()
    {
        turnText.text = "";
    }

    private void updateDiceText(string result)
    {
        numberText.text += result;
        Slots[numberText.text.Count() - 1].gameObject.SetActive(true);
    }

    private void clearDiceText()
    {
        foreach (var slot in Slots) slot.gameObject.SetActive(false);
        numberText.text = "";
    }
}
