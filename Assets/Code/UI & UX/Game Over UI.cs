using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems; // [BARU]

public class GameOverUI : MonoBehaviour
{
    [Header("UI Controller Setup")]
    public GameObject gameOverFirstButton; // Tombol Restart

    public GameObject panelGameOver;
    private bool isPanelActive = false; // Mencegah pemanggilan SetSelected terus menerus

    void Start()
    {
        panelGameOver.SetActive(false);
    }

    void Update()
    {
        if (GameManager.instance != null && GameManager.instance.isGameOver && !isPanelActive)
        {
            panelGameOver.SetActive(true);
            isPanelActive = true; // Tandai panel sudah nyala

            // [BARU] Sorot tombol Restart
            if (gameOverFirstButton != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(gameOverFirstButton);
            }
        }
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

    public void MainMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}