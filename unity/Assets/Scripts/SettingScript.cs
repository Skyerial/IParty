using UnityEngine;
using UnityEngine.UI;

/**
 * @brief Handles UI settings such as volume and fullscreen toggle.
 */
public class SettingScript : MonoBehaviour
{
    public GameObject settingsPanel;

    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private Slider musicVolumeSlider;

    /**
     * @brief Initializes the settings panel and links volume slider to audio source.
     * @return void
     */
    void Start()
    {
        settingsPanel.SetActive(false);
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            musicVolumeSlider.value = audioSource.volume;
        }
    }

    /**
     * @brief Called when the music volume slider is adjusted. Updates audio volume.
     * @param value New volume level from the slider (0.0 to 1.0).
     * @return void
     */
    public void OnMusicVolumeChanged(float value)
    {
        audioSource.volume = value;
    }

    /**
     * @brief Toggles fullscreen mode on or off.
     * @return void
     */
    public void Fullscreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
    }
}
