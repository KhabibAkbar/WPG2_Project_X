using UnityEngine;
using UnityEngine.SceneManagement;

public class Continue : MonoBehaviour
{
    public void Continue_Game()
    {
        // Ambil index scene saat ini, lalu tambah 1 untuk scene berikutnya
        int nextLevelIndex = SceneManager.GetActiveScene().buildIndex + 1;

        // Dapatkan path/nama dari scene berikutnya berdasarkan index
        string nextLevelName = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(nextLevelIndex));

        // Simpan menggunakan kunci "LastScene" agar terbaca oleh GameDataHandler
        PlayerPrefs.SetString("LastScene", nextLevelName); 
        PlayerPrefs.Save(); 
        
        Debug.Log("Level berikutnya terbuka: " + nextLevelName);
    }
}