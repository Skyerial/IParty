// DeathIndicator.cs
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(AudioSource))]
/**
 * @brief Displays a rising beam and plays a sound effect at a given world position to indicate a player's death.
 */
public class DeathIndicator : MonoBehaviour
{
    /**
     * @brief How tall the death beam grows in world units.
     */
    [Tooltip("How tall the beam will grow (world units)")]
    public float maxHeight = 5f;

    /**
     * @brief Speed at which the death beam grows.
     */
    [Tooltip("Speed at which the beam grows")]
    public float growSpeed = 10f;

    /**
     * @brief Audio clip played when the beam appears.
     */
    [Tooltip("Death sound effect")]
    public AudioClip deathSFX;

    private LineRenderer lr;
    private AudioSource audioSrc;
    private Vector3 basePos;
    private float currentHeight = 0f;

    /**
     * @brief Unity event called when the script instance is loaded; sets up LineRenderer and AudioSource.
     */
    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        audioSrc = GetComponent<AudioSource>();

        lr.positionCount = 2;
        lr.startWidth   = 0.1f;
        lr.endWidth     = 0.1f;
        lr.material = new Material(Shader.Find("Unlit/Color"));
        audioSrc.playOnAwake = false;
    }

    /**
     * @brief Positions the death indicator at the specified world position with the given color and starts the growth coroutine.
     */
    public void ShowAt(Vector3 worldPos, Color playerColor)
    {
        lr.material.color = playerColor;

        basePos = new Vector3(worldPos.x, worldPos.y, worldPos.z);
        lr.SetPosition(0, basePos);
        lr.SetPosition(1, basePos);

        if (deathSFX != null)
            audioSrc.PlayOneShot(deathSFX);

        StartCoroutine(GrowBeam());
    }

    /**
     * @brief Coroutine that animates the beam growing upward until it reaches maxHeight, then destroys the GameObject.
     */
    private IEnumerator GrowBeam()
    {
        while (currentHeight < maxHeight)
        {
            currentHeight += growSpeed * Time.deltaTime;
            float h = Mathf.Min(currentHeight, maxHeight);
            lr.SetPosition(1, basePos + Vector3.up * h);
            yield return null;
        }

        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }
}
