using UnityEngine;
using UnityEngine.SceneManagement;

public class Continue : MonoBehaviour
{
    public void Continue_Game()
    {
        int currentLevel = SceneManager.GetActiveScene().buildIndex;

        PlayerPrefs.SetInt ("LastLevel", currentLevel + 1); 
        PlayerPrefs.Save(); 
    }
}
