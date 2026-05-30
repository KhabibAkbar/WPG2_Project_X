using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResolutionSetting : MonoBehaviour
{
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;

    Resolution[] resolutions;
    List<string> options = new List<string>();
    int selectedResolutionIndex;

    void Start()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

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

        selectedResolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", currentResolutionIndex);
        bool isfullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;

        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        fullscreenToggle.isOn = isfullscreen;

        applyresolutions();
    }

    public void OnResolutionChanged(int index)
    {
        selectedResolutionIndex = index;
        applyresolutions();
    }

    public void setfullscreen(bool isfullscreen)
    {
        Screen.fullScreen = isfullscreen;
        applyresolutions();
    }

    public void applyresolutions()
    {
        Resolution res = resolutions[selectedResolutionIndex];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);

        PlayerPrefs.SetInt("ResolutionIndex", selectedResolutionIndex);
        PlayerPrefs.SetInt("Fullscreen", Screen.fullScreen ? 1 : 0);
        PlayerPrefs.Save();
    }
}
