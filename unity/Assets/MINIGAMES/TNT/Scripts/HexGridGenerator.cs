// HexGridGenerator.cs
// Generates a flat-topped hexagonal grid within a circular radius.

using UnityEngine;

public class HexGridGenerator : MonoBehaviour
{
    [Header("Circle Fill Settings")]
    [Tooltip("Circle radius in world units.")]
    public float fillRadius = 10f;

    [Header("Hexagon Settings")]
    [Tooltip("Prefab of a hex tile; must have HexTile.cs attached.")]
    public GameObject hexTilePrefab;

    [Tooltip("Distance from hexagon center to any vertex.")]
    public float hexRadius = 1f;

    [Header("Spacing (Gap between tiles)")]
    [Tooltip("Extra distance (world units) between edges of adjacent hexes.")]
    public float spacing = 0.1f;

    [Header("Height Settings")]
    [Tooltip("Y-position at which to spawn all tiles.")]
    public float baseHeight = 2f;

    private void Start()
    {
        if (hexTilePrefab == null)
        {
            Debug.LogError("HexGridGenerator: No hexTilePrefab assigned!");
            enabled = false;
            return;
        }

        // Compute hex dimensions for a flat-topped layout
        float hexWidth  = hexRadius * 2f;                 // From leftmost to rightmost vertex
        float hexHeight = Mathf.Sqrt(3f) * hexRadius;     // From top to bottom vertex

        // Calculate center-to-center spacing with extra gap
        float horizontalSpacing = (hexWidth * 0.75f) + spacing;
        float verticalSpacing   = hexHeight + spacing;

        // Determine the max columns/rows needed to cover the circle
        int maxCols = Mathf.CeilToInt(fillRadius / horizontalSpacing);
        int maxRows = Mathf.CeilToInt(fillRadius / verticalSpacing);

        // Preserve any rotation that the prefab originally had
        Quaternion prefabRotation = hexTilePrefab.transform.rotation;

        // Loop through a square grid that fully encloses the circle
        for (int col = -maxCols; col <= maxCols; col++)
        {
            for (int row = -maxRows; row <= maxRows; row++)
            {
                // Compute X position of this column
                float xPos = col * horizontalSpacing;

                // In a flat-topped grid, odd columns are offset 0.5 row down
                float zOffset = (Mathf.Abs(col) % 2 == 1) ? (verticalSpacing * 0.5f) : 0f;
                float zPos    = row * verticalSpacing + zOffset;

                // Only instantiate if inside the circle of radius 'fillRadius'
                Vector2 flatPos = new Vector2(xPos, zPos);
                if (flatPos.magnitude <= fillRadius)
                {
                    Vector3 worldPos = new Vector3(xPos, baseHeight, zPos);
                    GameObject hexGO = Instantiate(
                        hexTilePrefab,
                        worldPos,
                        prefabRotation,
                        transform // parent under this generator
                    );

                    hexGO.name = $"Hex_{col}_{row}";
                    // The HexTile component on the prefab handles falling/respawning
                }
            }
        }
    }
}
