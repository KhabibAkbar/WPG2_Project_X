using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class UIMainMenu : MonoBehaviour
{
    [Header("Zoom Transisi Setup")]
    [Tooltip("Masukkan RectTransform dari gambar UI utama (yang berisi background + karakter)")]
    public RectTransform mainUIBackground; 
    
    [Tooltip("Target ukuran zoom. Disarankan antara 1.2 sampai 1.5 agar gambar tidak terlalu pecah")]
    public float zoomMultiplier = 1.3f;

    [Tooltip("Durasi efek zoom berlangsung (detik)")]
    public float animationDuration = 1.2f; 

    [Tooltip("Masukkan GameObject induk dari semua tombol menu agar bisa disembunyikan saat mulai zoom")]
    public GameObject menuButtonsContainer;

    public void Play_Game()
    {
        StartCoroutine(PlayZoomEffectAndLoad());
        Debug.Log("Play Game - Memulai Efek Zoom Layar Utama");
    }

    private IEnumerator PlayZoomEffectAndLoad()
    {
        // 1. Sembunyikan tombol-tombol menu (Play, Exit, dll) agar bersih saat zoom
        if (menuButtonsContainer != null) 
        {
            menuButtonsContainer.SetActive(false);
        }

        // 2. Proses melakukan perbesaran (Zoom In) pada seluruh gambar
        if (mainUIBackground != null)
        {
            Vector3 initialScale = mainUIBackground.localScale;
            Vector3 targetScale = initialScale * zoomMultiplier;
            float elapsedTime = 0f;

            while (elapsedTime < animationDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / animationDuration;
                
                // Efek perlambatan halus di awal dan akhir zoom
                float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

                mainUIBackground.localScale = Vector3.Lerp(initialScale, targetScale, smoothProgress);
                
                yield return null; 
            }
        }
        else
        {
            yield return new WaitForSeconds(animationDuration);
        }

        // 3. Pindah ke scene berikutnya menggunakan FadeManager
        FadeManager.instance.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        Debug.Log("Zoom Selesai - Pindah Scene");
    }

    public void Exit()
    {
        Debug.Log("Exit");
        Application.Quit();
    }

    public void Back_To_Menu()
    {
        SceneManager.LoadScene("Main Menu");
        Debug.Log("Back To Menu");
    }

    // --- FUNGSI UPDATE ---

    public void Pilih(string namaSceneTujuan)
    {
        SceneManager.LoadScene(namaSceneTujuan); 
        Debug.Log("Pindah ke scene: " + namaSceneTujuan);
    }

    public void Back()
    {
        int previousSceneIndex = SceneManager.GetActiveScene().buildIndex - 1;
        SceneManager.LoadScene(previousSceneIndex);
        Debug.Log("Kembali ke Scene sebelumnya (Index: " + previousSceneIndex + ")");
    }
}