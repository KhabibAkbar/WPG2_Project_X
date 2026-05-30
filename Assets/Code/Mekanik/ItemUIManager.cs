using UnityEngine;
using UnityEngine.UI;

public class ItemUIManager : MonoBehaviour
{
    public static ItemUIManager instance;

    public Image[] itemIcons;
    public Sprite collectedSprite;

    private int collectedCount = 0;

    public Penghalang platform;

    void Awake()
    {
        instance = this;
    }

    public void Collect(int id)
    {
        itemIcons[id].sprite = collectedSprite;

        collectedCount++;

        if (AudioManager.instance != null)
        {
            // Pastikan nama "Collect" ada di AudioManager Inspector
            AudioManager.instance.PlaySFX("Collect Item");
        }

        if (collectedCount >= 3)
        {
            platform.ActivatePlatform();
        }
    }
}