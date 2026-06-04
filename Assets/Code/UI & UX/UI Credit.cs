using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems; // Wajib untuk mendukung Controller

public class UICredits : MonoBehaviour
{
    [Header("UI Controller Setup")]
    [Tooltip("Masukkan tombol Back/Kembali yang ada di menu Credit")]
    public GameObject backButton;

    [Header("Configuration")]
    [Tooltip("Centang ini jika Credit adalah SCENE TERPISAH. Jangan dicentang jika Credit hanya sebuah PANEL di dalam Main Menu")]
    public bool isSeparateScene = true;

    [Header("If Credit is a Panel (Optional)")]
    [Tooltip("Masukkan GameObject induk Main Menu agar bisa memunculkan tombol menu utama kembali")]
    public GameObject mainMenuPanel;
    [Tooltip("Masukkan tombol 'Credit' di Main Menu agar kursor kembali menyorot tombol tersebut saat panel ditutup")]
    public GameObject creditButtonInMainMenu;

    void Start()
    {
        // Jika berupa scene terpisah, fokus langsung diberikan saat scene dimuat
        if (isSeparateScene)
        {
            FocusOnBackButton();
        }
    }

    void OnEnable()
    {
        // Jika berupa panel di dalam Main Menu, fokus diberikan setiap kali panel aktif
        if (!isSeparateScene)
        {
            FocusOnBackButton();
        }
    }

    private void FocusOnBackButton()
    {
        if (backButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null); // Bersihkan fokus lama
            EventSystem.current.SetSelectedGameObject(backButton); // Sorot tombol Back
        }
    }

    // Fungsi utama untuk kembali ke Main Menu (Hubungkan ke OnClick tombol Back)
    public void BackToMainMenu()
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX("Button Click"); 
        }

        if (isSeparateScene)
        {
            // Opsi 1: Jika berupa scene terpisah, muat ulang scene menu utama
            SceneManager.LoadScene("Main Menu");
        }
        else
        {
            // Opsi 2: Jika berupa panel, hidupkan kembali menu utama dan matikan panel ini
            if (mainMenuPanel != null) 
            {
                mainMenuPanel.SetActive(true);
            }
            
            // Kembalikan fokus kursor controller ke tombol di Main Menu
            if (creditButtonInMainMenu != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(creditButtonInMainMenu);
            }

            gameObject.SetActive(false); // Matikan panel credit ini
        }
    }
}