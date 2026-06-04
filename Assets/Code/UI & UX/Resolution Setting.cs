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

    // Diubah dari Start() ke OnEnable() agar selalu refresh tiap kali panel dibuka
    void OnEnable()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();
        options.Clear(); // [BARU] Bersihkan list agar tidak menumpuk kalau dibuka tutup

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

        resolutionDropdown.value = selectedResolutionIndex; // [DIPERBARUI] Pakai data simpanan
        resolutionDropdown.RefreshShownValue();
        
        if (fullscreenToggle != null) fullscreenToggle.isOn = isfullscreen;

        applyresolutions();
        
        // Listener dinamis agar lebih aman
        resolutionDropdown.onValueChanged.RemoveAllListeners();
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
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