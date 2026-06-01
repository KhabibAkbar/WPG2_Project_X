using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ComboUI : MonoBehaviour
{
    public GameObject keyIconPrefab;
    // Slot Particle 3D sudah dihapus karena kita pakai trik UI murni
    
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
            // 1. Ubah icon asli menjadi putih terang
            spawnedIcons[index].color = Color.white;
            spawnedIcons[index].transform.localScale = Vector3.one * 1.2f;

            // 2. Panggil efek ledakan UI 2D (Sangat Juicy & Aman!)
            StartCoroutine(UIPopEffect(spawnedIcons[index]));
        }
    }

    // --- FITUR BARU: Animasi UI Pop ---
    private IEnumerator UIPopEffect(Image originalIcon)
    {
        // 1. Buat objek baru
        GameObject popObj = new GameObject("UI_Pop");
        
        // 2. JADIKAN ANAK DARI ICON ASLI (agar tidak diculik oleh HorizontalLayoutGroup)
        popObj.transform.SetParent(originalIcon.transform, false); 
        
        // 3. Gunakan RectTransform agar posisinya absolut di tengah parent-nya
        RectTransform rt = popObj.AddComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero; // Mengunci persis di tengah tombol
        rt.sizeDelta = originalIcon.rectTransform.sizeDelta; // Menyamakan ukuran
        rt.localScale = Vector3.one;

        // 4. Pasang komponen gambar dan tiru icon yang dipencet
        Image popImage = popObj.AddComponent<Image>();
        popImage.sprite = originalIcon.sprite;
        
        // Beri warna ledakan (Putih dengan transparansi 70%)
        popImage.color = new Color(1f, 1f, 1f, 0.7f); 

        float duration = 0.25f; // Kecepatan efek (0.25 detik)
        float elapsed = 0f;

        Vector3 startScale = Vector3.one;
        Vector3 targetScale = Vector3.one * 2.0f; // Efek membesar 2x lipat

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Animasi membesar
            rt.localScale = Vector3.Lerp(startScale, targetScale, t);
            
            // Animasi memudar (Alpha dari 0.7 perlahan hilang menjadi 0)
            Color c = popImage.color;
            c.a = Mathf.Lerp(0.7f, 0f, t);
            popImage.color = c;
            
            yield return null;
        }

        // Hancurkan efeknya agar tidak memberatkan memori
        Destroy(popObj);
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
            default: return null;
        }
    }
}