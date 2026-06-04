using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DefenseSystem : MonoBehaviour
{
    [Header("Input Settings")]
    public string defenseInputP1 = "Defense_P1"; // Tombol Space / X Stick 1
    public string defenseInputP2 = "Defense_P2"; // Tombol Enter / X Stick 2

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
    public float defenseTolerance = 10f; 
    public float toleranceShrink = 0.8f; 
    public float minTolerance = 5f; 
    private float currentTolerance; 

    private int defenseCount = 0;
    private float currentSpeed;
    private bool movingRight = false;
    private bool isActive = false;
    private float targetSliderValue;

    private Coroutine attackRoutine;
    private bool isAttacking = false;

    // --- BARU: Menyimpan ID Player yang sedang ditargetkan musuh ---
    private int targetedPlayerID = 0; 

    void Update()
    {
        if (isActive)
        {
            MoveSlider();
            HandleDefenseTimer();

            // --- DIPERBARUI: Hanya dengarkan input dari Player yang ditargetkan ---
            string activeDefenseInput = (targetedPlayerID == 0) ? defenseInputP1 : defenseInputP2;

            if (Input.GetButtonDown(activeDefenseInput))
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

        bool isPerfect = false;

        if (slider != null && slider.handleRect != null && perfectZone != null)
        {
            Vector3[] handleCorners = new Vector3[4];
            slider.handleRect.GetWorldCorners(handleCorners);
            float handleCenterX = (handleCorners[0].x + handleCorners[2].x) * 0.5f;

            Vector3[] zoneCorners = new Vector3[4];
            perfectZone.GetWorldCorners(zoneCorners);
            float zoneLeftX = zoneCorners[0].x;
            float zoneRightX = zoneCorners[2].x;

            float visualBuffer = (zoneRightX - zoneLeftX) * 0.1f; 

            if (handleCenterX >= (zoneLeftX - visualBuffer) && handleCenterX <= (zoneRightX + visualBuffer))
            {
                isPerfect = true;
            }
        }
        else
        {
            float framePadding = currentSpeed * Time.deltaTime;
            float actualTolerance = currentTolerance + framePadding;
            if (Mathf.Abs(slider.value - targetSliderValue) <= actualTolerance)
            {
                isPerfect = true;
            }
        }

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

    // --- DIPERBARUI: Tidak mengacak target lagi di sini karena sudah ditentukan di awal ---
    void FailDefense()
    {
        isActive = false;

        if (defenseTimerSlider != null)
            defenseTimerSlider.gameObject.SetActive(false);

        if (p1Animator != null) p1Animator.SetBool("IsDefending", false);
        if (p2Animator != null) p2Animator.SetBool("IsDefending", false);

        GameObject targetObj = (targetedPlayerID == 0) ? player1Obj : player2Obj;

        if (enemyObj != null && targetObj != null)
        {
            if (attackRoutine != null)
            {
                StopCoroutine(attackRoutine);
                attackRoutine = null;
            }

            attackRoutine = StartCoroutine(
                EnemyAttackVisual(enemyObj.transform, targetObj.transform, targetedPlayerID)
            );
        }
        else
        {
            battleController.PlayerTakeDamage(targetedPlayerID);
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

        float moveSpeed = 50f;
        while (Vector3.Distance(enemyTr.position, attackPos) > 0.1f)
        {
            enemyTr.position = Vector3.MoveTowards(enemyTr.position, attackPos, Time.deltaTime * moveSpeed);
            yield return null;
        }
        enemyTr.position = attackPos;

        if (enemyAnimator != null) enemyAnimator.SetTrigger("Attack"); 
        if (AudioManager.instance != null) AudioManager.instance.PlaySFX("Enemy_Attack");

        yield return new WaitForSeconds(0.02f); 

        if (battleController != null)
        {
            battleController.TriggerHitStop(0.15f); 
            battleController.TriggerCameraShake(0.2f, 0.4f); 
        }

        battleController.PlayerTakeDamage(targetID);

        yield return new WaitForSeconds(1.28f); 

        if (battleController != null)
        {
            battleController.StartCoroutine(battleController.CinematicActionPan(camAwal, 1f / 0.85f, 0.2f));
        }
        mainCam.transform.localPosition = camAwal;
        if (mainCam.orthographic) mainCam.orthographicSize = camSizeAwal; else mainCam.fieldOfView = camSizeAwal;

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
        if (slider != null) slider.gameObject.SetActive(false);
        if (defenseTimerSlider != null) defenseTimerSlider.gameObject.SetActive(false);
        if (p1Animator != null) p1Animator.SetBool("IsDefending", false);
        if (p2Animator != null) p2Animator.SetBool("IsDefending", false);
    }

    // --- DIPERBARUI: Penentuan target dipindah ke sini (awal mula turn bertahan) ---
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

        // 1. Tentukan siapa yang akan diserang musuh turn ini
        targetedPlayerID = battleController.GetRandomTargetID();

        // 2. Amankan target jika ada salah satu player yang sudah mati/tidak aktif
        if (targetedPlayerID == 0 && (player1Obj == null || !player1Obj.activeInHierarchy)) targetedPlayerID = 1;
        if (targetedPlayerID == 1 && (player2Obj == null || !player2Obj.activeInHierarchy)) targetedPlayerID = 0;

        isActive = true;
        slider.value = 100;
        movingRight = false;

        RandomizePerfectZone();

        // ===================================================================
        // --- [BARU] LOGIKA ANIMASI DEFENSE SPESIFIK TARGET ---
        // ===================================================================
        // Jika targetedPlayerID == 0 (Player 1), maka P1 akan bernilai TRUE (Defense) dan P2 bernilai FALSE (Idle)
        // Jika targetedPlayerID == 1 (Player 2), maka P1 akan bernilai FALSE (Idle) dan P2 bernilai TRUE (Defense)
        // ===================================================================
        if (p1Animator != null && player1Obj != null && player1Obj.activeInHierarchy) 
        {
            p1Animator.SetBool("IsDefending", targetedPlayerID == 0);
        }
            
        if (p2Animator != null && player2Obj != null && player2Obj.activeInHierarchy) 
        {
            p2Animator.SetBool("IsDefending", targetedPlayerID == 1);
        }
        // ===================================================================
    }

    // Coroutine bantuan untuk memberikan kedipan indikator siapa yang ditargetkan musuh
    IEnumerator BlinkTargetFeedback(GameObject target)
    {
        SpriteRenderer sr = target.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;
        Color originalColor = sr.color;
        // Berkedip warna merah tipis sebagai tanda bahaya target
        Color alertColor = new Color(1f, 0.3f, 0.3f, 1f); 
        for (int i = 0; i < 2; i++)
        {
            sr.color = alertColor;
            yield return new WaitForSeconds(0.1f);
            sr.color = originalColor;
            yield return new WaitForSeconds(0.1f);
        }
        sr.color = originalColor;
    }
}