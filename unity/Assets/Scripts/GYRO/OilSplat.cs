using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/**
 * @brief Handles the fade-in and fade-out screen splat visual effect when the oil barrel is hit.
 */
public class OilSplatEffect : MonoBehaviour
{
    /**
     * @brief Duration over which the splat fades out completely.
     */
    public float fadeDuration = 1.5f;

    /**
     * @brief Reference to the UI Image component displaying the splat.
     */
    private Image splatImage;

    /**
     * @brief Original (transparent) color of the splat image.
     */
    private Color originalColor;

    /**
     * @brief Called on object initialization; sets initial splat transparency.
     */
    void Awake()
    {
        splatImage = GetComponent<Image>();
        originalColor = splatImage.color;
        originalColor.a = 0f;
        splatImage.color = originalColor;
    }

    /**
     * @brief Triggers the splat effect by starting the fade coroutine.
     */
    public void ShowSplat()
    {
        StopAllCoroutines();
        StartCoroutine(FadeInOut());
    }

    /**
     * @brief Coroutine to fade the splat image in and then out over time.
     * @return IEnumerator for coroutine execution.
     */
    private IEnumerator FadeInOut()
    {
        SetAlpha(1f);

        yield return new WaitForSeconds(fadeDuration * 0.3f);

        float t = 0f;
        while (t < fadeDuration * 0.4f)
        {
            t += Time.deltaTime;
            SetAlpha(Mathf.Lerp(1, 0, t / (fadeDuration * 0.4f)));
            yield return null;
        }

        SetAlpha(0f);
    }

    /**
     * @brief Sets the alpha value of the splat image color.
     * @param alpha The target alpha value to apply.
     */
    private void SetAlpha(float alpha)
    {
        var color = splatImage.color;
        color.a = alpha;
        splatImage.color = color;
    }
}
