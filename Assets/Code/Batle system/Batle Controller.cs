using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;

public class BattleController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject mainUIContainer; 
    public ComboUI comboUI;
    public DefenseSystem defenseSystem;
    public Slider playerTimerSlider;
    public TextMeshProUGUI phaseText;
    public TextMeshProUGUI comboNotificationText; 
    public Image flashScreenImage;
    public GameObject floatingTextPrefab; 
    public Transform canvasTransform;

    [Header("Enemy Phase UI Portraits")]
    public Image enemyUIPortrait; 
    public Sprite stage1Portrait;
    public Sprite stage2Portrait;
    public Sprite stage3Portrait;

    [Header("Win/Lose UI Panels")]
    public GameObject winPanel;
    public GameObject losePanel;
    public GameObject gameOverPanel;

    [Header("Health & Status UI")]
    public Slider enemyHealthSlider;
    public Image[] p1Hearts;
    public Image[] p2Hearts;
    public GameObject player1Obj;
    public GameObject player2Obj;

    [Header("Player Portraits")]
    public Image p1Portrait;
    public Image p2Portrait;
    public Sprite p1AliveSprite;
    public Sprite p1DeadSprite;
    public Sprite p2AliveSprite;
    public Sprite p2DeadSprite;

    [Header("Ready Go Cutscene")]
    public CanvasGroup cutscenePanelGroup;
    public GameObject readyImage;
    public GameObject goImage;

    [Header("Camera Constraints")]
    public Collider2D mapBounds;

    [Header("Battle Logic")]
    public EnemyData currentEnemy;
    private int currentPhase = 12; 
    private int p1Health = 3;
    private int p2Health = 3;
    private float enemyHealth = 80f;
    private float maxEnemyHealth = 80f; 
    private bool isGameOver = false;

    [Header("New Mechanics Logic")]
    private int successfulAttackCount = 0; 
    private bool isComboTurn = false; 
    private int enemyCurrentStage = 1; 
    private int enemyAttacksLeft = 0; 

    [Header("Timer Settings")]
    public float timePerTurn = 60f;
    private float currentTimer;
    private bool isPlayerTurn = false;
    private bool isStartingCutscene = true;

    private int playerYangLagiNgetik = 1;
    private List<KeyCode> currentRandomCombo = new List<KeyCode>();
    private int currentInputIndex = 0;
    private KeyCode[] possibleKeys = { KeyCode.W, KeyCode.A, KeyCode.D, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow };

    [Header("Animators")]
    public Animator p1Animator; 
    public Animator p2Animator;
    public Animator enemyAnimator;

    [Header("Enemy Stage Animators (Overrides)")]
    public AnimatorOverrideController stage2AnimatorOverride;
    public AnimatorOverrideController stage3AnimatorOverride;

    [Header("Audio Settings")]
    public string phaseTransitionSFX = "Transisi";
    public string pumpSFX = "Transisi"; 

    [Header("Spawn Points")]
    public Transform missSpawnPoint;
    public Transform perfectSpawnPoint;

    void Start()
    {
        isGameOver = false;
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (comboNotificationText != null) comboNotificationText.gameObject.SetActive(false);

        enemyHealth = 80f;
        maxEnemyHealth = 80f;
        enemyHealthSlider.maxValue = maxEnemyHealth; 
        enemyHealthSlider.value = enemyHealth;
        playerTimerSlider.maxValue = timePerTurn;
        playerTimerSlider.value = timePerTurn;

        currentPhase = 12; 

        UpdateHeartUI();
        UpdatePlayerPortraits();

        if (enemyUIPortrait != null && stage1Portrait != null) 
            enemyUIPortrait.sprite = stage1Portrait;

        if (currentEnemy != null) StartCoroutine(PlayReadyGo());
    }

    bool CheckEnemyStage()
    {
        int previousStage = enemyCurrentStage;

        if (enemyHealth <= 20) enemyCurrentStage = 3;
        else if (enemyHealth <= 50) enemyCurrentStage = 2;
        else enemyCurrentStage = 1;

        return previousStage != enemyCurrentStage;
    }

    void EnemyHeal(float amount)
    {
        if (enemyCurrentStage == 2 && enemyHealth > 0 && enemyHealth < maxEnemyHealth)
        {
            enemyHealth = Mathf.Min(enemyHealth + amount, maxEnemyHealth);
            enemyHealthSlider.value = enemyHealth;
        }
    }

    public void StartPlayerTurn()
    {
        if (isStartingCutscene || isGameOver) return;

        if (currentPhase <= 0)
        {
            DetermineLoser();
            return;
        }

        isPlayerTurn = true;
        currentInputIndex = 0;
        currentTimer = timePerTurn;
        playerTimerSlider.gameObject.SetActive(true);
        if (defenseSystem != null) defenseSystem.gameObject.SetActive(false);

        if (enemyAnimator != null) enemyAnimator.SetBool("IsDefending", true);

        int comboLength = 3;
        if (currentEnemy.keysPerPhase != null && currentEnemy.keysPerPhase.Count > 0)
        {
            int phaseIndex = Mathf.Clamp(12 - currentPhase, 0, currentEnemy.keysPerPhase.Count - 1);
            comboLength = currentEnemy.keysPerPhase[phaseIndex];
        }

        if (successfulAttackCount >= 2)
        {
            isComboTurn = true; 
            comboLength += 2; 
            successfulAttackCount = 0; 

            if (comboNotificationText != null) 
            {
                comboNotificationText.text = "COMBO ATTACK!";
                comboNotificationText.gameObject.SetActive(true);
                StartCoroutine(HideComboNotification());
            }
        }
        else
        {
            isComboTurn = false; 
            if (comboNotificationText != null) comboNotificationText.gameObject.SetActive(false);
        }

        GenerateRandomCombo(comboLength);
        UpdateUI();
        comboUI.gameObject.SetActive(true);
        comboUI.SetupComboUI(currentRandomCombo);
    }

    IEnumerator HideComboNotification()
    {
        yield return new WaitForSeconds(1.55f);
        if (comboNotificationText != null) comboNotificationText.gameObject.SetActive(false);
    }

    void HandleAttackInput()
    {
        if (!isPlayerTurn || isGameOver || currentRandomCombo == null || currentRandomCombo.Count == 0) return;
        if (!Input.anyKeyDown || Input.GetMouseButtonDown(0)) return;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D))
            playerYangLagiNgetik = 1;
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
            playerYangLagiNgetik = 2;

        KeyCode expectedKey = currentRandomCombo[currentInputIndex];

        if (Input.GetKeyDown(expectedKey))
        {
            if (AudioManager.instance != null) AudioManager.instance.PlaySFX("SFC Clik");
            comboUI.UpdateKeyColor(currentInputIndex);
            currentInputIndex++;

            if (currentInputIndex >= currentRandomCombo.Count)
            {
                isPlayerTurn = false; 

                if (!isComboTurn) successfulAttackCount++; 

                if (playerYangLagiNgetik == 1 && p1Health <= 0) playerYangLagiNgetik = 2; 
                else if (playerYangLagiNgetik == 2 && p2Health <= 0) playerYangLagiNgetik = 1; 

                // --- FIX: Cek apakah ini pukulan terakhir (Sisa darah 10 atau kurang) ---
                bool isFinisher = (enemyHealth <= 10);
                EksekusiSeranganVisual(playerYangLagiNgetik, isFinisher); // Teruskan info Finisher

                enemyHealth -= 10;

                if (enemyAnimator != null) 
                {
                    enemyAnimator.SetBool("IsDefending", false);
                    enemyAnimator.SetTrigger("Hurt");
                }

                if (AudioManager.instance != null) AudioManager.instance.PlaySFX("Enemy Terkena Hit");

                bool isStageChanged = CheckEnemyStage(); 
                enemyHealthSlider.value = enemyHealth;

                if (defenseSystem.enemyObj != null) StartCoroutine(BlinkEffect(defenseSystem.enemyObj));

                if (enemyHealth <= 0) 
                {
                    // (Flash dipanggil di dalam Coroutine EfekMaju saat tinju mengenai sasaran)
                    DetermineWinner();
                    return;
                }

                currentInputIndex = 0;

                if (isStageChanged) StartCoroutine(EnemyStageTransition());
                else Invoke(nameof(StartEnemyTurn), 0.8f); 
            }
        }
        else
        {
            if (missSpawnPoint != null)
                SpawnFloatingText("MISS!", Color.red, missSpawnPoint.position);
            else
                SpawnFloatingText("MISS!", Color.red, comboUI.transform.position);
            if (AudioManager.instance != null) AudioManager.instance.PlaySFX("Miss");
            currentInputIndex = 0;
            comboUI.SetupComboUI(currentRandomCombo);
        }
    }

    IEnumerator EnemyStageTransition()
    {
        if (mainUIContainer != null) mainUIContainer.SetActive(false);
        comboUI.gameObject.SetActive(false);
        playerTimerSlider.gameObject.SetActive(false);

        if (AudioManager.instance != null && !string.IsNullOrEmpty(phaseTransitionSFX))
        {
            AudioManager.instance.PlaySFXPitched(phaseTransitionSFX, 0.5f); 
        }

        yield return new WaitForSeconds(0.8f);

        Camera mainCam = Camera.main;
        Transform enemyTr = defenseSystem.enemyObj.transform; 

        if (mainCam != null && enemyTr != null)
        {
            // --- ZOOM IN KAMERA ---
            Vector3 originalPos = mainCam.transform.position;
            float originalSize = mainCam.orthographic ? mainCam.orthographicSize : mainCam.fieldOfView;
            float targetSize = originalSize * 0.6f; 
            float targetX = enemyTr.position.x;
            float targetY = enemyTr.position.y;

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

            Vector3 targetPos = new Vector3(targetX, targetY, originalPos.z);
            float elapsed = 0f;
            float duration = 0.5f; 

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                mainCam.transform.position = Vector3.Lerp(originalPos, targetPos, t);
                if (mainCam.orthographic) mainCam.orthographicSize = Mathf.Lerp(originalSize, targetSize, t);
                else mainCam.fieldOfView = Mathf.Lerp(originalSize, targetSize, t);
                yield return null;
            }

            // ==========================================
            // KOREOGRAFI BARU: MENYUSUT -> GANTI -> MUNCUL -> PUMP
            // ==========================================

            Vector3 originalScale = enemyTr.localScale;
            
            // 1. Musuh Phase Lama Menyusut dan Hilang
            float shrinkDuration = 0.4f;
            float elapsedShrink = 0f;
            while (elapsedShrink < shrinkDuration)
            {
                elapsedShrink += Time.deltaTime;
                enemyTr.localScale = Vector3.Lerp(originalScale, Vector3.zero, elapsedShrink / shrinkDuration);
                yield return null;
            }
            enemyTr.localScale = Vector3.zero; // Pastikan ukurannya 0

            // Jeda dramatis saat arena kosong
            yield return new WaitForSeconds(0.2f);

            // 2. Ganti Wujud (Animator & UI Portrait) saat musuh sedang ga kelihatan
            if (enemyAnimator != null)
            {
                if (enemyCurrentStage == 2) 
                {
                    if (stage2AnimatorOverride != null) enemyAnimator.runtimeAnimatorController = stage2AnimatorOverride;
                    if (enemyUIPortrait != null && stage2Portrait != null) enemyUIPortrait.sprite = stage2Portrait;
                }
                else if (enemyCurrentStage == 3) 
                {
                    if (stage3AnimatorOverride != null) enemyAnimator.runtimeAnimatorController = stage3AnimatorOverride;
                    if (enemyUIPortrait != null && stage3Portrait != null) enemyUIPortrait.sprite = stage3Portrait;
                }
            }

            // 3. Musuh Phase Baru Muncul dan Membesar perlahan
            float growDuration = 0.4f;
            float elapsedGrow = 0f;
            while (elapsedGrow < growDuration)
            {
                elapsedGrow += Time.deltaTime;
                enemyTr.localScale = Vector3.Lerp(Vector3.zero, originalScale, elapsedGrow / growDuration);
                yield return null;
            }
            enemyTr.localScale = originalScale;

            // 4. Efek Detak Jantung / Pump (Sesuai aslinya)
            Vector3 enlargedScale = originalScale * 1.4f; 
            int pumpCount = 3; 
            float pumpSpeed = 0.15f; 

            for (int i = 0; i < pumpCount; i++)
            {
                // SFX Pump keluar tiap kali dia memompa
                if (AudioManager.instance != null && !string.IsNullOrEmpty(pumpSFX))
                {
                    AudioManager.instance.PlaySFX(pumpSFX); 
                }

                float pumpTime = 0f;
                while (pumpTime < pumpSpeed)
                {
                    pumpTime += Time.deltaTime;
                    enemyTr.localScale = Vector3.Lerp(originalScale, enlargedScale, pumpTime / pumpSpeed);
                    yield return null;
                }

                pumpTime = 0f;
                while (pumpTime < pumpSpeed)
                {
                    pumpTime += Time.deltaTime;
                    enemyTr.localScale = Vector3.Lerp(enlargedScale, originalScale, pumpTime / pumpSpeed);
                    yield return null;
                }
            }
            enemyTr.localScale = originalScale; 

            yield return new WaitForSeconds(0.5f); 

            // --- ZOOM OUT KAMERA KEMBALI NORMAL ---
            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                mainCam.transform.position = Vector3.Lerp(targetPos, originalPos, t);
                if (mainCam.orthographic) mainCam.orthographicSize = Mathf.Lerp(targetSize, originalSize, t);
                else mainCam.fieldOfView = Mathf.Lerp(targetSize, originalSize, t);
                yield return null;
            }

            mainCam.transform.position = originalPos;
            if (mainCam.orthographic) mainCam.orthographicSize = originalSize;
            else mainCam.fieldOfView = originalSize;
        }
        else
        {
            yield return new WaitForSeconds(1.5f);
        }

        if (mainUIContainer != null) mainUIContainer.SetActive(true);

        StartEnemyTurn();
    }

    public void StartEnemyTurn()
    {
        if (isGameOver) return;

        if (!isComboTurn) currentPhase--;

        UpdateUI();

        isPlayerTurn = false;
        playerTimerSlider.gameObject.SetActive(false);
        comboUI.gameObject.SetActive(false);

        if (currentPhase <= 0 && enemyHealth > 0)
        {
            DetermineLoser();
            return;
        }

        if (defenseSystem != null)
        {
            defenseSystem.gameObject.SetActive(true);

            enemyAttacksLeft = (enemyCurrentStage == 3) ? 2 : 1;
            defenseSystem.ActivateDefense();
        }
    }

    void TriggerNextDefense()
    {
        if (!isGameOver && defenseSystem != null) defenseSystem.ActivateDefense();
    }

    public void PlayerTakeDamage(int forcedTarget)
    {
        if (isGameOver) return;

        EnemyHeal(5f); 

        GameObject targetObj = null;
        Animator targetAnim = null; 

        if (forcedTarget == 0)
        {
            if (p1Health > 0) { p1Health--; targetObj = player1Obj; targetAnim = p1Animator; }
            else if (p2Health > 0) { p2Health--; targetObj = player2Obj; targetAnim = p2Animator; }
        }
        else
        {
            if (p2Health > 0) { p2Health--; targetObj = player2Obj; targetAnim = p2Animator; }
            else if (p1Health > 0) { p1Health--; targetObj = player1Obj; targetAnim = p1Animator; }
        }

        if (targetAnim != null) targetAnim.SetTrigger("Hurt"); 
        if (targetObj != null) StartCoroutine(BlinkEffect(targetObj)); 

        if (AudioManager.instance != null) AudioManager.instance.PlaySFX("Player terkena hit");

        UpdateHeartUI();
        UpdatePlayerPortraits();
        CheckPlayerLives();
    }

    public void EndEnemyAttack()
    {
        if (isGameOver) return;

        if (p1Health <= 0 && p2Health <= 0)
        {
            DetermineLoser();
            return;
        }

        enemyAttacksLeft--;
        if (enemyAttacksLeft > 0) Invoke(nameof(TriggerNextDefense), 1.0f); 
        else StartPlayerTurn(); 
    }

    public void PlayerSuccessDefense() 
    { 
        if (isGameOver) return;

        if (AudioManager.instance != null) 
        {
            AudioManager.instance.PlaySFX("Player Defense"); 
        }

        enemyAttacksLeft--;
        if (enemyAttacksLeft > 0) Invoke(nameof(TriggerNextDefense), 1.0f); 
        else StartPlayerTurn(); 
    }

    IEnumerator PlayReadyGo()
    {
        isStartingCutscene = true;
        cutscenePanelGroup.gameObject.SetActive(true);
        cutscenePanelGroup.alpha = 0;
        readyImage.SetActive(false);
        goImage.SetActive(false);

        float duration = 0.5f;
        float currentTime = 0;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            cutscenePanelGroup.alpha = Mathf.Lerp(0, 1, currentTime / duration);
            yield return null;
        }

        readyImage.SetActive(true);
        if (AudioManager.instance != null) AudioManager.instance.PlaySFXDirect("Ready");
        yield return new WaitForSeconds(1.2f);
        readyImage.SetActive(false);

        goImage.SetActive(true);
        if (AudioManager.instance != null) AudioManager.instance.PlaySFXDirect("GO");
        yield return new WaitForSeconds(2f);
        goImage.SetActive(false);

        currentTime = 0;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            cutscenePanelGroup.alpha = Mathf.Lerp(1, 0, currentTime / duration);
            yield return null;
        }

        cutscenePanelGroup.gameObject.SetActive(false);
        isStartingCutscene = false;
        StartPlayerTurn();
    }

    IEnumerator BlinkEffect(GameObject target)
    {
        SpriteRenderer sr = target.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;
        Color originalColor = sr.color;
        Color blinkColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0.2f); 
        for (int i = 0; i < 3; i++)
        {
            sr.color = blinkColor;
            yield return new WaitForSeconds(0.1f);
            sr.color = originalColor;
            yield return new WaitForSeconds(0.1f);
        }
        sr.color = originalColor;
    }

    // --- FIX: Menerima info isFinisher ---
    void EksekusiSeranganVisual(int noPlayer, bool isFinisher)
    {
       Transform targetTr = (noPlayer == 1) ? player1Obj.transform : player2Obj.transform;
       Animator targetAnim = (noPlayer == 1) ? p1Animator : p2Animator;

       StartCoroutine(EfekMaju(targetTr, targetAnim, noPlayer, isFinisher));
    }

    // --- FIX: Parameter isFinisher ditambahkan ---
    IEnumerator EfekMaju(Transform playerTr, Animator anim, int noPlayer, bool isFinisher)
    {
        Vector3 posAwal = playerTr.position;
        Transform enemyTr = defenseSystem.enemyObj.transform;
        SpriteRenderer pSR = playerTr.GetComponent<SpriteRenderer>();
        SpriteRenderer eSR = enemyTr.GetComponent<SpriteRenderer>();
        float pWidth = pSR != null ? pSR.bounds.extents.x : 0.5f;
        float eWidth = eSR != null ? eSR.bounds.extents.x : 0.5f;
        float spacing = 0.5f;
        Vector3 posSerang = new Vector3(enemyTr.position.x - (eWidth + pWidth + spacing), posAwal.y, posAwal.z);

        // --- DINAMIKA KAMERA: Simpan posisi & ukuran awal kamera ---
        Camera mainCam = Camera.main;
        Vector3 camAwal = mainCam.transform.localPosition;
        float camSizeAwal = mainCam.orthographic ? mainCam.orthographicSize : mainCam.fieldOfView;

        // Panggil Zoom In ke arah musuh bersamaan dengan player maju (Kecepatan 0.2 detik)
        StartCoroutine(CinematicActionPan(posSerang, 0.85f, 0.2f)); 
        // -----------------------------------------------------------

        float moveSpeed = 50f;
        // 1. Player maju ke depan musuh
        while (Vector3.Distance(playerTr.position, posSerang) > 0.1f)
        {
            playerTr.position = Vector3.MoveTowards(playerTr.position, posSerang, Time.deltaTime * moveSpeed);
            yield return null;
        }
        playerTr.position = posSerang;

        // 2. Mulai putar animasi Attack
        if (anim != null) anim.SetTrigger("Attack"); 
        
        yield return new WaitForSeconds(0.15f); 

        // 3. SUNTIKAN JUICE & SUARA
        if (AudioManager.instance != null) 
            AudioManager.instance.PlaySFX(noPlayer == 1 ? "P1 Attack" : "Player 2_Attack");

        TriggerHitStop(0.15f); 
        TriggerCameraShake(0.15f, 0.2f); 

        if (isFinisher) TriggerScreenFlash();

        yield return new WaitForSeconds(0.25f);

        // --- DINAMIKA KAMERA: Kembalikan kamera ke semula saat player mundur ---
        StartCoroutine(CinematicActionPan(camAwal, 1f / 0.85f, 0.2f));
        // Kita juga pastikan posisi akhirnya terkunci rapi agar tidak meleset
        mainCam.transform.localPosition = camAwal;
        if (mainCam.orthographic) mainCam.orthographicSize = camSizeAwal; else mainCam.fieldOfView = camSizeAwal;
        // -----------------------------------------------------------------------

        // 5. Mundur ke tempat semula
        while (Vector3.Distance(playerTr.position, posAwal) > 0.1f)
        {
            playerTr.position = Vector3.MoveTowards(playerTr.position, posAwal, Time.deltaTime * 40f);
            yield return null;
        }
        playerTr.position = posAwal;
    }

    void DetermineWinner()
    {
        if (isGameOver) return;
        isGameOver = true;
        isPlayerTurn = false;
        StartCoroutine(WinSequence());
    }

    IEnumerator WinSequence()
    {
        // 1. Tunggu sebentar sampai tinju Player benar-benar mendarat
        // Waktu 0.2f ini sengaja disinkronkan agar pas dengan layar Flash & Hit Stop
        yield return new WaitForSeconds(0.2f);

        // 2. Efek Musuh Hancur / Mati (Menyusut dengan cepat)
        if (defenseSystem != null && defenseSystem.enemyObj != null)
        {
            Transform enemyTr = defenseSystem.enemyObj.transform;
            
            float duration = 0.4f; // Kecepatan musuh lenyap (0.4 detik)
            float elapsed = 0f;
            Vector3 startScale = enemyTr.localScale;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                // Animasi mengecil menjadi hilang (Vector3.zero)
                enemyTr.localScale = Vector3.Lerp(startScale, Vector3.zero, elapsed / duration);
                yield return null;
            }
            
            // Matikan objek musuh dari arena
            defenseSystem.enemyObj.SetActive(false);
        }

        // 3. Jeda dramatis (layar sepi sesaat setelah musuh hancur)
        yield return new WaitForSeconds(0.5f);

        // 4. Baru Tampilkan UI "You Win"
        if (winPanel != null) winPanel.SetActive(true);
        if (AudioManager.instance != null) AudioManager.instance.PlaySFX("WinSound");
        
        yield return new WaitForSeconds(2f);
        
        // 5. Pindah Scene
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings) SceneManager.LoadScene(nextSceneIndex);
    }

    void DetermineLoser()
    {
        if (isGameOver) return;
        isGameOver = true;
        isPlayerTurn = false;
        StartCoroutine(LoseSequence());
    }

    IEnumerator LoseSequence()
    {
        if (losePanel != null) losePanel.SetActive(true);
        yield return new WaitForSeconds(2f);
        if (losePanel != null) losePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    void CheckPlayerLives()
    {
        // Jika darah habis dan objeknya masih aktif, jalankan animasi menyusut
        if (p1Health <= 0 && player1Obj != null && player1Obj.activeInHierarchy) 
            StartCoroutine(PlayerDeathRoutine(player1Obj));
            
        if (p2Health <= 0 && player2Obj != null && player2Obj.activeInHierarchy) 
            StartCoroutine(PlayerDeathRoutine(player2Obj));
    }

    // --- COROUTINE BARU UNTUK KEMATIAN PLAYER ---
    IEnumerator PlayerDeathRoutine(GameObject playerObj)
    {
        float duration = 0.4f;
        float elapsed = 0f;
        Vector3 startScale = playerObj.transform.localScale;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            playerObj.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, elapsed / duration);
            yield return null;
        }
        
        playerObj.SetActive(false);
        // Kembalikan skala ke normal (untuk persiapan jika player tekan Retry / main lagi)
        playerObj.transform.localScale = startScale; 
    }

    void UpdatePlayerPortraits()
    {
        if (p1Portrait != null) p1Portrait.sprite = (p1Health > 0) ? p1AliveSprite : p1DeadSprite;
        if (p2Portrait != null) p2Portrait.sprite = (p2Health > 0) ? p2AliveSprite : p2DeadSprite;
    }

    void UpdateHeartUI()
    {
        for (int i = 0; i < p1Hearts.Length; i++) p1Hearts[i].enabled = (i < p1Health);
        for (int i = 0; i < p2Hearts.Length; i++) p2Hearts[i].enabled = (i < p2Health);
    }

    void UpdateUI()
    {
        phaseText.text = "Phase: " + currentPhase;
    }

    void Update()
    {
        if (isGameOver) return;
        if (isPlayerTurn && !isStartingCutscene)
        {
            HandleTimer();
            HandleAttackInput();
        }
    }

    void HandleTimer()
    {
        currentTimer -= Time.deltaTime;
        playerTimerSlider.value = currentTimer;
        if (currentTimer <= 0 && isPlayerTurn) 
        {
            if (enemyAnimator != null) enemyAnimator.SetBool("IsDefending", false);

            EnemyHeal(5f); 
            StartEnemyTurn();
        }
    }

    void GenerateRandomCombo(int length)
    {
        currentRandomCombo.Clear();
        for (int i = 0; i < length; i++)
            currentRandomCombo.Add(possibleKeys[Random.Range(0, possibleKeys.Length)]);
    }

    public int GetRandomTargetID() { return Random.Range(0, 2); }

    public void RetryGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


    public void TriggerHitStop(float duration)
    {
        StartCoroutine(HitStopRoutine(duration));
    }

    private IEnumerator HitStopRoutine(float duration)
    {
        Time.timeScale = 0f; 
        yield return new WaitForSecondsRealtime(duration); 
        Time.timeScale = 1f; 
    }

    public void TriggerCameraShake(float duration, float magnitude)
    {
        StartCoroutine(CameraShakeRoutine(duration, magnitude));
    }

    private IEnumerator CameraShakeRoutine(float duration, float magnitude)
    {
        Camera mainCam = Camera.main;
        if (mainCam == null) yield break;

        Vector3 originalPos = mainCam.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = originalPos.x + Random.Range(-1f, 1f) * magnitude;
            float y = originalPos.y + Random.Range(-1f, 1f) * magnitude;

            mainCam.transform.localPosition = new Vector3(x, y, originalPos.z);
            
            // UnscaledDeltaTime agar tetap bergetar saat Time.timeScale = 0
            elapsed += Time.unscaledDeltaTime; 
            yield return null;
        }

        mainCam.transform.localPosition = originalPos;
    }
    public void TriggerScreenFlash()
    {
        if (flashScreenImage != null)
            StartCoroutine(ScreenFlashRoutine());
    }

    private IEnumerator ScreenFlashRoutine()
    {
        // Set warna putih dengan Alpha 80% secara instan
        flashScreenImage.color = new Color(1f, 1f, 1f, 0.8f);

        float duration = 0.15f; // Sangat cepat
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Gunakan unscaledDeltaTime agar efek flash tetap jalan meskipun game sedang beku (Hit Stop)
            elapsed += Time.unscaledDeltaTime;
            
            // Pudarkan Alpha dari 0.8 kembali ke 0
            float alpha = Mathf.Lerp(0.8f, 0f, elapsed / duration);
            flashScreenImage.color = new Color(1f, 1f, 1f, alpha);
            
            yield return null;
        }

        // Pastikan benar-benar transparan di akhir
        flashScreenImage.color = new Color(1f, 1f, 1f, 0f);
    }

    public void SpawnFloatingText(string textMsg, Color textColor, Vector3 spawnPosition)
    {
        if (floatingTextPrefab != null && canvasTransform != null)
        {
            // Munculkan prefab di dalam Canvas
            GameObject popText = Instantiate(floatingTextPrefab, canvasTransform);
            
            // Atur posisinya (misal: di atas tombol QTE atau di atas Slider)
            popText.transform.position = spawnPosition; 

            // Panggil fungsi setup untuk mengatur tulisan dan animasinya
            FloatingText ft = popText.GetComponent<FloatingText>();
            if (ft != null)
            {
                ft.Setup(textMsg, textColor);
            }
        }
    }

    public IEnumerator CinematicActionPan(Vector3 targetPos, float zoomMultiplier, float duration)
    {
        Camera mainCam = Camera.main;
        if (mainCam == null) yield break;

        Vector3 startPos = mainCam.transform.localPosition;
        float startSize = mainCam.orthographic ? mainCam.orthographicSize : mainCam.fieldOfView;
        
        float targetSize = startSize * zoomMultiplier;

        // --- FIX: LOGIKA MAP BOUNDS UNTUK CINEMATIC PAN ---
        float targetX = targetPos.x;
        float targetY = targetPos.y;

        // Jika mapBounds dipasang dan kamera bertipe 2D (Orthographic)
        if (mapBounds != null && mainCam.orthographic)
        {
            // Hitung lebar dan tinggi jangkauan kamera berdasarkan target size zoom yang baru
            float camHeight = targetSize;
            float camWidth = targetSize * mainCam.aspect;
            
            // Batas minimal dan maksimal posisi kamera agar tidak keluar batas Collider2D mapBounds
            float minX = mapBounds.bounds.min.x + camWidth;
            float maxX = mapBounds.bounds.max.x - camWidth;
            float minY = mapBounds.bounds.min.y + camHeight;
            float maxY = mapBounds.bounds.max.y - camHeight;

            // Kunci posisi X agar tetap di dalam batas map
            if (minX < maxX) targetX = Mathf.Clamp(targetX, minX, maxX);
            else targetX = (minX + maxX) / 2f;

            // Kunci posisi Y agar tetap di dalam batas map
            if (minY < maxY) targetY = Mathf.Clamp(targetY, minY, maxY);
            else targetY = (minY + maxY) / 2f;
        }

        // Terapkan koordinat akhir yang sudah aman dimasukkan ke batas map
        Vector3 finalTargetPos = new Vector3(targetX, targetY, startPos.z);
        // --------------------------------------------------

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; 
            float t = elapsed / duration;

            // Efek Ease-Out agar gerakannya mulus
            t = 1f - Mathf.Pow(1f - t, 3f); 

            mainCam.transform.localPosition = Vector3.Lerp(startPos, finalTargetPos, t);
            
            if (mainCam.orthographic) 
                mainCam.orthographicSize = Mathf.Lerp(startSize, targetSize, t);
            else 
                mainCam.fieldOfView = Mathf.Lerp(startSize, targetSize, t);

            yield return null;
        }
    }
}