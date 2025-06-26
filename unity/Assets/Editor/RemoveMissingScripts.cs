using UnityEditor;
using UnityEngine;

/**
 * @brief Editor utility to remove all missing MonoBehaviour script references
 *        from all GameObjects in the current open scene.
 */
public class RemoveMissingScripts
{
    /**
     * @brief Scans all GameObjects in the scene and removes components
     *        with missing script references (usually caused by deleted or renamed scripts).
     *
     * Adds a menu item under Tools > Remove All Missing Scripts in Scene.
     * Logs the number of removed components per GameObject and the total.
     */
    [MenuItem("Tools/Remove All Missing Scripts in Scene")]
    static void RemoveMissingScriptsInScene()
    {
        int removedCount = 0;
        GameObject[] gos = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        foreach (GameObject go in gos)
        {
            int count = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
            if (count > 0)
            {
                Debug.Log($"Removed {count} missing script(s) from GameObject: {go.name}", go);
                removedCount += count;
            }
        }

        Debug.Log($"Removed total {removedCount} missing script components from scene.");
    }
}
