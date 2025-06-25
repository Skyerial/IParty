using UnityEngine;
using System.Collections;

public class TM_MusicController : MonoBehaviour
{
    // AudioSources (game object with audio source component) 
    public AudioSource sfxSource;
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
