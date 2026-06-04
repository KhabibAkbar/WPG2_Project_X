using UnityEngine;
using UnityEngine.EventSystems; // [BARU] Wajib untuk Controller

public class OptionSetting : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject Resolution;
    public GameObject Graphic;
    public GameObject BGM;
    public GameObject SFX;

    [Header("Controller Focus Setup")]
    [Tooltip("Masukkan Dropdown Resolution ke sini")]
    public GameObject firstVideoSelected; 
    [Tooltip("Masukkan Slider BGM ke sini")]
    public GameObject firstAudioSelected; 

    // Gunakan OnEnable agar saat panel Setting pertama kali dibuka dari Main Menu/Pause,
    // dia langsung menyorot menu resolusi.
    private void OnEnable()
    {
        Tampilkan_Resolustion();
    }

    public void Tampilkan_Resolustion()
    {
        Resolution.SetActive(true);
        Graphic.SetActive(true);
        BGM.SetActive(false);
        SFX.SetActive(false);

        // [BARU] Arahkan kursor controller ke Dropdown Resolution
        if (firstVideoSelected != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstVideoSelected);
        }
    }

    public void Tampilkan_Auidio()
    {
        BGM.SetActive(true);
        SFX.SetActive(true);
        Resolution.SetActive(false);
        Graphic.SetActive(false);

        // [BARU] Arahkan kursor controller ke Slider BGM
        if (firstAudioSelected != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstAudioSelected);
        }
    }
}