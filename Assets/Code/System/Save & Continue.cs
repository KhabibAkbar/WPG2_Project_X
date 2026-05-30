using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameDataHandler : MonoBehaviour
{
    public Button continueButton;

    void Start()
    {
        if (continueButton != null)
        {
            CheckContinueStatus();
        }
    }

    public void CheckContinueStatus()
    {
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
            // --- FIX: Kembalikan waktu menjadi normal sebelum pindah scene ---
            Time.timeScale = 1f; 

            string sceneToLoad = PlayerPrefs.GetString("LastScene");
            FadeManager.instance.LoadScene(sceneToLoad);
        }
    }

    public void StartNewGame(string firstLevelName)
    {
        // --- FIX: Kembalikan waktu menjadi normal sebelum pindah scene ---
        Time.timeScale = 1f; 

        SceneManager.LoadScene(firstLevelName);
    }
}