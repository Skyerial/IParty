using UnityEngine;

/**
 * @class FloatCrown
 * @brief Makes the GameObject float up and down in a sine wave pattern.
 */
public class FloatCrown : MonoBehaviour
{
    /** 
     * @brief The height of the floating motion.
     */
    public float amplitude = 0.5f;

    /** 
     * @brief The speed of the floating motion.
     */
    public float frequency = 1f;

    /** 
     * @brief The original position of the GameObject.
     */
    private Vector3 startPos;

    /**
     * @brief Caches the starting position of the GameObject.
     * @return void
     */
    void Start()
    {
        startPos = transform.position;
    }

    /**
     * @brief Updates the GameObject's position to create a floating effect.
     * @return void
     */
    void Update()
    {
        if (!gameObject.activeInHierarchy) return;

        float offset = Mathf.Sin(Time.time * frequency) * amplitude;
        transform.position = startPos + Vector3.up * offset;
    }
}

