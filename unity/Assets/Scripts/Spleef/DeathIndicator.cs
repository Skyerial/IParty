using System.Collections;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(AudioSource))]
public class DeathIndicator : MonoBehaviour
{
    [Tooltip("How tall the beam will grow (world units)")]
    public float maxHeight = 5f;

    [Tooltip("Speed at which the beam grows")]
    public float growSpeed = 10f;

    [Tooltip("Death sound effect")]
    public AudioClip deathSFX;

    private LineRenderer lr;
    private AudioSource audioSrc;
    private Vector3 basePos;
    private float currentHeight = 0f;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        audioSrc = GetComponent<AudioSource>();

        lr.positionCount = 2;
        lr.startWidth   = 0.1f;
        lr.endWidth     = 0.1f;
        lr.material     = new Material(Shader.Find("Unlit/Color")) { color = Color.red };
        audioSrc.playOnAwake = false;
    }

    public void ShowAt(Vector3 worldPos)
    {
        basePos = new Vector3(worldPos.x, worldPos.y, worldPos.z);
        lr.SetPosition(0, basePos);
        lr.SetPosition(1, basePos);

        if (deathSFX != null)
            audioSrc.PlayOneShot(deathSFX);

        StartCoroutine(GrowBeam());
    }

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
