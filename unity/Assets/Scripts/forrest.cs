using UnityEngine;

public class ForestGenerator : MonoBehaviour
{
    public int terrainWidth = 512;
    public int terrainLength = 512;
    public int terrainHeight = 100;

    public int treeCount = 500;
    public GameObject[] treePrefabs;

    void Start()
    {
        // Create terrain
        TerrainData terrainData = new TerrainData
        {
            heightmapResolution = 513,
            size = new Vector3(terrainWidth, terrainHeight, terrainLength)
        };

        GameObject terrainGO = Terrain.CreateTerrainGameObject(terrainData);
        Terrain terrain = terrainGO.GetComponent<Terrain>();

        // Random hills
        float[,] heights = new float[terrainData.heightmapResolution, terrainData.heightmapResolution];
        for (int x = 0; x < terrainData.heightmapResolution; x++)
        {
            for (int y = 0; y < terrainData.heightmapResolution; y++)
            {
                heights[x, y] = Mathf.PerlinNoise(x * 0.01f, y * 0.01f) * 0.1f;
            }
        }
        terrainData.SetHeights(0, 0, heights);

        // Spawn trees
        for (int i = 0; i < treeCount; i++)
        {
            Vector3 position = new Vector3(
                Random.Range(0, terrainWidth),
                0,
                Random.Range(0, terrainLength)
            );
            float terrainHeightAtPos = terrain.SampleHeight(position);
            position.y = terrainHeightAtPos;

            GameObject tree = Instantiate(
                treePrefabs[Random.Range(0, treePrefabs.Length)],
                position,
                Quaternion.Euler(0, Random.Range(0, 360), 0)
            );
            tree.transform.SetParent(terrainGO.transform);
        }
    }
}
