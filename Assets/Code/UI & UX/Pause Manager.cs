using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static PauseManager instance;

    public GameObject panelPause;
    public GameObject panelSettings;

    private bool isPaused = false;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        panelPause.SetActive(false);

        if (panelSettings != null)
            panelSettings.SetActive(false);
    }

    void Update()
    {
        if (GameManager.instance != null && GameManager.instance.isGameOver)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX("Button Click"); 
        }
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        panelPause.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        panelPause.SetActive(false);

        if (panelSettings != null)
            panelSettings.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;
    }

    public void RestartGame()
    {
        // 1. Matikan waktu
        Time.timeScale = 1f;

        // 2. Beritahu AudioManager untuk blokir SEMUA SFX sekarang juga
        if (AudioManager.instance != null)
        {
            AudioManager.instance.blockAllSFX = true;
            AudioManager.instance.StopLoopingSFX();
        }

        // 3. (Opsional) Beritahu semua Platform untuk diam
        InjectTriggerSingle[] allPlatforms = FindObjectsOfType<InjectTriggerSingle>();
        foreach (var p in allPlatforms)
        {
            p.PrepareForSceneTransition();
        }

        // 4. Baru pindah scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OpenSettings()
    {
        if (panelSettings != null)
            panelSettings.SetActive(true);
    }

    public void BackToPauseMenu()
    {
        if (panelSettings != null)
            panelSettings.SetActive(false);
    }

    public void CloseSettings()
    {
        if (panelSettings != null)
            panelSettings.SetActive(false);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Menu");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}