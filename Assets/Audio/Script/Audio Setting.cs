using UnityEngine;
using UnityEngine.UI;

public class AudioSetting : MonoBehaviour
{
    public Slider bgmSlider;
    public Slider sfxSlider;

    void Start()
    {
        // 1. Ambil data yang tersimpan. 
        // Gunakan default 0f jika ingin saat pertama kali buka lsg Mute (sesuai request kamu).
        float savedBGM = PlayerPrefs.GetFloat("BGMVolume", 0f);
        float savedSFX = PlayerPrefs.GetFloat("SFXVolume", 0f);

        // 2. Set nilai visual slider di UI
        bgmSlider.value = savedBGM;
        sfxSlider.value = savedSFX;

        // 3. PAKSA AudioManager untuk memakai nilai ini saat Start
        // Ini penting agar suaranya sinkron dengan posisi slider saat buka aplikasi
        if (AudioManager.instance != null)
        {
            AudioManager.instance.SetBGMVolume(savedBGM);
            AudioManager.instance.SetSFXVolume(savedSFX);
        }

        // 4. Daftarkan event listener (digunakan saat player menggeser slider)
        bgmSlider.onValueChanged.AddListener(AudioManager.instance.SetBGMVolume);
        sfxSlider.onValueChanged.AddListener(AudioManager.instance.SetSFXVolume);
    }
}