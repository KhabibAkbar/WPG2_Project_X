using UnityEngine;
using UnityEngine.EventSystems; // Wajib dipanggil untuk mendeteksi UI

// Kita wajibkan script ini memakai Interface dari EventSystem
public class UIButtonScaleEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [Header("Scale Settings")]
    [Tooltip("Seberapa besar perubahannya? 1.1 = Membesar 10%")]
    public float scaleMultiplier = 1.1f; 
    
    [Tooltip("Seberapa cepat animasinya bergerak?")]
    public float animationSpeed = 15f; 

    private Vector3 originalScale;
    private bool isSelectedOrHovered = false;

    void Start()
    {
        // Simpan ukuran asli objek ini saat game dimulai
        originalScale = transform.localScale;
    }

    void Update()
    {
        // Tentukan target ukuran: Jika disorot, kalikan ukuran aslinya. Jika tidak, kembali ke asli.
        Vector3 targetScale = isSelectedOrHovered ? originalScale * scaleMultiplier : originalScale;

        // Gunakan Lerp untuk membuat transisi perbesaran yang halus (smooth)
        // [PENTING] Kita menggunakan unscaledDeltaTime agar animasi tetap jalan walau game di-Pause (TimeScale = 0)
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * animationSpeed);
    }

    // --- DETEKSI KURSOR MOUSE ---
    public void OnPointerEnter(PointerEventData eventData)
    {
        isSelectedOrHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isSelectedOrHovered = false;
    }

    // --- DETEKSI CONTROLLER / D-PAD ---
    public void OnSelect(BaseEventData eventData)
    {
        isSelectedOrHovered = true;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        isSelectedOrHovered = false;
    }
    
    // --- RESET AMAN SAAT MENU DIMATIKAN ---
    void OnDisable()
    {
        // Mencegah bug ukuran tersangkut saat panel tiba-tiba dimatikan
        isSelectedOrHovered = false;
        transform.localScale = originalScale;
    }
}