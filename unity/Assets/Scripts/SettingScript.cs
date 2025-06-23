using UnityEngine;
using UnityEngine.UI;

public class SettingScript : MonoBehaviour
{
    public GameObject settingsPanel;

    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private Slider musicVolumeSlider;
    void Start()
    {
        settingsPanel.SetActive(false);
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            musicVolumeSlider.value = audioSource.volume;
        }

    }
    public void OnMusicVolumeChanged(float value)
    {
        audioSource.volume = value;
    }
    public void Fullscreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
    }
}
