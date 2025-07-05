using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsScript : MonoBehaviour
{
    [SerializeField] GameObject pauseScreen;

    [SerializeField] GameObject settingsScreen;

    [SerializeField] Slider musicSlider;

    [SerializeField] Slider effectsSlider;

    [SerializeField] AudioMixer musicMixer;

    [SerializeField] AudioMixer effectsMixer;


    public TMP_Dropdown resolutionDropdown;



    Resolution[] resolutions;

    void Start()
    {
        LoadVolume();
        resolutions = Screen.resolutions;

        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();

        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;

            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);

        resolutionDropdown.value = currentResolutionIndex;

        resolutionDropdown.RefreshShownValue();
    }

    public void LoadVolume()
    {
        float savedMusicVolume = PlayerPrefs.GetFloat("MusicVolume", 0f); // Default to 0 if no saved value
        float savedEffectsVolume = PlayerPrefs.GetFloat("EffectsVolume", 0f); // Default to 0 if no saved value
        musicSlider.value = savedMusicVolume;
        effectsSlider.value = savedEffectsVolume;
        SetMusicVolume(savedMusicVolume);
        SetEffectsVolume(savedEffectsVolume);
    }

    public void SetResolution(int resolutionIndex)
    {
        Debug.Log("SetResolution called with index: " + resolutionIndex);
        Resolution resolution = resolutions[resolutionIndex];
        Debug.Log("Setting resolution to: " + resolution.width + " x " + resolution.height);
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }


    public void SetMusicVolume(float volume)
    {
        musicMixer.SetFloat("MusicVolume", volume);
        PlayerPrefs.SetFloat("MusicVolume", volume); // Save volume setting
    }

    public void SetEffectsVolume(float volume)
    {
        effectsMixer.SetFloat("SoundEffectVolume", volume);
        PlayerPrefs.SetFloat("EffectsVolume", volume); // Save volume setting
    }

    public void closePauseMenu()
    {
        pauseScreen.SetActive(false);
    }

    public void openSettingsMenu()
    {
        settingsScreen.SetActive(true);
    }

    public void closeSettingsMenu()
    {
        settingsScreen.SetActive(false);
    }

    public void SetFullscreen(bool fullscreen)
    {
        Screen.fullScreen = fullscreen;
    }
}
