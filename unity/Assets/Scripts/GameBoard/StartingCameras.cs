using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class StartingCameras : MonoBehaviour
{
    public Transform centerPoint; // The point to look at (e.g., player or scene center)
    public float duration = 10f;    // Time to complete one full circle
    // public float height = 500f;
    public new GameObject camera;
    public float startRadius = 100f;
    public float endRadius = 2f;
    public float startHeight = 50f;
    public float endHeight = 10f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public IEnumerator SpiralCamera(System.Action onComplete)
    {
        Debug.LogWarning("started");
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
        // yield return StartCoroutine(RotateToAngle(new Vector3(20f, 0f, 0f), 0.5f));
        camera.SetActive(false);
        onComplete?.Invoke();
    }

    private IEnumerator RotateToAngle(Vector3 targetEuler, float rotationDuration)
    {
        Quaternion startRotation = camera.transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(targetEuler);
        float elapsed = 0f;

        while (elapsed < rotationDuration)
        {
            camera.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsed / rotationDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        camera.transform.rotation = targetRotation; // Ensure exact final rotation
    }
}


