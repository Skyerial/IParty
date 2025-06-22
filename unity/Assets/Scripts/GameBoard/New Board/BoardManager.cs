using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
public class BoardManager : MonoBehaviour
{
    public Transform tileGroup;      // Assign "Tile Groupe" in Inspector
    public Transform players;        // Assign player GameObject

    public int tileNr;

    private Rigidbody rb;
    private CapsuleCollider cap;
    private Animator animator;

    private InputAction jumpAction;
    private int round;

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
                int result = MovePlayerToTileMarker(player, 2, i);
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

   private int MovePlayerToTileMarker(GameObject player, int steps, int markerIndex = 0, float duration = 0.5f, float jumpHeight = 2f)
    {
        int tileIndex = tileNr + steps;
        bool finished = false;
        if (tileGroup == null || tileGroup.childCount <= tileIndex)
        {
            finished = true;
            tileIndex = tileGroup.childCount - 1;
        }

        Transform tile = tileGroup.GetChild(tileIndex);
        tileHandler tileScript = tile.GetComponent<tileHandler>();

        if (tileScript == null)
        {
            Debug.LogWarning("Tile at index " + tileIndex + " has no Tile script.");
            return -1;
        }

        Transform[] markers = tileScript.markers;

        if (markers == null || markers.Length <= markerIndex || markers[markerIndex] == null)
        {
            Debug.LogWarning("Invalid marker index or unassigned marker.");
            return -1;
        }

        Vector3 start = player.transform.position;
        Vector3 end = markers[markerIndex].position;
        end.y += 1;

        tileNr = tileIndex;

        playerAnimations playerScript = player.GetComponent<playerAnimations>();

        StartCoroutine(playerScript.rotate_and_jump(start, end));

        if (finished == true)
        {
            return 1;
        }
        return 0;
    }
}

