using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
public class BoardManager : MonoBehaviour
{
    public Transform tileGroup;      // Assign "Tile Groupe" in Inspector
    public Transform players;        // Assign player GameObject

    public int tileNr;
    public GameObject Bird;
    public GameObject Ants;
    public float arcHeight = 5f;           // Arc height
    public float arcDuration = 1.5f;       // Time to reach the target

    private Rigidbody rb;
    private CapsuleCollider cap;
    private Animator animator;

    private InputAction jumpAction;
    private int round;
    private int[] myPlaces = { 0 };

    void Awake()
    {
        // get InputActions once
        var pi = GetComponent<PlayerInput>();
        jumpAction = pi.actions["Jump"];
        int nr_players = players.childCount;
        for (int i = 0; i < nr_players; i++)
        {
            GameObject player = players.GetChild(i).gameObject;
            playerAnimations playerScript = player.GetComponent<playerAnimations>();
            playerScript.ActivateCamera(false);
        }
        Debug.LogWarning("Deactivated all cameras");
        StartCoroutine(gameLoop());
    }

    private IEnumerator gameLoop()
    {
        round = 1;
        int game_over = 0;
        int nr_players = players.childCount;
        while (game_over != 1)
        {
            for (int i = 0; i < nr_players; i++)
            {
                GameObject player = players.GetChild(i).gameObject;
                playerAnimations playerScript = player.GetComponent<playerAnimations>();
                playerScript.ActivateCamera(true);
                yield return WaitForJumpInput();
                myPlaces[i] = myPlaces[i] + 1;
                int result = MovePlayerToTileMarker(i, 1, i);

                playerScript.ActivateCamera(false);
            }
            Debug.Log("Round Played:" + round);
            round++;
        }
    }
    private IEnumerator WaitForJumpInput()
    {
        bool jumpPressed = false;

        void OnJump(InputAction.CallbackContext ctx)
        {
            jumpPressed = true;
        }

        jumpAction.performed += OnJump;

        // Wait until jump is pressed
        while (!jumpPressed)
        {
            yield return null; // wait one frame
        }

        jumpAction.performed -= OnJump; // Unsubscribe when done
    }

    private int MovePlayerToTileMarker(int player_nr, int steps, int markerIndex = 0, float duration = 0.5f, float jumpHeight = 2f)
    {
        GameObject player = players.GetChild(player_nr).gameObject;
        int tileIndex = tileNr + steps;
        bool finished = false;
        if (tileGroup == null || tileGroup.childCount <= tileIndex)
        {
            finished = true;
            tileIndex = tileGroup.childCount - 1;
        }


        Vector3 start = player.transform.position;
        Vector3 end = getTileMarkerPos(tileIndex, markerIndex);
        end.y += 1;

        tileNr = tileIndex;

        StartCoroutine(JumpAndDoAfter(start, end, player, player_nr));

        if (finished == true)
        {
            return 1;
        }
        return 0;
    }

    private IEnumerator JumpAndDoAfter(Vector3 start, Vector3 end, GameObject player, int player_nr)
    {
        playerAnimations playerScript = player.GetComponent<playerAnimations>();
        yield return StartCoroutine(playerScript.rotate_and_jump(start, end));
        landedTileHandler(player, player_nr);
    }

    private void landedTileHandler(GameObject player, int player_nr)
    {
        int tile_nr = myPlaces[player_nr];
        Transform tile = tileGroup.GetChild(tile_nr);
        tileHandler tileScript = tile.GetComponent<tileHandler>();
        if (tileScript.tileType == 2)
        {
            flyPlayer(player, player_nr);
        }
        else if (tileScript.tileType == 1)
        {
            scarePlayer(player, player_nr);
        }
    }

    private void flyPlayer(GameObject player, int playerNr)
    {
        Vector3 offset = new Vector3(0, 15, -20); // e.g., 2 units above the player

        // Calculate spawn position relative to the player
        Vector3 spawnPosition = player.transform.position + offset;
        GameObject newBird = Instantiate(Bird, spawnPosition, Quaternion.identity, player.transform);
        StartCoroutine(MoveAlongParabola(spawnPosition, player.transform.position, newBird, player, playerNr));

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

        for (float t = 0; t < 1; t += Time.deltaTime / arcDuration)
        {
            Vector3 point = Vector3.Lerp(start, end, t);
            point.y -= arcHeight * 4 * t * (1 - t); // Parabola: 4h * t(1 - t)

            bird.transform.position = point;
            yield return null;
        }

        transform.position = end;
        Vector3 oldEnd = end;
        if (myPlaces[playerNr] + 5 >= tileGroup.childCount)
        {
            myPlaces[playerNr] = tileGroup.childCount - 1;
        }
        else
        {
            myPlaces[playerNr] = myPlaces[playerNr] + 5;
        }
        end = getTileMarkerPos(myPlaces[playerNr], playerNr);
        end = new Vector3(end.x, end.y + 20, end.z);

        float elapsed = 0f;

        playerAnimations playerScript = player.GetComponent<playerAnimations>();
        StartCoroutine(playerScript.LinearMovement(oldEnd, end, arcDuration));

        while (elapsed < arcDuration)
        {
            float t = elapsed / arcDuration;
            bird.transform.position = Vector3.Lerp(oldEnd, end, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure final position
        transform.position = end;
        Destroy(bird);
        playerScript.makeFall();
    }

    private void scarePlayer(GameObject player, int playerNr)
    {
        int tileNr = myPlaces[playerNr]; // e.g., 2 units above the player

        // Calculate spawn position relative to the player
        Vector3 spawnPosition = getTileMarkerPos(tileNr + 1, 4);
        spawnPosition = new Vector3(spawnPosition.x, spawnPosition.y + 20, spawnPosition.z);
        GameObject newAnts = Instantiate(Ants, spawnPosition, Quaternion.identity, player.transform);
    }

}

