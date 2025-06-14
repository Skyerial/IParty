using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobManager : MonoBehaviour
{
    [System.Serializable]
    public class MobEntry
    {
        public MonoBehaviour mobScript; // Mole or Bomb
        [Range(0f, 1f)]
        public float spawnWeight = 0.5f;
    }

    [Tooltip("Weighted list of mob entries")]
    public List<MobEntry> mobEntries = new List<MobEntry>();

    [Tooltip("Delay between each mob pop-up")]
    public float spawnDelay = 1.5f;

    private MonoBehaviour currentActiveMob;

    void Start()
    {
        StartCoroutine(PopLoop());
    }

    IEnumerator PopLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnDelay);

            currentActiveMob = GetWeightedRandomMob();

            if (currentActiveMob is Mole mole)
            {
                yield return StartCoroutine(mole.PopCycle());
            }
            else if (currentActiveMob is Bomb bomb)
            {
                yield return StartCoroutine(bomb.PopCycle());
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
