using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class FadeManager : MonoBehaviour
{
    public static FadeManager instance;

    public Image fadeImage;
    public float fadeSpeed = 2f;

    // SUDAH DIPERBAIKI: Menjadi public
    public bool isFading = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(FadeIn());
    }

    public IEnumerator FadeIn()
    {
        if (isFading) yield break;

        isFading = true;

        Color c = fadeImage.color;

        while (c.a > 0)
        {
            c.a -= Time.deltaTime * fadeSpeed;
            fadeImage.color = c;
            yield return null;
        }

        isFading = false;

        if (AudioManager.instance != null)
        {
            AudioManager.instance.AllowSFX();
        }
    }

    public IEnumerator FadeOut()
    {
        if (isFading) yield break;

        isFading = true;

        Color c = fadeImage.color;

        while (c.a < 1)
        {
            c.a += Time.deltaTime * fadeSpeed;
            fadeImage.color = c;
            yield return null;
        }

        isFading = false;
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneRoutine(sceneName));
    }
    public void LoadScene(int sceneIndex)
    {
        StartCoroutine(LoadSceneRoutine(sceneIndex));
    }

    IEnumerator LoadSceneRoutine(int sceneIndex)
    {
        yield return new WaitForSeconds(0.1f);

        if (AudioManager.instance != null)
        {
            AudioManager.instance.blockAllSFX = true;
            AudioManager.instance.StopLoopingSFX();
        }

        InjectTriggerSingle[] platforms = FindObjectsOfType<InjectTriggerSingle>();
        foreach (var p in platforms)
        {
            p.PrepareForSceneTransition();
        }

        yield return StartCoroutine(FadeOut());
        SceneManager.LoadScene(sceneIndex);
    }

    IEnumerator LoadSceneRoutine(string sceneName)
    {
        yield return new WaitForSeconds(0.1f);

        if (AudioManager.instance != null)
        {
            AudioManager.instance.blockAllSFX = true;
            AudioManager.instance.StopLoopingSFX();
        }

        InjectTriggerSingle[] platforms = FindObjectsOfType<InjectTriggerSingle>();
        foreach (var p in platforms)
        {
            p.PrepareForSceneTransition();
        }

        yield return StartCoroutine(FadeOut());
        SceneManager.LoadScene(sceneName);
    }
}