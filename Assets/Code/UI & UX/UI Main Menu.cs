using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.EventSystems; // [BARU] Wajib ditambahkan untuk UI Controller

public class UIMainMenu : MonoBehaviour
{
    [Header("UI Controller Setup")]
    [Tooltip("Masukkan tombol PLAY ke sini agar langsung tersorot saat game mulai")]
    public GameObject firstSelectedButton; // [BARU]

    [Header("Zoom Transisi Setup")]
    public RectTransform mainUIBackground; 
    public float zoomMultiplier = 1.3f;
    public float animationDuration = 1.2f; 
    public GameObject menuButtonsContainer;

    void Start()
    {
        // [BARU] Sorot tombol pertama saat Main Menu terbuka
        if (firstSelectedButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null); // Bersihkan dulu
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        }
    }

    public void Play_Game()
    {
        StartCoroutine(PlayZoomEffectAndLoad());
        Debug.Log("Play Game - Memulai Efek Zoom Layar Utama");
    }

    private IEnumerator PlayZoomEffectAndLoad()
    {
        if (menuButtonsContainer != null) menuButtonsContainer.SetActive(false);

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

        FadeManager.instance.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void Back_To_Menu()
    {
        SceneManager.LoadScene("Main Menu");
    }

    public void Pilih(string namaSceneTujuan)
    {
        SceneManager.LoadScene(namaSceneTujuan); 
    }

    public void Back()
    {
        int previousSceneIndex = SceneManager.GetActiveScene().buildIndex - 1;
        SceneManager.LoadScene(previousSceneIndex);
    }
}