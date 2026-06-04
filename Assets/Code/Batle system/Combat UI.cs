using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ComboUI : MonoBehaviour
{
    public GameObject keyIconPrefab;
    
    [Header("Keyboard Sprites")]
    public Sprite spriteW;
    public Sprite spriteA;
    public Sprite spriteD;
    public Sprite spriteDown;
    public Sprite spriteLeft;
    public Sprite spriteRight;

    [Header("Controller Sprites P1 (Shapes PS)")]
    [Tooltip("Masukkan sprite tombol Segitiga")]
    public Sprite spriteTriangle; // Pengganti W
    [Tooltip("Masukkan sprite tombol Kotak")]
    public Sprite spriteSquare;   // Pengganti A
    [Tooltip("Masukkan sprite tombol Bulat")]
    public Sprite spriteCircle;   // Pengganti D

    [Header("Controller Sprites P2 (D-Pad)")]
    [Tooltip("Masukkan sprite panah bawah D-pad")]
    public Sprite spriteDpadDown;  // Pengganti DownArrow
    [Tooltip("Masukkan sprite panah kiri D-pad")]
    public Sprite spriteDpadLeft;  // Pengganti LeftArrow
    [Tooltip("Masukkan sprite panah kanan D-pad")]
    public Sprite spriteDpadRight; // Pengganti RightArrow

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
                // Fungsi GetSpriteForKey sekarang otomatis mendeteksi perangkat
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
            StartCoroutine(UIPopEffect(spawnedIcons[index]));
        }
    }

    private IEnumerator UIPopEffect(Image originalIcon)
    {
        GameObject popObj = new GameObject("UI_Pop");
        popObj.transform.SetParent(originalIcon.transform, false); 
        
        RectTransform rt = popObj.AddComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero; 
        rt.sizeDelta = originalIcon.rectTransform.sizeDelta; 
        rt.localScale = Vector3.one;

        Image popImage = popObj.AddComponent<Image>();
        popImage.sprite = originalIcon.sprite;
        popImage.color = new Color(1f, 1f, 1f, 0.7f); 

        float duration = 0.25f; 
        float elapsed = 0f;

        Vector3 startScale = Vector3.one;
        Vector3 targetScale = Vector3.one * 2.0f; 

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            rt.localScale = Vector3.Lerp(startScale, targetScale, t);
            
            Color c = popImage.color;
            c.a = Mathf.Lerp(0.7f, 0f, t);
            popImage.color = c;
            
            yield return null;
        }

        Destroy(popObj);
    }

    // --- [BARU] Fungsi Cek Koneksi Controller ---
    private bool IsControllerConnected()
    {
        string[] joysticks = Input.GetJoystickNames();
        foreach (string joy in joysticks)
        {
            // Jika ada nama joystick yang terdeteksi dan tidak kosong
            if (!string.IsNullOrEmpty(joy)) 
            {
                return true;
            }
        }
        return false;
    }

    // --- [DIPERBARUI] Pemilihan Sprite Dinamis ---
    Sprite GetSpriteForKey(KeyCode key)
    {
        bool useController = IsControllerConnected();

        switch (key)
        {
            // KELOMPOK INPUT PLAYER 1
            case KeyCode.W: 
                return useController ? spriteTriangle : spriteW;
            case KeyCode.A: 
                return useController ? spriteSquare : spriteA;
            case KeyCode.D: 
                return useController ? spriteCircle : spriteD;

            // KELOMPOK INPUT PLAYER 2
            case KeyCode.DownArrow: 
                return useController ? spriteDpadDown : spriteDown;
            case KeyCode.LeftArrow: 
                return useController ? spriteDpadLeft : spriteLeft;
            case KeyCode.RightArrow: 
                return useController ? spriteDpadRight : spriteRight;

            default: 
                return null;
        }
    }
}