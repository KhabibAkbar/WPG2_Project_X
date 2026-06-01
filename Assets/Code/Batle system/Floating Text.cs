using UnityEngine;
using TMPro;
using System.Collections;

public class FloatingText : MonoBehaviour
{
    public float moveSpeed = 50f; 
    public float destroyTime = 1f;
    
    private TextMeshProUGUI tmpro;
    private CanvasGroup canvasGroup;

    public void Setup(string textMsg, Color textColor)
    {
        tmpro = GetComponent<TextMeshProUGUI>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (tmpro != null)
        {
            tmpro.text = textMsg;
            tmpro.color = textColor;

            // ==========================================
            // --- EFEK GLOW / MEMANCARKAN CAHAYA ---
            // ==========================================
            // Kita ambil instance material dari teks ini
            Material textMat = tmpro.fontMaterial; 
            
            // Pastikan fitur Glow menyala
            textMat.EnableKeyword("GLOW_ON"); 

            // Samakan warna pendaran cahaya dengan warna teks utama (merah/hijau)
            textMat.SetColor("_GlowColor", textColor); 

            // Atur seberapa tebal dan kuat cahayanya memancar
            textMat.SetFloat("_GlowOuter", 0.4f); // Jarak pendaran cahaya (0.0 - 1.0)
            textMat.SetFloat("_GlowPower", 0.8f); // Kekuatan cahaya (0.0 - 1.0)
            // ==========================================
        }

        // Teks membesar sedikit di awal (efek pop)
        transform.localScale = Vector3.one * 1.5f; 
        
        StartCoroutine(AnimateText());
    }

    private IEnumerator AnimateText()
    {
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;
        
        while (elapsed < destroyTime)
        {
            elapsed += Time.deltaTime;

            // 1. Bergerak melayang ke atas
            transform.position += new Vector3(0, moveSpeed * Time.deltaTime, 0);

            // 2. Skala kembali normal setelah meledak membesar
            transform.localScale = Vector3.Lerp(startScale, Vector3.one, elapsed / (destroyTime * 0.5f));

            // 3. Mulai memudar di separuh waktu terakhir
            if (elapsed > destroyTime * 0.5f && canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, (elapsed - (destroyTime * 0.5f)) / (destroyTime * 0.5f));
            }

            yield return null;
        }

        Destroy(gameObject); // Hancurkan setelah selesai
    }
}