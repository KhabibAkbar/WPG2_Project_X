using UnityEngine;

public class AudioManagerrr : MonoBehaviour
{
    public AudioSource audioSource;

    public AudioClip clickSound;
    public AudioClip winSound;
    public AudioClip loseSound;
    public AudioClip attackSound;
    public AudioClip shuffleSound;
    public AudioClip seriSound; // --- TAMBAHAN BARU: Slot untuk suara seri ---

    public void PlayClick()
    {
        if (audioSource != null && clickSound != null)
            audioSource.PlayOneShot(clickSound);
    }

    public void PlayWin()
    {
        if (audioSource != null && winSound != null)
            audioSource.PlayOneShot(winSound);
    }

    public void PlayLose()
    {
        if (audioSource != null && loseSound != null)
            audioSource.PlayOneShot(loseSound);
    }

    public void PlayAttack()
    {
        if (audioSource != null && attackSound != null)
            audioSource.PlayOneShot(attackSound);
    }

    public void PlayShuffle()
    {
        if (audioSource != null && shuffleSound != null)
            audioSource.PlayOneShot(shuffleSound);
    }

    // --- TAMBAHAN BARU: Fungsi untuk memutar suara seri ---
    public void PlaySeri()
    {
        if (audioSource != null && seriSound != null)
            audioSource.PlayOneShot(seriSound);
    }
}