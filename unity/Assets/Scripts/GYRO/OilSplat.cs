using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class OilSplatEffect : MonoBehaviour
{
    public float fadeDuration = 1.5f;

    private Image splatImage;
    private Color originalColor;

    void Awake()
    {
        splatImage = GetComponent<Image>();
        originalColor = splatImage.color;
        originalColor.a = 0f;
        splatImage.color = originalColor;
    }

    public void ShowSplat()
    {
        StopAllCoroutines();
        StartCoroutine(FadeInOut());
    }

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

    private void SetAlpha(float alpha)
    {
        var color = splatImage.color;
        color.a = alpha;
        splatImage.color = color;
    }
}
