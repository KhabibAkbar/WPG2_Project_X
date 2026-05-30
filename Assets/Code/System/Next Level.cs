using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevel : MonoBehaviour
{
    public int jumlahPlayer = 2;

    [Header("Audio Settings")]
    public string nextSFXName = "Next"; // Pastikan nama ini sama dengan di AudioManager

    private int playerDiArea = 0;
    private bool levelSelesai = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerDiArea++;

            if (playerDiArea >= jumlahPlayer && !levelSelesai)
            {
                levelSelesai = true;

                // --- BAGIAN SFX ---
                if (AudioManager.instance != null)
                {
                    // Gunakan PlaySFX biasa, FadeManager nanti akan mengurus pembersihan audionya
                    AudioManager.instance.PlaySFX(nextSFXName);
                }

                // Pindah Scene
                if (FadeManager.instance != null)
                {
                    FadeManager.instance.LoadScene(
                        SceneManager.GetActiveScene().buildIndex + 1
                    );
                }
                else
                {
                    // Fallback jika FadeManager tidak ada di scene
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Mencegah playerDiArea menjadi minus jika terjadi bug physics
            playerDiArea = Mathf.Max(0, playerDiArea - 1);
        }
    }
}