using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems; // [BARU]

public class PauseManager : MonoBehaviour
{
    public static PauseManager instance;

    [Header("UI Controller Setup")]
    public GameObject pauseFirstButton; // Tombol Resume
    public GameObject settingsFirstButton; // Tombol pertama di menu Setting

    [Header("Input Setup")]
    public string pauseInputController = "Pause_Action"; // Untuk tombol Start/Options controller

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
        if (panelSettings != null) panelSettings.SetActive(false);
    }

    void Update()
    {
        if (GameManager.instance != null && GameManager.instance.isGameOver) return;

        // [DIPERBARUI] Mendukung Escape (Keyboard) dan Tombol Start (Controller)
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetButtonDown(pauseInputController))
        {
            if (AudioManager.instance != null) AudioManager.instance.PlaySFX("Button Click"); 
            
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        panelPause.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;

        // [BARU] Sorot tombol Resume
        if (pauseFirstButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(pauseFirstButton);
        }
    }

    public void ResumeGame()
    {
        panelPause.SetActive(false);
        if (panelSettings != null) panelSettings.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        if (AudioManager.instance != null)
        {
            AudioManager.instance.blockAllSFX = true;
            AudioManager.instance.StopLoopingSFX();
        }

        InjectTriggerSingle[] allPlatforms = FindObjectsOfType<InjectTriggerSingle>();
        foreach (var p in allPlatforms) p.PrepareForSceneTransition();

        FadeManager.instance.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OpenSettings()
    {
        if (panelSettings != null)
        {
            panelSettings.SetActive(true);
            
            // [BARU] Sorot tombol pertama di dalam panel setting
            if (settingsFirstButton != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(settingsFirstButton);
            }
        }
    }

    public void BackToPauseMenu()
    {
        if (panelSettings != null) panelSettings.SetActive(false);
        
        // [BARU] Saat tutup setting, sorot kembali tombol Resume (atau tombol setting di menu utama pause)
        if (pauseFirstButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(pauseFirstButton);
        }
    }

    public void CloseSettings()
    {
        BackToPauseMenu(); // Gunakan logika yang sama
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