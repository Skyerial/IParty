using UnityEngine;

[DisallowMultipleComponent]
public class SceneGravityController : MonoBehaviour
{
    [Tooltip("The gravity vector you want for this scene.")]
    public Vector3 sceneGravity = new Vector3(0f, -9.81f, 0f);

    // We’ll store the previous (project‐wide) gravity so we can restore it if/when this object is destroyed.
    private Vector3 originalGravity;

    void OnEnable()
    {
        // Cache whatever Physics.gravity was before
        originalGravity = Physics.gravity;

        // Apply our scene‐specific gravity
        Physics.gravity = sceneGravity;
    }

    void OnDisable()
    {
        // When this object is disabled or the scene unloads, restore the previous gravity
        Physics.gravity = originalGravity;
    }
}
