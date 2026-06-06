using UnityEngine;
using System.Collections;

public class CutsceneManager : MonoBehaviour
{
    public Camera mainCam; 

    [Header("Players")]
    public Transform player1;
    [Tooltip("Kosongkan jika level 1 player")]
    public Transform player2; 

    public MovementP1 movementP1;
    public MovementP2 movementP2;

    [Header("Animasi Setup")]
    public Animator animP1;
    public Animator animP2;
    public string dropAnimTrigger = "Land";
    public float animWaitTime = 1.0f; 

    [Header("Camera & Bounds Setup")]
    public Collider2D mapBounds; 
    public float cinematicZoomSize = 3.5f;
    public float cameraTransitionDuration = 0.5f; 

    [Header("UI Setup")]
    [Tooltip("Masukkan semua panel UI (selain Dialog) yang ingin disembunyikan saat cutscene")]
    public GameObject[] uiToHide; 

    [Header("Dialogue Setup")]
    public DialogueManager dialogue;
    public Sprite spriteP1;
    public Sprite spriteP2;
    [TextArea] public string dialogP1;
    [TextArea] public string dialogP2;
    public GameObject blurEffect;
    
    [Header("Timing Setup")]
    [Tooltip("Waktu tunggu setelah layar terang sebelum mulai menyorot (Isi 1.6 jika ada gempa)")]
    public float initialWaitTime = 0.2f; 

    private Vector3 originalCamPos;
    private float originalCamSize;

    void Awake()
    {
        if (movementP1 != null) 
        {
            movementP1.enabled = false;
            movementP1.SetCutsceneLock(true); 
        }
        
        if (movementP2 != null) 
        {
            movementP2.enabled = false;
            movementP2.SetCutsceneLock(true); 
        }
    }

    void Start()
    {
        if (mainCam == null) mainCam = Camera.main;

        originalCamPos = mainCam.transform.position;
        originalCamSize = mainCam.orthographicSize;

        StartCoroutine(StartCutsceneSequence());
    }

    IEnumerator StartCutsceneSequence()
    {
        // 0. BERSIHKAN UI DI AWAL
        if (dialogue != null) dialogue.CloseDialogue(); 
        
        // --- SEMBUNYIKAN SEMUA UI GAMEPLAY (ANTI-CRASH) ---
        if (uiToHide != null)
        {
            foreach (GameObject ui in uiToHide)
            {
                if (ui != null) ui.SetActive(false);
            }
        }
        // -------------------------------------

        if (FadeManager.instance != null)
        {
            yield return new WaitUntil(() => FadeManager.instance.isFading == false);
            yield return new WaitForSeconds(initialWaitTime); 
        }

        float maxWaitTime = 2.0f; 

        // 2. SEQUENCE PLAYER 1
        if (player1 != null)
        {
            yield return StartCoroutine(MoveAndZoomCamera(player1.position, cinematicZoomSize, cameraTransitionDuration));
            
            if (movementP1 != null) 
            {
                movementP1.isGrounded = false; 
                movementP1.SetCutsceneLock(false); 
            }

            float p1Timer = 0f;
            while (movementP1 != null && !movementP1.isGrounded && p1Timer < maxWaitTime)
            {
                p1Timer += Time.deltaTime;
                yield return null; 
            }

            if (animP1 != null) animP1.SetTrigger(dropAnimTrigger);
            yield return new WaitForSeconds(animWaitTime);

            if (dialogue != null)
            {
                if (blurEffect != null) blurEffect.SetActive(true);
                dialogue.ShowDialogue(dialogP1, spriteP1, true);
                yield return new WaitUntil(() => dialogue.selesai);
                if (blurEffect != null) blurEffect.SetActive(false);
            }
            
            yield return new WaitForSeconds(0.2f); 
        }

        // 3. SEQUENCE PLAYER 2
        if (player2 != null && spriteP2 != null)
        {
            yield return StartCoroutine(MoveAndZoomCamera(player2.position, cinematicZoomSize, cameraTransitionDuration));
            
            if (movementP2 != null) 
            {
                movementP2.isGrounded
                 = false; 
                movementP2.SetCutsceneLock(false);
            }

            float p2Timer = 0f;
            while (movementP2 != null && !movementP2.isGrounded && p2Timer < maxWaitTime) 
            {
                p2Timer += Time.deltaTime;
                yield return null;
            }

            if (animP2 != null) animP2.SetTrigger(dropAnimTrigger);
            yield return new WaitForSeconds(animWaitTime);

            if (dialogue != null)
            {
                if (blurEffect != null) blurEffect.SetActive(true);
                dialogue.ShowDialogue(dialogP2, spriteP2, false);
                yield return new WaitUntil(() => dialogue.selesai);
                if (blurEffect != null) blurEffect.SetActive(false);
            }
            yield return new WaitForSeconds(0.2f);
        }

        // 4. ZOOM OUT KEMBALI
        yield return StartCoroutine(MoveAndZoomCamera(originalCamPos, originalCamSize, cameraTransitionDuration));

        // 5. MULAI GAMEPLAY UTAMA
        if (movementP1 != null) { movementP1.enabled = true; movementP1.SetCutsceneLock(false); }
        if (movementP2 != null) { movementP2.enabled = true; movementP2.SetCutsceneLock(false); }

        // --- MUNCULKAN KEMBALI SEMUA UI GAMEPLAY (ANTI-CRASH) ---
        if (uiToHide != null)
        {
            foreach (GameObject ui in uiToHide)
            {
                if (ui != null) ui.SetActive(true);
            }
        }
        // -------------------------------------------
    }

    IEnumerator MoveAndZoomCamera(Vector3 targetPosition, float targetSize, float duration)
    {
        Vector3 startPos = mainCam.transform.position;
        float startSize = mainCam.orthographicSize;
        float elapsed = 0f;

        float targetX = targetPosition.x;
        float targetY = targetPosition.y;

        if (mapBounds != null && mainCam.orthographic)
        {
            float camHeight = targetSize;
            float camWidth = targetSize * mainCam.aspect;
            float minX = mapBounds.bounds.min.x + camWidth;
            float maxX = mapBounds.bounds.max.x - camWidth;
            float minY = mapBounds.bounds.min.y + camHeight;
            float maxY = mapBounds.bounds.max.y - camHeight;

            if (minX < maxX) targetX = Mathf.Clamp(targetX, minX, maxX);
            else targetX = (minX + maxX) / 2f; 
            
            if (minY < maxY) targetY = Mathf.Clamp(targetY, minY, maxY);
            else targetY = (minY + maxY) / 2f;
        }

        Vector3 finalTargetPos = new Vector3(targetX, targetY, startPos.z); 

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            mainCam.transform.position = Vector3.Lerp(startPos, finalTargetPos, smoothT);
            mainCam.orthographicSize = Mathf.Lerp(startSize, targetSize, smoothT);
            yield return null;
        }

        mainCam.transform.position = finalTargetPos;
        mainCam.orthographicSize = targetSize;
    }
}