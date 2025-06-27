using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * @brief Handles spawning and cycling of different mob types (e.g., Mole, BombGyro, OilBarrel) based on weighted randomness.
 */
public class MobManager : MonoBehaviour
{
    /**
     * @brief Represents a mob entry with its script and spawn weight.
     */
    [System.Serializable]
    public class MobEntry
    {
        /**
         * @brief The MonoBehaviour script controlling the mob behavior.
         */
        public MonoBehaviour mobScript;

        /**
         * @brief Spawn probability weight for this mob (between 0 and 1).
         */
        [Range(0f, 1f)]
        public float spawnWeight = 0.5f;
    }

    /**
     * @brief List of all mobs with their respective spawn weights.
     */
    [Tooltip("Weighted list of mob entries")]
    public List<MobEntry> mobEntries = new List<MobEntry>();

    /**
     * @brief Minimum delay between mob spawns.
     */
    [Tooltip("Minimum time between spawns")]
    public float minSpawnDelay = 0.5f;

    /**
     * @brief Maximum delay between mob spawns.
     */
    [Tooltip("Maximum time between spawns")]
    public float maxSpawnDelay = 2f;

    /**
     * @brief Reference to the currently active mob in the scene.
     */
    private MonoBehaviour currentActiveMob;

    /**
     * @brief Unity Start method; begins the continuous mob spawn cycle.
     */
    void Start()
    {
        StartCoroutine(PopLoop());
    }

    /**
     * @brief Coroutine that loops indefinitely, selecting and activating a mob after random delays.
     * @return IEnumerator for coroutine execution.
     */
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

    /**
     * @brief Selects a mob from the list based on weighted probability.
     * @return The selected mob's MonoBehaviour script.
     */
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
