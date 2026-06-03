using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DefenseSystem : MonoBehaviour
{
    public Slider slider;
    public RectTransform perfectZone;
    public BattleController battleController;

    [Header("Enemy & Players Reference")]
    public GameObject enemyObj;
    public GameObject player1Obj;
    public GameObject player2Obj;

    [Header("Animators")]
    public Animator p1Animator;
    public Animator p2Animator;
    public Animator enemyAnimator;

    [Header("Enemy Start Point (WAJIB DIISI)")]
    public Transform enemyStartPoint;

    [Header("Defense Timer Settings")]
    public Slider defenseTimerSlider;
    public float timeLimit = 3f;
    private float currentDefenseTimer;

    [Header("Speed Settings")]
    public float baseSpeed = 40f;
    public float speedIncrement = 15f;

    [Header("Defense Logic")]
    [Tooltip("Ukuran awal kotak hijau saat turn pertama.")]
    public float defenseTolerance = 10f; 
    
    [Header("Shrinking Hitbox Settings")]
    [Tooltip("Berapa banyak area hijau menyusut setiap kali Player sukses bertahan.")]
    public float toleranceShrink = 0.8f; 
    
    [Tooltip("Batas paling kecil area hijau agar game tidak mustahil dimainkan.")]
    public float minTolerance = 5f; 
    
    private float currentTolerance; 

    private int defenseCount = 0;
    private float currentSpeed;
    private bool movingRight = false;
    private bool isActive = false;
    private float targetSliderValue;

    private Coroutine attackRoutine;
    private bool isAttacking = false;

    void Update()
    {
        if (isActive)
        {
            MoveSlider();
            HandleDefenseTimer();

            if (Input.GetKeyDown(KeyCode.Space))
            {
                CheckDefense();
            }
        }
    }

    void LateUpdate()
    {
        if (!isAttacking && enemyObj != null && enemyStartPoint != null)
        {
            enemyObj.transform.position = enemyStartPoint.position;
        }
    }

    void HandleDefenseTimer()
    {
        currentDefenseTimer -= Time.deltaTime;

        if (defenseTimerSlider != null)
            defenseTimerSlider.value = currentDefenseTimer;

        if (currentDefenseTimer <= 0)
            FailDefense();
    }

    void MoveSlider()
    {
        if (movingRight)
            slider.value += Time.deltaTime * currentSpeed;
        else
            slider.value -= Time.deltaTime * currentSpeed;

        if (slider.value >= 100) movingRight = false;
        if (slider.value <= 0) movingRight = true;
    }

    void CheckDefense()
    {
        isActive = false;

        if (defenseTimerSlider != null)
            defenseTimerSlider.gameObject.SetActive(false);

        if (p1Animator != null) p1Animator.SetBool("IsDefending", false);
        if (p2Animator != null) p2Animator.SetBool("IsDefending", false);

        // ===================================================================
        // --- BULLETPROOF VISUAL OVERLAP CHECK (WORLD SPACE) ---
        // Menghitung akurasi berdasarkan posisi fisik piksel yang tampak di monitor.
        // Menjamin jika PIN putih ada di dalam KOTAK HIJAU, 100% terhitung PERFECT.
        // ===================================================================
        bool isPerfect = false;

        if (slider != null && slider.handleRect != null && perfectZone != null)
        {
            // 1. Ambil posisi fisik pin putih di layar
            Vector3[] handleCorners = new Vector3[4];
            slider.handleRect.GetWorldCorners(handleCorners);
            float handleCenterX = (handleCorners[0].x + handleCorners[2].x) * 0.5f;

            // 2. Ambil batas fisik kiri dan kanan kotak hijau di layar
            Vector3[] zoneCorners = new Vector3[4];
            perfectZone.GetWorldCorners(zoneCorners);
            float zoneLeftX = zoneCorners[0].x;
            float zoneRightX = zoneCorners[2].x;

            // 3. Beri toleransi visual bonus 10% dari lebar kotak agar pencetan di ujung garis tetap lolos (anti-frustrasi)
            float visualBuffer = (zoneRightX - zoneLeftX) * 0.1f; 

            // Cek apakah pin putih berada di dalam rentang kotak hijau
            if (handleCenterX >= (zoneLeftX - visualBuffer) && handleCenterX <= (zoneRightX + visualBuffer))
            {
                isPerfect = true;
            }
        }
        else
        {
            // Fallback matematika cadangan jika ada komponen UI yang belum terpasang
            float framePadding = currentSpeed * Time.deltaTime;
            float actualTolerance = currentTolerance + framePadding;
            if (Mathf.Abs(slider.value - targetSliderValue) <= actualTolerance)
            {
                isPerfect = true;
            }
        }
        // ===================================================================

        if (isPerfect)
        {
            if (battleController != null)
            {
                if (battleController.perfectSpawnPoint != null)
                    battleController.SpawnFloatingText("PERFECT!", Color.green, battleController.perfectSpawnPoint.position);
                else
                    battleController.SpawnFloatingText("PERFECT!", Color.green, slider.transform.position);
            }
            battleController.PlayerSuccessDefense();
        }
        else
        {
            FailDefense();
        }
    }

    void FailDefense()
    {
        isActive = false;

        if (defenseTimerSlider != null)
            defenseTimerSlider.gameObject.SetActive(false);

        if (p1Animator != null) p1Animator.SetBool("IsDefending", false);
        if (p2Animator != null) p2Animator.SetBool("IsDefending", false);

        int targetID = battleController.GetRandomTargetID();
        GameObject targetObj = (targetID == 0) ? player1Obj : player2Obj;

        if (targetObj == null || !targetObj.activeInHierarchy)
            targetObj = (targetID == 0) ? player2Obj : player1Obj;

        if (enemyObj != null && targetObj != null)
        {
            if (attackRoutine != null)
            {
                StopCoroutine(attackRoutine);
                attackRoutine = null;
            }

            attackRoutine = StartCoroutine(
                EnemyAttackVisual(enemyObj.transform, targetObj.transform, targetID)
            );
        }
        else
        {
            battleController.PlayerTakeDamage(targetID);
            battleController.EndEnemyAttack();
        }
    }

    IEnumerator EnemyAttackVisual(Transform enemyTr, Transform playerTr, int targetID)
    {
        isAttacking = true;

        Vector3 startPos = enemyStartPoint.position;
        enemyTr.position = startPos;

        SpriteRenderer enemySR = enemyTr.GetComponent<SpriteRenderer>();
        SpriteRenderer playerSR = playerTr.GetComponent<SpriteRenderer>();

        float enemyWidth = enemySR != null ? enemySR.bounds.extents.x : 0.5f;
        float playerWidth = playerSR != null ? playerSR.bounds.extents.x : 0.5f;
        float spacing = 0.5f;

        float targetX = playerTr.position.x + playerWidth + enemyWidth + spacing;
        Vector3 attackPos = new Vector3(targetX, startPos.y, startPos.z);

        Camera mainCam = Camera.main;
        Vector3 camAwal = mainCam.transform.localPosition;
        float camSizeAwal = mainCam.orthographic ? mainCam.orthographicSize : mainCam.fieldOfView;

        if (battleController != null)
        {
            battleController.StartCoroutine(battleController.CinematicActionPan(attackPos, 0.85f, 0.2f));
        }

        // 1. MAJU
        float moveSpeed = 50f;
        while (Vector3.Distance(enemyTr.position, attackPos) > 0.1f)
        {
            enemyTr.position = Vector3.MoveTowards(enemyTr.position, attackPos, Time.deltaTime * moveSpeed);
            yield return null;
        }
        enemyTr.position = attackPos;

        // 2. HIT
        if (enemyAnimator != null) enemyAnimator.SetTrigger("Attack"); 
        if (AudioManager.instance != null) AudioManager.instance.PlaySFX("Enemy_Attack");

        yield return new WaitForSeconds(0.02f); 

        if (battleController != null)
        {
            battleController.TriggerHitStop(0.15f); 
            battleController.TriggerCameraShake(0.2f, 0.4f); 
        }

        // 3. DAMAGE
        battleController.PlayerTakeDamage(targetID);

        yield return new WaitForSeconds(1.28f); 

        // KEMBALIKAN KAMERA
        if (battleController != null)
        {
            battleController.StartCoroutine(battleController.CinematicActionPan(camAwal, 1f / 0.85f, 0.2f));
        }
        mainCam.transform.localPosition = camAwal;
        if (mainCam.orthographic) mainCam.orthographicSize = camSizeAwal; else mainCam.fieldOfView = camSizeAwal;

        // 4. MUNDUR
        float returnSpeed = 40f;
        while (Vector3.Distance(enemyTr.position, startPos) > 0.1f)
        {
            enemyTr.position = Vector3.MoveTowards(enemyTr.position, startPos, Time.deltaTime * returnSpeed);
            yield return null;
        }

        enemyTr.position = startPos;
        isAttacking = false; 
        attackRoutine = null;

        battleController.EndEnemyAttack();
    }

    void RandomizePerfectZone()
    {
        targetSliderValue = Random.Range(15f, 85f);
        float percentage = targetSliderValue / 100f; 

        float tolerancePercentage = currentTolerance / 100f;

        perfectZone.pivot = new Vector2(0.5f, 0.5f);
        perfectZone.anchorMin = new Vector2(percentage - tolerancePercentage, 0f);
        perfectZone.anchorMax = new Vector2(percentage + tolerancePercentage, 1f);
        
        perfectZone.sizeDelta = Vector2.zero; 
        perfectZone.anchoredPosition = Vector2.zero;
    }

    private void OnDisable()
    {
        if (slider != null)
            slider.gameObject.SetActive(false);

        if (defenseTimerSlider != null)
            defenseTimerSlider.gameObject.SetActive(false);

        if (p1Animator != null) p1Animator.SetBool("IsDefending", false);
        if (p2Animator != null) p2Animator.SetBool("IsDefending", false);
    }

    public void ActivateDefense()
    {
        slider.gameObject.SetActive(true);
        slider.minValue = 0;
        slider.maxValue = 100;

        defenseCount++;
        currentSpeed = baseSpeed + (defenseCount * speedIncrement);

        currentTolerance = defenseTolerance - (defenseCount * toleranceShrink);
        currentTolerance = Mathf.Max(currentTolerance, minTolerance);

        currentDefenseTimer = timeLimit;

        if (defenseTimerSlider != null)
        {
            defenseTimerSlider.maxValue = timeLimit;
            defenseTimerSlider.value = timeLimit;
            defenseTimerSlider.gameObject.SetActive(true);
        }

        isActive = true;
        slider.value = 100;
        movingRight = false;

        RandomizePerfectZone();

        if (p1Animator != null && player1Obj != null && player1Obj.activeInHierarchy) 
            p1Animator.SetBool("IsDefending", true);
            
        if (p2Animator != null && player2Obj != null && player2Obj.activeInHierarchy) 
            p2Animator.SetBool("IsDefending", true);
    }
}