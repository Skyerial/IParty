using UnityEngine;

/**
 * @brief handles a single tile. This means the different markers on the tile
 * as well as the tile type.
 */
public class tileHandler : MonoBehaviour
{

    /**
    * @brief public list containing all markers on the tile
    */
    public Transform[] markers; // Should have x markers assigned automatically

    /**
    * @brief boolean that can be switched off and on again to reload the
    * tile handler (it will reassigns markers)
    */
    public bool autoLink = true;

    /**
    * @brief public variable that can be used by tilegroup.cs to give material
    * accordingly
    */
    public int tileType;

    /**
    * @brief function called upon inspector changes in edit mode. This function
    * causes tiles to have proper markers
    */
    private void OnValidate()
    {
        if (autoLink)
        {
            assignMarkers();
        }
    }

    /**
    * @brief This function renames the markers on the object and adds them
    * to a script datastructure
    */
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
}
