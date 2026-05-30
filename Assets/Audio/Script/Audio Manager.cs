using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;
    public AudioSource sfxLoopSource;

    [Header("BGM Clips")]
    public AudioClip mainmenu;
    public AudioClip cutscene;
    public AudioClip gameplayBGM;
    public AudioClip FightingBGM;

    [Header("SFX Clips")]
    public List<AudioClip> sfxClips;

    private Dictionary<string, AudioClip> bgmDict;
    private Dictionary<string, AudioClip> sfxDict;

    [HideInInspector] // Agar tidak membingungkan di Inspector
    public bool blockAllSFX = true;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Load Volume
        float savedBGM = PlayerPrefs.GetFloat("BGMVolume", 50f); // Default 50 jika baru main
        float savedSFX = PlayerPrefs.GetFloat("SFXVolume", 50f);

        bgmSource.volume = savedBGM / 100f;
        sfxSource.volume = savedSFX / 100f;
        if (sfxLoopSource != null) sfxLoopSource.volume = savedSFX / 100f;

        // Inisialisasi Dictionary
        bgmDict = new Dictionary<string, AudioClip>
        {
            { "Main Menu", mainmenu },
            { "Cutscene", cutscene },
            { "Gameplay", gameplayBGM },
            { "Fighting", FightingBGM },
        };

        sfxDict = new Dictionary<string, AudioClip>();
        foreach (var clip in sfxClips)
        {
            if (clip != null && !sfxDict.ContainsKey(clip.name))
                sfxDict.Add(clip.name, clip);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 1. Blokir suara saat pindah scene agar tidak ada suara sisa/sampah
        blockAllSFX = true;

        if (sfxSource != null) sfxSource.Stop();
        if (sfxLoopSource != null)
        {
            sfxLoopSource.Stop();
            sfxLoopSource.clip = null;
        }

        // 2. Jalankan Coroutine untuk membuka blokir secara otomatis
        StartCoroutine(AutoUnlockRoutine());

        // Switch BGM
        switch (scene.name)
        {
            case "Main Menu": PlayBGM("Main Menu"); break;
            case "Cutscene 01": PlayBGM("Cutscene"); break;
            case "Level 1":
            case "Level 2":
            case "Level 3":
            case "Level 4": PlayBGM("Gameplay"); break;
            case "Level 5": PlayBGM("Fighting"); break;
        }
    }

    // --- BAGIAN BARU: Pembuka Blokir Otomatis ---
    IEnumerator AutoUnlockRoutine()
    {
        // Tunggu sebentar (0.2 detik) untuk memastikan transisi scene selesai
        // dan suara sampah sudah hilang.
        yield return new WaitForSeconds(0.2f);
        blockAllSFX = false;
        Debug.Log("<color=cyan>SFX Block Released.</color>");
    }

    public void PlayBGM(string name, bool loop = true)
    {
        if (!bgmDict.TryGetValue(name, out var clip) || clip == null) return;
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        bgmSource.Stop();
        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.Play();
    }

    public void PlaySFX(string name)
    {
        if (blockAllSFX) return; // Menghalangi suara sampah saat loading
        if (!sfxDict.TryGetValue(name, out var clip)) return;
        if (sfxSource.volume <= 0f) return;

        sfxSource.PlayOneShot(clip);
    }

    // --- BARU: FUNGSI UNTUK MEMAINKAN SUARA DENGAN PITCH/KECEPATAN CUSTOM ---
    public void PlaySFXPitched(string name, float customPitch)
    {
        if (blockAllSFX) return; 
        if (!sfxDict.TryGetValue(name, out var clip)) return;
        if (sfxSource.volume <= 0f) return;

        // Buat 'speaker' sementara khusus untuk suara lambat/cepat
        // Agar tidak merusak kecepatan SFX pukulan/klik yang lain
        GameObject tempAudioObj = new GameObject("TempPitchedSFX_" + name);
        AudioSource tempSource = tempAudioObj.AddComponent<AudioSource>();
        
        tempSource.clip = clip;
        tempSource.volume = sfxSource.volume;
        tempSource.pitch = customPitch;
        tempSource.Play();

        // Hancurkan speaker siluman ini tepat saat suaranya selesai
        Destroy(tempAudioObj, clip.length / customPitch);
    }

    // Gunakan ini untuk suara yang HARUS bunyi saat Start Scene (seperti Platform1)
    public void PlaySFXDirect(string name)
    {
        if (!sfxDict.TryGetValue(name, out var clip)) return;
        if (sfxSource.volume <= 0f) return;

        sfxSource.PlayOneShot(clip);
        Debug.Log("<color=green>Direct SFX Played: </color>" + name);
    }

    public void PlayLoopingSFX(string name)
    {
        if (blockAllSFX) return;
        if (!sfxDict.TryGetValue(name, out var clip)) return;
        if (sfxLoopSource == null || sfxLoopSource.volume <= 0f) return;

        if (sfxLoopSource.clip != clip) sfxLoopSource.clip = clip;
        if (!sfxLoopSource.isPlaying) sfxLoopSource.Play();
    }

    public void StopLoopingSFX()
    {
        if (sfxLoopSource != null) sfxLoopSource.Stop();
    }

    public void SetBGMVolume(float value)
    {
        bgmSource.volume = value / 100f;
        PlayerPrefs.SetFloat("BGMVolume", value);
    }

    public void SetSFXVolume(float value)
    {
        float vol = value / 100f;
        sfxSource.volume = vol;
        if (sfxLoopSource != null) sfxLoopSource.volume = vol;
        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Fungsi manual jika dibutuhkan oleh script lain
    public void AllowSFX()
    {
        blockAllSFX = false;
    }

    public void PlayLoopingSFXDirect(string name)
    {
        // 1. Pastikan Dictionary terisi
        if (sfxDict == null || sfxDict.Count == 0)
        {
            sfxDict = new Dictionary<string, AudioClip>();
            foreach (var clip in sfxClips)
            {
                if (clip != null && !sfxDict.ContainsKey(clip.name))
                    sfxDict.Add(clip.name, clip);
            }
        }

        // 2. Cari clip tanpa peduli huruf besar/kecil (Menghindari error p kecil vs P besar)
        AudioClip foundClip = null;
        foreach (var kvp in sfxDict)
        {
            if (string.Equals(kvp.Key, name, System.StringComparison.OrdinalIgnoreCase))
            {
                foundClip = kvp.Value;
                break;
            }
        }

        // 3. Eksekusi jika ditemukan
        if (foundClip != null)
        {
            if (sfxLoopSource == null) return;

            float savedSFX = PlayerPrefs.GetFloat("SFXVolume", 50f);
            sfxLoopSource.volume = savedSFX / 100f;

            sfxLoopSource.clip = foundClip;
            sfxLoopSource.loop = true;
            sfxLoopSource.Play();

            Debug.Log("<color=yellow>Looping SFX Berhasil: </color>" + foundClip.name);
        }
        else
        {
            Debug.LogWarning("AudioManager: Nama '" + name + "' tidak ditemukan di list SFX Clips!");
        }
    }
}