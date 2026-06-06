using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems; 
using System.Collections; 

public class UICredits : MonoBehaviour
{
    [Header("UI Controller Setup")]
    public GameObject backButton;

    [Header("Configuration")]
    public bool isSeparateScene = true;

    [Header("If Credit is a Panel (Optional)")]
    public GameObject mainMenuPanel;
    public GameObject creditButtonInMainMenu;

    void Start()
    {
        if (isSeparateScene) StartCoroutine(FocusOnBackButtonLate());
    }

    void OnEnable()
    {
        if (!isSeparateScene) StartCoroutine(FocusOnBackButtonLate());
    }

    private IEnumerator FocusOnBackButtonLate()
    {
        yield return null; 
        if (backButton != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null); 
            EventSystem.current.SetSelectedGameObject(backButton); 
        }
    }

    // Fungsi utama dipanggil oleh tombol Back
    public void BackToMainMenu()
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX("Button Click"); 
        }

        if (isSeparateScene)
        {
            SceneManager.LoadScene("Main Menu");
        }
        else
        {
            // [DIPERBARUI] Jalankan proses penutupan panel menggunakan Coroutine
            StartCoroutine(ReturnToMainMenuSequence());
        }
    }

    // [BARU] Sistem anti-balapan (Race Condition) dengan Main Menu
    private IEnumerator ReturnToMainMenuSequence()
    {
        // 1. Nyalakan panel Main Menu (Ini akan memicu script Main Menu menyorot tombol 'Play')
        if (mainMenuPanel != null) 
        {
            mainMenuPanel.SetActive(true);
        }

        // 2. Tunggu 1 frame agar script Main Menu selesai beraksi
        yield return null;

        // 3. Rebut kembali kursornya dan paksa sorot tombol 'Credit'
        if (creditButtonInMainMenu != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(creditButtonInMainMenu);
        }

        // 4. Setelah kursor aman di tombol Credit, baru matikan panel Credit ini
        gameObject.SetActive(false); 
    }
}