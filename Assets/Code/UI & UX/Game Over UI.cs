using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    public GameObject panelGameOver;

    void Start()
    {
        panelGameOver.SetActive(false);
    }

    void Update()
    {
        if (GameManager.instance != null && GameManager.instance.isGameOver)
        {
            panelGameOver.SetActive(true);
        }
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

    public void MainMenu()
    {
        SceneManager.LoadScene("Main Menu");//ganti nama scene dengan nama scene menu yang akan di load pertama kali (namanya harus sama ya(huruf kapital dan yang lainya))
        Debug.Log("Back To Menu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}