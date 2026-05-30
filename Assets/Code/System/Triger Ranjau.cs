using UnityEngine;
using System.Collections;

public class TriggerRanjau : MonoBehaviour
{
    [Header("Settings")]
    public string explosionSFXName = "Ranjau"; // Sesuaikan dengan nama di AudioManager

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(HitMine());
        }
    }

    IEnumerator HitMine()
    {
        // Mainkan suara
        if (AudioManager.instance != null)
            AudioManager.instance.PlaySFX(explosionSFXName);

        // Tunggu sebentar (misal 0.2 detik) agar ledakan terasa
        yield return new WaitForSeconds(0.2f);

        // Baru Game Over
        GameManager.instance.GameOver();
    }
}