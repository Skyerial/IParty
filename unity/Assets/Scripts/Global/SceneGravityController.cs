using UnityEngine;

[DisallowMultipleComponent]
/**
 * @brief Overrides the global Physics.gravity vector for this scene and restores it on disable.
 */
public class SceneGravityController : MonoBehaviour
{
    /**
     * @brief The gravity vector to apply for this scene.
     */
    [Tooltip("The gravity vector you want for this scene.")]
    public Vector3 sceneGravity = new Vector3(0f, -9.81f, 0f);

    // We’ll store the previous (project‐wide) gravity so we can restore it if/when this object is destroyed.
    private Vector3 originalGravity;

    /**
     * @brief Unity event called when this component is enabled; caches the previous gravity and applies sceneGravity.
     */
    void OnEnable()
    {
        originalGravity = Physics.gravity;
        Physics.gravity = sceneGravity;
    }

    /**
     * @brief Unity event called when this component is disabled; restores the cached original gravity.
     */
    void OnDisable()
    {
        Physics.gravity = originalGravity;
    }
}
