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

    private Rigidbody2D rbP1;
    private Rigidbody2D rbP2;

    [Header("Animasi Setup")]
    public Animator animP1;
    public Animator animP2;
    public string dropAnimTrigger = "Land";
    public float animWaitTime = 1.0f; 

    [Header("Camera & Bounds Setup")]
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

        if (player1 != null) rbP1 = player1.GetComponent<Rigidbody2D>();
        if (player2 != null) rbP2 = player2.GetComponent<Rigidbody2D>();

        StartCoroutine(StartCutsceneSequence());
    }

    IEnumerator StartCutsceneSequence()
    {
        if (dialogue != null) dialogue.CloseDialogue(); 

        // 1. KUNCI INPUT PERGERAKAN
        if (movementP1 != null) movementP1.SetCutsceneLock(true);
        if (movementP2 != null) movementP2.SetCutsceneLock(true);

        // 2. TAHAN GRAVITASI 
        if (rbP1 != null) rbP1.simulated = false;
        if (rbP2 != null) rbP2.simulated = false;

        // 3. TUNGGU FADE MANAGER SELESAI
        if (FadeManager.instance != null)
        {
            yield return new WaitUntil(() => FadeManager.instance.isFading == false);
            yield return new WaitForSeconds(0.2f); 
        }

        // 4. SEQUENCE PLAYER 1 
        if (player1 != null)
        {
            // Zoom ke P1
            yield return StartCoroutine(MoveAndZoomCamera(player1.position, cinematicZoomSize, cameraTransitionDuration));
            
            // Aktifkan gravitasi P1
            if (rbP1 != null) rbP1.simulated = true;

            // [DIPERBARUI] Tunggu sampai Player 1 benar-benar menyentuh tanah
            if (movementP1 != null) 
                yield return new WaitUntil(() => movementP1.isGrounded == true);
            else 
                yield return new WaitForSeconds(0.5f); // Fallback jika script tidak ada
            
            // Setelah menyentuh tanah, baru panggil animasi Land
            if (animP1 != null) animP1.SetTrigger(dropAnimTrigger);
            yield return new WaitForSeconds(animWaitTime);

            if (blurEffect != null) blurEffect.SetActive(true);
            dialogue.ShowDialogue(dialogP1, spriteP1, true);
            yield return new WaitUntil(() => dialogue.selesai);
            if (blurEffect != null) blurEffect.SetActive(false);
            
            yield return new WaitForSeconds(0.2f); 
        }

        // 5. SEQUENCE PLAYER 2
        if (player2 != null && spriteP2 != null)
        {
            // Pan & Zoom Kamera ke P2 
            yield return StartCoroutine(MoveAndZoomCamera(player2.position, cinematicZoomSize, cameraTransitionDuration));
            
            // Aktifkan gravitasi P2
            if (rbP2 != null) rbP2.simulated = true;

            // [DIPERBARUI] Tunggu sampai Player 2 menyentuh tanah (tanah P2 ada di atas)
            if (movementP2 != null) 
                yield return new WaitUntil(() => movementP2.isGrounded == true);
            else 
                yield return new WaitForSeconds(0.5f); // Fallback jika script tidak ada
            
            // Setelah menyentuh tanah, baru panggil animasi Land
            if (animP2 != null) animP2.SetTrigger(dropAnimTrigger);
            yield return new WaitForSeconds(animWaitTime);

            if (blurEffect != null) blurEffect.SetActive(true);
            dialogue.ShowDialogue(dialogP2, spriteP2, false);
            yield return new WaitUntil(() => dialogue.selesai);
            if (blurEffect != null) blurEffect.SetActive(false);

            yield return new WaitForSeconds(0.2f);
        }

        // 6. ZOOM OUT KEMBALI & MULAI GAMEPLAY
        yield return StartCoroutine(MoveAndZoomCamera(originalCamPos, originalCamSize, cameraTransitionDuration));

        if (movementP1 != null) movementP1.SetCutsceneLock(false);
        if (movementP2 != null) movementP2.SetCutsceneLock(false);
        
        if (rbP1 != null) rbP1.simulated = true;
        if (rbP2 != null) rbP2.simulated = true;
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