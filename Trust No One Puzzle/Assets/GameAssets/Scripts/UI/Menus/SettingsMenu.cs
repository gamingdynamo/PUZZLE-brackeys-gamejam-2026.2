using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsMenu : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider volumeSlider;

    [Header("Mouse Sensitivity")]
    [SerializeField] private Slider sensitivitySlider;
    public static float mouseSensitivity = 2f;

    private void Start()
    {
        // Load saved volume or default to 0dB
        if (PlayerPrefs.HasKey("MasterVolume"))
        {
            float savedVolume = PlayerPrefs.GetFloat("MasterVolume");
            volumeSlider.value = savedVolume;
            SetVolume(savedVolume);
        }
        else
        {
            SetVolume(volumeSlider.value);
        }

        // Load saved mouse sensitivity
        if (PlayerPrefs.HasKey("MouseSensitivity"))
        {
            float savedSensitivity = PlayerPrefs.GetFloat("MouseSensitivity");
            sensitivitySlider.value = savedSensitivity;
            SetMouseSensitivity(savedSensitivity);
        }
        else
        {
            sensitivitySlider.value = mouseSensitivity;
            SetMouseSensitivity(mouseSensitivity);
        }
    }

    public void SetVolume(float sliderValue)
    {
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20);
        PlayerPrefs.SetFloat("MasterVolume", sliderValue);
    }

    public void SetMouseSensitivity(float sensitivityValue)
    {
        mouseSensitivity = sensitivityValue;
        PlayerPrefs.SetFloat("MouseSensitivity", sensitivityValue);
    }
}
