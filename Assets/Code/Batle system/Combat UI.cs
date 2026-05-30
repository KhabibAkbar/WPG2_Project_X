using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ComboUI : MonoBehaviour
{
    public GameObject keyIconPrefab;
    // SpriteUp dihilangkan dari daftar ini
    public Sprite spriteW, spriteA, spriteD, spriteDown, spriteLeft, spriteRight;

    private List<Image> spawnedIcons = new List<Image>(); 

    public void SetupComboUI(List<KeyCode> combo)
    {
        if (spawnedIcons == null) spawnedIcons = new List<Image>();
        foreach (var icon in spawnedIcons) if (icon != null) Destroy(icon.gameObject);
        spawnedIcons.Clear();

        foreach (KeyCode key in combo)
        {
            GameObject newIcon = Instantiate(keyIconPrefab, transform);
            
            RectTransform rt = newIcon.GetComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;
            rt.localScale = Vector3.one;

            Image img = newIcon.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = GetSpriteForKey(key);
                img.color = Color.gray; 
                spawnedIcons.Add(img);
            }
        }
    }

    public void UpdateKeyColor(int index)
    {
        if (index < spawnedIcons.Count)
        {
            spawnedIcons[index].color = Color.white;
            spawnedIcons[index].transform.localScale = Vector3.one * 1.2f;
        }
    }

    Sprite GetSpriteForKey(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.W: return spriteW;
            case KeyCode.A: return spriteA;
            case KeyCode.D: return spriteD;
            case KeyCode.DownArrow: return spriteDown;
            case KeyCode.LeftArrow: return spriteLeft;
            case KeyCode.RightArrow: return spriteRight;
            // KeyCode.UpArrow dihapus dari sini agar tidak bisa diproses secara visual
            default: return null;
        }
    }
}