using UnityEngine;
using System.Collections.Generic;

public class BombManager : MonoBehaviour
{
    public GameObject bombPrefab;
    private GameObject currentBomb;
    public List<GameObject> players = new List<GameObject>();

    void Start()
    {
        SpawnBombOnRandomPlayer();
    }
    void Update()
    {
        players.RemoveAll(p => p == null);

        if (players.Count == 1)
        {
            Debug.Log("Gameover: winner " + players[0].name);
            return;
        }

        if (currentBomb == null && players.Count > 1)
        {
            SpawnBombOnRandomPlayer();
        }

    }
    void SpawnBombOnRandomPlayer()
    {
        int index = Random.Range(0, players.Count);
        GameObject selectedPlayer = players[index];

        currentBomb = Instantiate(bombPrefab);
        currentBomb.transform.SetParent(selectedPlayer.transform);
        currentBomb.transform.localPosition = new Vector3(0, 2f, 0f);
        currentBomb.transform.localRotation = Quaternion.identity;
    }
    
    public GameObject GetCurrentBomb()
    {
        return currentBomb;
    }
}
