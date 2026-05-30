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
    [Tooltip("Masukkan Collider2D yang menutupi seluruh map abu-abu sebagai batas")]
    public Collider2D mapBounds; 
    public float cinematicZoomSize = 3.5f;
    public float cameraTransitionDuration = 0.5f; 

    [Header("Dialogue Setup")]
    public DialogueManager dialogue;
    public Sprite spriteP1;
    public Sprite spriteP2;
    [TextArea] public string dialogP1;
    [TextArea] public string dialogP2;
    public GameObject blurEffect;

    private Vector3 originalCamPos;
    private float originalCamSize;

    void Start()
    {
        if (mainCam == null) mainCam = Camera.main;

        originalCamPos = mainCam.transform.position;
        originalCamSize = mainCam.orthographicSize;

        StartCoroutine(StartCutsceneSequence());
    }

    IEnumerator StartCutsceneSequence()
    {
        // 0. BERSIHKAN UI YANG BOCOR SAAT START
        if (dialogue != null) dialogue.CloseDialogue(); 

        // 1. KUNCI PERGERAKAN PLAYER
        if (movementP1 != null) movementP1.enabled = false;
        if (movementP2 != null) movementP2.enabled = false;

        // 2. TUNGGU FADE MANAGER SELESAI MEMBUKA LAYAR
        if (FadeManager.instance != null)
        {
            yield return new WaitUntil(() => FadeManager.instance.isFading == false);
            yield return new WaitForSeconds(0.2f); // Jeda bernapas sedikit sebelum kamera jalan
        }

        // 3. SEQUENCE PLAYER 1 (Zoom -> Animasi -> Dialog)
        if (player1 != null)
        {
            // Panning & Zoom ke P1
            yield return StartCoroutine(MoveAndZoomCamera(player1.position, cinematicZoomSize, cameraTransitionDuration));
            
            // Trigger Animasi Jatuh & Suara (dari Animation Event)
            if (animP1 != null) animP1.SetTrigger(dropAnimTrigger);
            
            // Tunggu animasi selesai
            yield return new WaitForSeconds(animWaitTime);

            // Dialog P1
            if (blurEffect != null) blurEffect.SetActive(true);
            dialogue.ShowDialogue(dialogP1, spriteP1, true);
            yield return new WaitUntil(() => dialogue.selesai);
            if (blurEffect != null) blurEffect.SetActive(false);
            
            yield return new WaitForSeconds(0.2f); 
        }

        // 4. SEQUENCE PLAYER 2 (Zoom -> Animasi -> Dialog)
        if (player2 != null && spriteP2 != null)
        {
            // Panning Kamera ke P2
            yield return StartCoroutine(MoveAndZoomCamera(player2.position, cinematicZoomSize, cameraTransitionDuration));
            
            // Trigger Animasi Jatuh
            if (animP2 != null) animP2.SetTrigger(dropAnimTrigger);
            
            // Tunggu animasi selesai
            yield return new WaitForSeconds(animWaitTime);

            // Dialog P2
            if (blurEffect != null) blurEffect.SetActive(true);
            dialogue.ShowDialogue(dialogP2, spriteP2, false);
            yield return new WaitUntil(() => dialogue.selesai);
            if (blurEffect != null) blurEffect.SetActive(false);

            yield return new WaitForSeconds(0.2f);
        }

        // 5. ZOOM OUT KEMBALI KE TAMPILAN PENUH
        yield return StartCoroutine(MoveAndZoomCamera(originalCamPos, originalCamSize, cameraTransitionDuration));

        // 6. MULAI GAMEPLAY
        if (movementP1 != null) movementP1.enabled = true;
        if (movementP2 != null) movementP2.enabled = true;
    }

    // Fungsi Zoom & Pan dengan Map Bounds Collider2D
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