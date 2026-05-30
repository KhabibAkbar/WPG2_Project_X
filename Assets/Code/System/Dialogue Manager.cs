using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    [Header("UI Player 1 (Kiri)")]
    public GameObject panelP1;
    public TextMeshProUGUI textP1;
    public Image charImageP1;

    [Header("UI Player 2 (Kanan)")]
    [Tooltip("Boleh dikosongkan jika level hanya 1 player")]
    public GameObject panelP2;
    public TextMeshProUGUI textP2;
    public Image charImageP2;

    [Header("Blur")]
    public GameObject blurEffect;

    [Header("Typing Settings")]
    public float typingSpeed = 0.03f;
    [Range(1, 5)]
    public int playSoundEveryXChars = 2;
    public string typingSFXName = "Teks";

    public bool selesai = false;
    private bool isTyping = false;
    private bool isDialogueActive = false; // BARU: Menandakan dialog sedang ada di layar

    // BARU: Variabel pelacak untuk fitur Skip
    private string currentFullText = "";
    private TextMeshProUGUI currentTextUI;
    private Coroutine typingCoroutine;
    private Coroutine autoCloseCoroutine;

    void Update()
    {
        // Mengecek input Space hanya ketika dialog sedang aktif
        if (isDialogueActive && Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                // SKIP TAHAP 1: Jika sedang ngetik, langsung tampilkan semua teks
                if (typingCoroutine != null) StopCoroutine(typingCoroutine);
                if (currentTextUI != null) currentTextUI.text = currentFullText;
                isTyping = false; // Membiarkan AutoClose mulai menghitung jeda tutup
            }
            else
            {
                // SKIP TAHAP 2: Jika teks sudah lengkap (sedang jeda AutoClose), langsung tutup
                if (autoCloseCoroutine != null) StopCoroutine(autoCloseCoroutine);
                CloseDialogue();
            }
        }
    }

    public void ShowDialogue(string text, Sprite character, bool isPlayer1)
    {
        if (AudioManager.instance != null) AudioManager.instance.StopLoopingSFX();
        StopAllCoroutines();
        
        selesai = false;
        isDialogueActive = true;
        currentFullText = text;

        if (blurEffect != null) blurEffect.SetActive(true);

        // LOGIKA DINAMIS: Cek apakah P2 ada, jika tidak ada selalu arahkan ke P1
        bool useP1 = isPlayer1 || panelP2 == null;

        if (useP1)
        {
            if (panelP1 != null) panelP1.SetActive(true);
            if (panelP2 != null) panelP2.SetActive(false);
            if (charImageP1 != null) charImageP1.sprite = character;

            currentTextUI = textP1;
            if (textP1 != null) typingCoroutine = StartCoroutine(TypeText(textP1, text));
        }
        else
        {
            if (panelP2 != null) panelP2.SetActive(true);
            if (panelP1 != null) panelP1.SetActive(false);
            if (charImageP2 != null) charImageP2.sprite = character;

            currentTextUI = textP2;
            if (textP2 != null) typingCoroutine = StartCoroutine(TypeText(textP2, text));
        }

        autoCloseCoroutine = StartCoroutine(AutoClose());
    }

    IEnumerator TypeText(TextMeshProUGUI textUI, string fullText)
    {
        isTyping = true;
        textUI.text = "";
        int charCounter = 0;

        for (int i = 0; i < fullText.Length; i++)
        {
            textUI.text += fullText[i];
            charCounter++;

            if (charCounter % playSoundEveryXChars == 0)
            {
                if (AudioManager.instance != null)
                {
                    AudioManager.instance.PlaySFX(typingSFXName);
                }
            }

            if (fullText[i] == ' ')
                yield return new WaitForSeconds(typingSpeed * 2);
            else
                yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    IEnumerator AutoClose()
    {
        yield return new WaitUntil(() => isTyping == false);
        yield return new WaitForSeconds(0.7f);
        CloseDialogue();
    }

    public void CloseDialogue()
    {
        if (AudioManager.instance != null) AudioManager.instance.StopLoopingSFX();

        if (panelP1 != null) panelP1.SetActive(false);
        if (panelP2 != null) panelP2.SetActive(false);
        if (blurEffect != null) blurEffect.SetActive(false);

        isDialogueActive = false; // Dialog sudah tidak ada di layar
        selesai = true;
    }
}