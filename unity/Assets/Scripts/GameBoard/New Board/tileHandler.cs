using UnityEngine;

public class tileHandler : MonoBehaviour
{
    public Transform[] markers; // Should have 4 markers assigned automatically
    public bool autoLink = true;
    public int marker_nr;
    public GameObject player;
    private void OnValidate()
    {
        if (autoLink)
        {
            assignMarkers();
        }
    }

    private void assignMarkers()
    {
        // Auto-assign markers if not manually set
        if (markers == null || markers.Length == 0)
        {
            markers = new Transform[5];

            for (int i = 0; i < 5; i++)
            {
                Transform foundMarker = transform.Find($"Marker{i}"); // "Marker1", "Marker2", etc.
                if (foundMarker != null)
                {
                    markers[i] = foundMarker;
                }
                else
                {
                    Debug.LogWarning($"Marker{i + 1} not found on tile: {gameObject.name}");
                }
            }
        }
    }

    public void MovePlayerToMarker(GameObject player, int markerIndex = 0)
    {
        // Debug.Log(PlayerManager)
        if (markers != null && markers.Length > markerIndex && markers[markerIndex] != null)
        {
            Vector3 playerPosition = player.transform.position;
            float targetX = markers[markerIndex].position.x;
            float targetZ = markers[markerIndex].position.z;

            player.transform.position = new Vector3(targetX, playerPosition.y, targetZ);
        }
        else
        {
            Debug.LogWarning("Invalid marker index or unassigned marker.");
        }
    }
}
