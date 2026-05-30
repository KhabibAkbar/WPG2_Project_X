using UnityEngine;

public class PlayerSoundAnimator : MonoBehaviour
{
    public void PlayLandSound()
    {
        // 1. Baris ini untuk mengecek apakah fungsinya benar-benar terpanggil oleh animasi
        Debug.Log("TEST: Fungsi PlayLandSound berhasil terpanggil dari Animation Event!");

        // 2. Baris ini untuk memanggil suaranya
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX("Player terkena hit"); // Ganti dengan nama SFX Anda
        }
        else
        {
            Debug.LogWarning("TEST: AudioManager tidak ditemukan di scene ini!");
        }
    }
}