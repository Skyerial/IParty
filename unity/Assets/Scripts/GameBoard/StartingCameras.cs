using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
/**
 * @brief Handles the starting camera of the board game. It causes the camera
 * to spiral towards a given centre point.
 */

public class StartingCameras : MonoBehaviour
{
    /**
    * @brief This given transform is the centerpoint to which the camera spirals
    */
    public Transform centerPoint; // The point to look at (e.g., player or scene center)

    /**
    * @brief This is the duration of the camera movement
    */
    public float duration = 10f;    // Time to complete one full circle

    /**
    * @brief This GameObject is the camera that spirals towards the center
    */
    public new GameObject camera;

    /**
    * @brief This defines the starting radius (distance from camera to center)
    * which will decrease during the movement (as a spiral should).
    */
    public float startRadius = 100f;
    /**
    * @brief This defines the ending radius (distance from camera to center).
    * So to what radius the camera will decrease during the movement.
    */
    public float endRadius = 2f;
    /**
    * @brief This defines the height at the start of the movement which will
    * also linearly decrease as the movement continues.
    */
    public float startHeight = 50f;
    /**
    * @brief This defines the ending height of the movement, so till what height
    * it will decrease.
    */
    public float endHeight = 10f;

    /**
    * @brief This public function can be called by other cs files
    * (e.g. GameMaster) causing the spiral camera movement to start.
    */
    public IEnumerator SpiralCamera(System.Action onComplete)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;

            // Spiral angle (360 degrees over time, or more if you want more loops)
            float angle = t * 360f * 1.5f; // 2 full circles
            float rad = angle * Mathf.Deg2Rad;

            // Linearly decrease the radius from startRadius to endRadius
            float radius = Mathf.Lerp(startRadius, endRadius, t);
            float height = Mathf.Lerp(startHeight, endHeight, t);

            // Position in spiral
            Vector3 offset = new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad)) * radius;
            offset.y = height;

            // Update position and look at center
            camera.transform.position = centerPoint.position + offset;
            camera.transform.LookAt(centerPoint.position);

            elapsed += Time.deltaTime;
            yield return null;
        }
        camera.SetActive(false);
        onComplete?.Invoke();
    }
}


