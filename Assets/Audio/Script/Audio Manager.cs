using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    [System.Serializable]
    public class SoundSetting
    {
        public AudioClip clip;
        [Range(0f, 1f)] public float volumeModifier = 1f;
    }

    public static AudioManager instance;

    [Header("Audio Sources")]
    public AudioSource bgmSource; // Speaker BGM Utama
    public AudioSource sfxSource;
    public AudioSource sfxLoopSource;

    [Header("BGM Clips")]
    public SoundSetting mainmenu;
    public SoundSetting cutscene;
    public SoundSetting gameplayBGM;
    public SoundSetting FightingBGM;

    [Header("SFX Clips")]
    public List<SoundSetting> sfxClips;

    private Dictionary<string, SoundSetting> bgmDict;
    private Dictionary<string, SoundSetting> sfxDict;

    [HideInInspector] 
    public bool blockAllSFX = true;

    private float currentBgmModifier = 1f;
    private float currentSfxLoopModifier = 1f;

    private Coroutine bgmFadeRoutine;

    // --- VARIABEL BARU: Untuk Crossfade ---
    private AudioSource activeBgmSource; 
    private AudioSource nextBgmSource;   

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

        // Setup Dictionary
        bgmDict = new Dictionary<string, SoundSetting>
        {
            { "Main Menu", mainmenu },
            { "Cutscene", cutscene },
            { "Gameplay", gameplayBGM },
            { "Fighting", FightingBGM },
        };

        sfxDict = new Dictionary<string, SoundSetting>();
        foreach (var sound in sfxClips)
        {
            if (sound.clip != null && !sfxDict.ContainsKey(sound.clip.name))
                sfxDict.Add(sound.clip.name, sound);
        }

        // --- SISTEM SPEAKER GANDA UNTUK CROSSFADE ---
        activeBgmSource = bgmSource;
        
        // Memunculkan speaker cadangan (nextBgmSource) secara otomatis di belakang layar
        nextBgmSource = gameObject.AddComponent<AudioSource>();
        nextBgmSource.playOnAwake = false;
        nextBgmSource.loop = true;

        // Load Volume
        float savedBGM = PlayerPrefs.GetFloat("BGMVolume", 50f); 
        float savedSFX = PlayerPrefs.GetFloat("SFXVolume", 50f);

        activeBgmSource.volume = (savedBGM / 100f);
        sfxSource.volume = (savedSFX / 100f);
        if (sfxLoopSource != null) sfxLoopSource.volume = (savedSFX / 100f);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        blockAllSFX = true;

        if (sfxSource != null) sfxSource.Stop();
        if (sfxLoopSource != null)
        {
            sfxLoopSource.Stop();
            sfxLoopSource.clip = null;
        }

        StartCoroutine(AutoUnlockRoutine());

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

    IEnumerator AutoUnlockRoutine()
    {
        yield return new WaitForSeconds(0.2f);
        blockAllSFX = false;
    }

    public void PlayBGM(string name, bool loop = true)
    {
        if (!bgmDict.TryGetValue(name, out var sound) || sound.clip == null) return;
        
        // Cek jika lagu yang diminta sudah sedang menyala, abaikan agar tidak mengulang dari awal
        if (activeBgmSource.clip == sound.clip && activeBgmSource.isPlaying) return;

        if (bgmFadeRoutine != null) StopCoroutine(bgmFadeRoutine);
        
        // Waktu Crossfade disetel ke 1.5 detik (Bisa kamu ubah sesuka hati)
        bgmFadeRoutine = StartCoroutine(CrossFadeBGM(sound, 1.5f));
    }

    // --- LOGIKA CROSSFADE BGM (MENYILANG) ---
    private IEnumerator CrossFadeBGM(SoundSetting newSound, float transitionTime)
    {
        // 1. Siapkan lagu baru di speaker cadangan (nextBgmSource)
        nextBgmSource.clip = newSound.clip;
        currentBgmModifier = newSound.volumeModifier;
        nextBgmSource.volume = 0f;
        nextBgmSource.Play();

        float savedBGM = PlayerPrefs.GetFloat("BGMVolume", 50f);
        float targetVolume = (savedBGM / 100f) * currentBgmModifier;
        float startVolume = activeBgmSource.volume;

        // 2. Turunkan volume lagu lama BERSAMAAN dengan menaikkan volume lagu baru
        for (float t = 0; t < transitionTime; t += Time.deltaTime)
        {
            float progress = t / transitionTime;
            
            // Lagu lama perlahan mengecil
            activeBgmSource.volume = Mathf.Lerp(startVolume, 0f, progress);
            
            // Lagu baru perlahan membesar
            nextBgmSource.volume = Mathf.Lerp(0f, targetVolume, progress);
            
            yield return null;
        }

        // 3. Pastikan volume mencapai target di akhir
        activeBgmSource.volume = 0f;
        activeBgmSource.Stop();
        nextBgmSource.volume = targetVolume;

        // 4. Tukar peran speaker (Cadangan menjadi Utama)
        AudioSource temp = activeBgmSource;
        activeBgmSource = nextBgmSource;
        nextBgmSource = temp;
    }

    public void PlaySFX(string name)
    {
        if (blockAllSFX) return; 
        if (!sfxDict.TryGetValue(name, out var sound)) return;
        if (sfxSource.volume <= 0f) return;

        sfxSource.PlayOneShot(sound.clip, sound.volumeModifier);
    }

    public void PlaySFXPitched(string name, float customPitch)
    {
        if (blockAllSFX) return; 
        if (!sfxDict.TryGetValue(name, out var sound)) return;
        if (sfxSource.volume <= 0f) return;

        GameObject tempAudioObj = new GameObject("TempPitchedSFX_" + name);
        AudioSource tempSource = tempAudioObj.AddComponent<AudioSource>();

        tempSource.clip = sound.clip;
        tempSource.volume = sfxSource.volume * sound.volumeModifier; 
        tempSource.pitch = customPitch;
        tempSource.Play();

        Destroy(tempAudioObj, sound.clip.length / customPitch);
    }

    public void PlaySFXDirect(string name)
    {
        if (!sfxDict.TryGetValue(name, out var sound)) return;
        if (sfxSource.volume <= 0f) return;

        sfxSource.PlayOneShot(sound.clip, sound.volumeModifier);
    }

    public void PlayLoopingSFX(string name)
    {
        if (blockAllSFX) return;
        if (!sfxDict.TryGetValue(name, out var sound)) return;
        if (sfxLoopSource == null || sfxLoopSource.volume <= 0f) return;

        currentSfxLoopModifier = sound.volumeModifier;
        float savedSFX = PlayerPrefs.GetFloat("SFXVolume", 50f);
        sfxLoopSource.volume = (savedSFX / 100f) * currentSfxLoopModifier;

        if (sfxLoopSource.clip != sound.clip) sfxLoopSource.clip = sound.clip;
        if (!sfxLoopSource.isPlaying) sfxLoopSource.Play();
    }

    public void StopLoopingSFX()
    {
        if (sfxLoopSource != null) sfxLoopSource.Stop();
    }

    public void SetBGMVolume(float value)
    {
        PlayerPrefs.SetFloat("BGMVolume", value);
        // Pastikan mengatur volume pada speaker yang sedang aktif bernyanyi
        if (activeBgmSource != null) 
            activeBgmSource.volume = (value / 100f) * currentBgmModifier;
    }

    public void SetSFXVolume(float value)
    {
        PlayerPrefs.SetFloat("SFXVolume", value);
        float vol = value / 100f;
        sfxSource.volume = vol;
        if (sfxLoopSource != null) sfxLoopSource.volume = vol * currentSfxLoopModifier;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void AllowSFX()
    {
        blockAllSFX = false;
    }

    public void PlayLoopingSFXDirect(string name)
    {
        if (sfxDict == null || sfxDict.Count == 0) return; 

        SoundSetting foundSound = null;
        foreach (var kvp in sfxDict)
        {
            if (string.Equals(kvp.Key, name, System.StringComparison.OrdinalIgnoreCase))
            {
                foundSound = kvp.Value;
                break;
            }
        }

        if (foundSound != null)
        {
            if (sfxLoopSource == null) return;

            currentSfxLoopModifier = foundSound.volumeModifier;
            float savedSFX = PlayerPrefs.GetFloat("SFXVolume", 50f);
            sfxLoopSource.volume = (savedSFX / 100f) * currentSfxLoopModifier;

            sfxLoopSource.clip = foundSound.clip;
            sfxLoopSource.loop = true;
            sfxLoopSource.Play();
        }
    }
}