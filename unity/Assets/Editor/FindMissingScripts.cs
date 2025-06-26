using UnityEditor;
using UnityEngine;

/**
 * @brief Editor utility to find all GameObjects in the current scene
 *        that have missing MonoBehaviour script references.
 */
public class FindMissingScripts : EditorWindow
{
    /**
     * @brief Scans all GameObjects in the open scene and logs any components
     *        that reference missing scripts (e.g., due to deleted or renamed files).
     *
     * Adds a menu item under Tools > Find Missing Scripts.
     * Prints the GameObject name and makes it clickable in the Console.
     */
    [MenuItem("Tools/Find Missing Scripts")]
    static void FindAllMissingScripts()
    {
        int goCount = 0;
        int componentsCount = 0;
        int missingCount = 0;

        GameObject[] gos = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (GameObject g in gos)
        {
            goCount++;
            Component[] components = g.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                componentsCount++;
                if (components[i] == null)
                {
                    missingCount++;
                    Debug.Log($"Missing script in GameObject: '{g.name}'", g);
                }
            }
        }

        Debug.Log($"Searched {goCount} GameObjects, {componentsCount} components, found {missingCount} missing scripts.");
    }
}
