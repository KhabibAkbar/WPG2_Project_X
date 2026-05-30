using UnityEngine;
using UnityEngine.SceneManagement;

public class UIMainMenu : MonoBehaviour
{
    // Hapus variabel string di atas, kita tidak butuh lagi

    public void Play_Game()
    {
        FadeManager.instance.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        Debug.Log("Play Game");
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

    // Tambahkan (string namaSceneTujuan) di dalam tanda kurung
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