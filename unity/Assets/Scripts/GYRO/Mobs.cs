using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobManager : MonoBehaviour
{
    [System.Serializable]
    public class MobEntry
    {
        public MonoBehaviour mobScript;
        [Range(0f, 1f)]
        public float spawnWeight = 0.5f;
    }

    [Tooltip("Weighted list of mob entries")]
    public List<MobEntry> mobEntries = new List<MobEntry>();

    [Tooltip("Minimum time between spawns")]
    public float minSpawnDelay = 0.5f;

    [Tooltip("Maximum time between spawns")]
    public float maxSpawnDelay = 2f;


    private MonoBehaviour currentActiveMob;

    void Start()
    {
        StartCoroutine(PopLoop());
    }

    IEnumerator PopLoop()
    {
        while (true)
        {
            float delay = Random.Range(minSpawnDelay, maxSpawnDelay);
            yield return new WaitForSeconds(delay);

            currentActiveMob = GetWeightedRandomMob();

            if (currentActiveMob is Mole mole)
            {
                yield return StartCoroutine(mole.PopCycle());
            }
            else if (currentActiveMob is BombGyro bomb)
            {
                yield return StartCoroutine(bomb.PopCycle());
            }
            else if (currentActiveMob is OilBarrel oil)
            {
                yield return StartCoroutine(oil.PopCycle());
            }

        }
    }

    private MonoBehaviour GetWeightedRandomMob()
    {
        float totalWeight = 0f;
        foreach (var entry in mobEntries)
            totalWeight += entry.spawnWeight;

        float rand = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (var entry in mobEntries)
        {
            cumulative += entry.spawnWeight;
            if (rand <= cumulative)
                return entry.mobScript;
        }

        // Fallback in case of rounding errors
        return mobEntries[Random.Range(0, mobEntries.Count)].mobScript;
    }
}
