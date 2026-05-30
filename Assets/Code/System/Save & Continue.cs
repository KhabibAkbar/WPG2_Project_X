using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections; // Wajib untuk Coroutine

public class GameDataHandler : MonoBehaviour
{
    [Header("UI Setup")]
    public Button continueButton;
    
    [Header("Zoom Transisi Setup")]
    [Tooltip("Masukkan gambar background UI utama yang menyatu")]
    public RectTransform mainUIBackground; 
    
    [Tooltip("Masukkan parent dari semua tombol menu")]
    public GameObject menuButtonsContainer;
    
    public float zoomMultiplier = 1.3f;
    public float animationDuration = 1.2f;

    void Start()
    {
        if (continueButton != null)
        {
            CheckContinueStatus();
        }
    }

    public void CheckContinueStatus()
    {
        // Mengecek apakah ada data save dengan kunci "LastScene"
        if (PlayerPrefs.HasKey("LastScene"))
        {
            continueButton.interactable = true;
        }
        else
        {
            continueButton.interactable = false;
        }
    }

    public void SaveCurrentLevel()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString("LastScene", currentSceneName);
        PlayerPrefs.Save();
        Debug.Log("Level Tersimpan: " + currentSceneName);
    }

    public void ContinueGame()
    {
        if (PlayerPrefs.HasKey("LastScene"))
        {
            Time.timeScale = 1f; 
            // Jalankan efek zoom sebelum pindah scene
            StartCoroutine(ZoomAndLoadContinue());
        }
    }

    private IEnumerator ZoomAndLoadContinue()
    {
        // 1. Sembunyikan tombol menu
        if (menuButtonsContainer != null) 
        {
            menuButtonsContainer.SetActive(false);
        }

        // 2. Proses Zoom In
        if (mainUIBackground != null)
        {
            Vector3 initialScale = mainUIBackground.localScale;
            Vector3 targetScale = initialScale * zoomMultiplier;
            float elapsedTime = 0f;

            while (elapsedTime < animationDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / animationDuration;
                float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

                mainUIBackground.localScale = Vector3.Lerp(initialScale, targetScale, smoothProgress);
                yield return null; 
            }
        }
        else
        {
            yield return new WaitForSeconds(animationDuration);
        }

        // 3. Pindah ke scene yang tersimpan
        string sceneToLoad = PlayerPrefs.GetString("LastScene");
        FadeManager.instance.LoadScene(sceneToLoad);
        Debug.Log("Melanjutkan ke level: " + sceneToLoad);
    }

    public void StartNewGame(string firstLevelName)
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene(firstLevelName);
    }
}