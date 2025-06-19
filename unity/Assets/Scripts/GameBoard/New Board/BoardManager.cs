using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider), typeof(PlayerInput))]
[ExecuteInEditMode]
public class BoardManager : MonoBehaviour
{
    public Transform tileGroup;      // Assign "Tile Groupe" in Inspector
    public GameObject player_test;        // Assign player GameObject

    public int tileNr;

    private Rigidbody rb;
    private CapsuleCollider cap;
    private Animator animator;

    private InputAction jumpAction;


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        cap = GetComponent<CapsuleCollider>();
        animator = GetComponent<Animator>();

        // get InputActions once
        var pi = GetComponent<PlayerInput>();
        jumpAction = pi.actions["Jump"];
        jumpAction.performed += _ => MovePlayerToTileMarker(player_test, 1, 4);
    }

    public void MovePlayerToTileMarker(GameObject player, int steps, int markerIndex = 0, float duration = 0.5f, float jumpHeight = 2f)
    {
        int tileIndex = tileNr + steps;
        if (tileGroup == null || tileGroup.childCount <= tileIndex)
        {
            Debug.LogWarning("Tile index out of range or tileGroup not set.");
            return;
        }

        Transform tile = tileGroup.GetChild(tileIndex);
        tileHandler tileScript = tile.GetComponent<tileHandler>();

        if (tileScript == null)
        {
            Debug.LogWarning("Tile at index " + tileIndex + " has no Tile script.");
            return;
        }

        Transform[] markers = tileScript.markers;

        if (markers == null || markers.Length <= markerIndex || markers[markerIndex] == null)
        {
            Debug.LogWarning("Invalid marker index or unassigned marker.");
            return;
        }

        Vector3 start = player.transform.position;
        Vector3 end = markers[markerIndex].position;

        // Start the jump coroutine
        tileNr = tileIndex;
        StartCoroutine(JumpArc(player.transform, start, end, duration, jumpHeight));
    }

    private IEnumerator JumpArc(Transform target, Vector3 start, Vector3 end, float duration, float height)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;

            // Basic parabolic arc formula: h * 4(t - t^2)
            float arc = height * 4 * (t - t * t);
            Vector3 currentPos = Vector3.Lerp(start, end, t);
            currentPos.y += arc;

            target.position = currentPos;

            elapsed += Time.deltaTime;
            yield return null;
        }

        target.position = end; // Final snap to ground
    }
}

