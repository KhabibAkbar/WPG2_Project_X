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
    public float defenseTolerance = 5f; 

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

        if (Mathf.Abs(slider.value - targetSliderValue) <= defenseTolerance)
        {
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

        // 1. MAJU
        float moveSpeed = 50f;
        while (Vector3.Distance(enemyTr.position, attackPos) > 0.1f)
        {
            enemyTr.position = Vector3.MoveTowards(enemyTr.position, attackPos, Time.deltaTime * moveSpeed);
            yield return null;
        }
        enemyTr.position = attackPos;

        // 2. HIT (Trigger Animasi Serang Musuh & Suara)
        if (enemyAnimator != null) enemyAnimator.SetTrigger("Attack"); 
        if (AudioManager.instance != null) AudioManager.instance.PlaySFX("Enemy_Attack");

        // --- FIX: Jeda 1.2 detik menunggu suara pukulan mendarat ---
        yield return new WaitForSeconds(1.2f); 

        // 3. DAMAGE (Player kena damage, animasi Hurt, & suara Hurt keluar)
        battleController.PlayerTakeDamage(targetID);

        // --- FIX: Jeda 0.8 detik untuk sisa animasi & suara sebelum mundur ---
        yield return new WaitForSeconds(0.8f); 

        // 4. MUNDUR
        float returnSpeed = 40f;
        while (Vector3.Distance(enemyTr.position, startPos) > 0.1f)
        {
            enemyTr.position = Vector3.MoveTowards(enemyTr.position, startPos, Time.deltaTime * returnSpeed);
            yield return null;
        }

        // 5. LOCK POSISI & SELESAI
        enemyTr.position = startPos;
        isAttacking = false; 
        attackRoutine = null;

        battleController.EndEnemyAttack();
    }

    void RandomizePerfectZone()
    {
        targetSliderValue = Random.Range(15f, 85f);
        float percentage = targetSliderValue / 100f; 

        perfectZone.pivot = new Vector2(0.5f, 0.5f);
        perfectZone.anchorMin = new Vector2(percentage, 0.5f);
        perfectZone.anchorMax = new Vector2(percentage, 0.5f);
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

        Debug.Log("Fungsi ActivateDefense DIPANGGIL! Mencoba menyalakan animasi...");

        if (p1Animator != null && player1Obj != null && player1Obj.activeInHierarchy) 
        {
            p1Animator.SetBool("IsDefending", true);
            Debug.Log("Animasi IsDefending untuk P1 BERHASIL DITEMBAK!");
        }
        else
        {
            Debug.LogWarning("P1 Animator GAGAL ditembak. p1Animator Null atau objek mati!");
        }
            
        if (p2Animator != null && player2Obj != null && player2Obj.activeInHierarchy) 
        {
            p2Animator.SetBool("IsDefending", true);
            Debug.Log("Animasi IsDefending untuk P2 BERHASIL DITEMBAK!");
        }
        else
        {
            Debug.LogWarning("P2 Animator GAGAL ditembak. p2Animator Null atau objek mati!");
        }
    }
}