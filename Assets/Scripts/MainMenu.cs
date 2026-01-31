using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown resolutionDropdown;

    private Resolution[] resolutions;
    private List<Resolution> filteredResolutions;

    private float currentRefreshRate;
    private int currentResolutionIndex = 0;
    [SerializeField] private Scene startingScene;

    [Header("Volume Settings")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    void Start()
    {
        // IDK why tf it's null the first time you click on Resolutions dropdown box
        if (resolutionDropdown == null) { return; }

        resolutions = Screen.resolutions;
        filteredResolutions = new List<Resolution>();

        resolutionDropdown.ClearOptions();
        currentRefreshRate = Screen.currentResolution.refreshRate;

        for (int i = 0; i < resolutions.Length; i++)
        {
            if (resolutions[i].refreshRate == currentRefreshRate)
            {
                filteredResolutions.Add(resolutions[i]);
            }
        }

        List<string> options = new List<string>();
        for (int i = 0; i < filteredResolutions.Count; i++)
        {
            string resolutionOption = filteredResolutions[i].width + "x" + filteredResolutions[i].height + " " + filteredResolutions[i].refreshRate;
            options.Add(resolutionOption);
            if (filteredResolutions[i].width == Screen.width && filteredResolutions[i].height == Screen.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        // Initialize Volume Sliders from PlayerPrefs
        if (masterSlider != null) masterSlider.value = PlayerPrefs.GetFloat("MasterVol", 0.75f);
        if (musicSlider != null) musicSlider.value = PlayerPrefs.GetFloat("MusicVol", 0.75f);
        if (sfxSlider != null) sfxSlider.value = PlayerPrefs.GetFloat("SFXVol", 0.75f);
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = filteredResolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, true);
    }

    public void SetFullScreen(bool isFullScreen)
    {
        Debug.Log(isFullScreen);
        Screen.fullScreen = isFullScreen;
    }

    public void OnStartGameClicked() 
    {
        SceneManager.LoadScene("domas_scene");
    }
    public void OnExitGameClicked()
    {
        Application.Quit();
    }

    public void OnMasterVolumeChanged(float value)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SetMasterVolume(value);
    }

    public void OnMusicVolumeChanged(float value)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SetMusicVolume(value);
    }

    public void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SetSFXVolume(value);
    }
}


