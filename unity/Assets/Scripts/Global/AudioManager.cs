using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Audio;

/**
 * @brief Singleton audio manager that handles theme and minigame track playback with fade transitions.
 */
public class AudioManager : MonoBehaviour
{
    /**
     * @brief Global instance reference.
     */
    public static AudioManager Instance;

    [Header("Audio Setup")]
    /**
     * @brief AudioSource used for playing music tracks.
     */
    public AudioSource musicSource;

    [Header("Main Theme")]
    /**
     * @brief Audio clip for the main theme.
     */
    public AudioClip mainTheme;

    [Header("Mini-Game Tracks")]
    /**
     * @brief Array of AudioClips for minigame background tracks.
     */
    public AudioClip[] miniGameTracks = new AudioClip[5];

    /**
     * @brief Unity event called when the script instance is loaded; initializes singleton and plays main theme.
     */
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

    /**
     * @brief Plays the main theme starting at a random cue point with a fade transition.
     * @param fadeDuration Duration of fade-out and fade-in in seconds (default 1f).
     */
    public void PlayMainTheme(float fadeDuration = 1f)
    {
        // Define cue points in seconds: 0:00, 1:14, 2:27
        float[] cuePoints = { 0f, 74f, 147f }; // 74s = 1:14, 147s = 2:27
        float startTime = cuePoints[Random.Range(0, cuePoints.Length)];

        StartCoroutine(FadeToTrack(mainTheme, fadeDuration, startTime));
    }

    /**
     * @brief Randomly selects and plays one of the mini-game tracks with a fade transition.
     * @param fadeDuration Duration of fade-out and fade-in in seconds (default 1f).
     */
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

    /**
     * @brief Plays a specific mini-game track by index with a fade transition.
     * @param index Index of the mini-game track in the array.
     * @param fadeDuration Duration of fade-out and fade-in in seconds (default 1f).
     */
    public void PlaySelectedMiniGameTrack(int index, float fadeDuration = 1f)
    {
        if (miniGameTracks.Length == 0)
        {
            Debug.LogWarning("No mini-game tracks assigned.");
            return;
        }

        StartCoroutine(FadeToTrack(miniGameTracks[index], fadeDuration));
    }

    /**
     * @brief Stops music playback immediately.
     */
    public void StopMusic()
    {
        musicSource.Stop();
    }

    /**
     * @brief Starts or resumes music playback immediately.
     */
    public void PlayMusic()
    {
        musicSource.Play();
    }

    /**
     * @brief Coroutine that fades out the current track, switches to a new clip, and fades it in.
     * @param newClip The AudioClip to switch to.
     * @param duration The duration of fade-out and fade-in in seconds.
     * @param startTime Optional start time within the new clip (default 0f).
     * @return IEnumerator for coroutine control.
     */
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

        StopMusic();
        musicSource.clip = newClip;
        musicSource.time = Mathf.Clamp(startTime, 0f, newClip.length);
        musicSource.loop = true;
        PlayMusic();

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
