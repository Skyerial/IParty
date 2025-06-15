using UnityEngine;
using System.Collections;

public class TM_MusicController : MonoBehaviour
{
    // AudioSources (game object with audio source component) 
    public AudioSource sfxSource;
    public AudioSource bgmSource;
    public AudioClip bgmTypingGame;
    public AudioClip correctWordSFX;
    public AudioClip finishSFX;
    public AudioClip endGameSFX;

    public static TM_MusicController Instance; // Singleton instance

    private void Awake()
    {
        // Ensure there's only one instance of this controller
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void FadeInBGM(float duration)
    {
        StartCoroutine(FadeInBGMCoroutine(duration));
    }

    public void FadeOutBGM(float duration)
    {
        StartCoroutine(FadeOutBGMCoroutine(duration));
    }

    private IEnumerator FadeInBGMCoroutine(float duration)
    {
        bgmSource.volume = 0f;
        bgmSource.clip = bgmTypingGame;
        bgmSource.loop = true;
        bgmSource.Play();

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            bgmSource.volume = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }
    }

    private IEnumerator FadeOutBGMCoroutine(float duration)
    {
        float startVolume = bgmSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        bgmSource.Stop();
        bgmSource.volume = startVolume; // reset for next play
    }

    // Method to play SFX
    public void PlaySFX(AudioClip clip, float volume = 1.0f)
    {
        sfxSource.PlayOneShot(clip, volume);
    }

    // Method to play correct word SFX
    public void PlayCorrectWordSFX()
    {
        PlaySFX(correctWordSFX);
    }

    // Method to play SFX when player finishes in 1st place
    public void PlayFinishSFX()
    {
        PlaySFX(finishSFX, 0.5f);
    }

    public void PlayEndGameSFX()
    {
        PlaySFX(endGameSFX);
    }

}
