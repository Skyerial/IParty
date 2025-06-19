using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Setup")]
    public AudioSource musicSource;

    [Header("Main Theme")]
    public AudioClip mainTheme;

    [Header("Mini-Game Tracks")]
    public AudioClip[] miniGameTracks = new AudioClip[5];

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // automatically play main theme on first load: lobby
            PlayMainTheme();
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    // Plays main theme 
    public void PlayMainTheme(float fadeDuration = 1f)
    {
        // Define cue points in seconds: 0:00, 1:14, 2:27
        float[] cuePoints = { 0f, 74f, 147f }; // 74s = 1:14, 147s = 2:27
        float startTime = cuePoints[Random.Range(0, cuePoints.Length)];

        StartCoroutine(FadeToTrack(mainTheme, fadeDuration, startTime));
    }

    // Randomly plays 1 of 5 minigame tracks
    public void PlayRandomMiniGameTrack(float fadeDuration = 1f)
    {
        if (miniGameTracks.Length == 0)
        {
            Debug.LogWarning("No mini-game tracks assigned.");
            return;
        }

        int index = Random.Range(0, miniGameTracks.Length);
        StartCoroutine(FadeToTrack(miniGameTracks[index], fadeDuration));
    }

    // Coroutine to fade out current track and fade in the new track
    private IEnumerator FadeToTrack(AudioClip newClip, float duration, float startTime = 0f)
    {
        float startVolume = musicSource.volume;
        float elapsed = 0f;

        // Fade out
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        musicSource.Stop();
        musicSource.clip = newClip;
        musicSource.time = Mathf.Clamp(startTime, 0f, newClip.length);
        musicSource.loop = true;
        musicSource.Play();

        // Fade in
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, startVolume, elapsed / duration);
            yield return null;
        }
    }
}
