using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class CutsceneSlideshow : MonoBehaviour
{
    public Image[] images;
    public Image fadeImage;

    public float displayTime = 3f;
    public float fadeSpeed = 2f;

    public string nextScene;

    void Start()
    {
        Color c = fadeImage.color;
        c.a = 0;
        fadeImage.color = c;

        StartCoroutine(PlaySlideshow());
    }

    IEnumerator PlaySlideshow()
    {
        ShowImage(0);

        yield return new WaitForSeconds(displayTime);

        for (int i = 1; i < images.Length; i++)
        {
            yield return StartCoroutine(FadeOut());

            ShowImage(i);

            yield return StartCoroutine(FadeIn());

            yield return new WaitForSeconds(displayTime);
        }

        yield return StartCoroutine(FadeOut());

        FadeManager.instance.LoadScene(nextScene);
    }

    IEnumerator FadeOut()
    {
        Color c = fadeImage.color;

        while (c.a < 1)
        {
            c.a += Time.deltaTime * fadeSpeed;
            fadeImage.color = c;
            yield return null;
        }

        c.a = 1;
        fadeImage.color = c;
    }

    IEnumerator FadeIn()
    {
        Color c = fadeImage.color;

        while (c.a > 0)
        {
            c.a -= Time.deltaTime * fadeSpeed;
            fadeImage.color = c;
            yield return null;
        }

        c.a = 0;
        fadeImage.color = c;
    }

    void ShowImage(int index)
    {
        for (int i = 0; i < images.Length; i++)
        {
            images[i].gameObject.SetActive(false);
        }

        images[index].gameObject.SetActive(true);
    }
}