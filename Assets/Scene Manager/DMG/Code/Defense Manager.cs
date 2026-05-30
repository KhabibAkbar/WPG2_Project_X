using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class DefenseManager : MonoBehaviour
{
    public BattleManager BattleManager;

    [Header("Defense UI & Logic")]
    public TextMeshProUGUI defenseWordText; 
    public Slider defenseTimerSlider;       
    public float timeLimit = 2f;            

    private string[] defenseWords = { "BLOCK", "BEWARE", "JUMP", "DODGE", "DEFEND", "RETREAT" };
    private string targetWord = "";
    private string currentTypedWord = "";
    private float currentDefenseTimer;
    private bool isActive = false;

    [Header("Enemy & Player Reference")]
    public GameObject enemyObj;
    public GameObject playerObj; // Single Player

    [Header("Animators")]
    public Animator playerAnimator; // Single Player
    public Animator enemyAnimator;

    [Header("Enemy Start Point")]
    public Transform enemyStartPoint;

    private Coroutine attackRoutine;
    private bool isAttacking = false;

    void Update()
    {
        if (isActive && !isAttacking)
        {
            HandleDefenseTimer();
            HandleTypingInput();
        }
    }

    void LateUpdate()
    {
        if (!isAttacking && enemyObj != null && enemyStartPoint != null)
        {
            enemyObj.transform.position = enemyStartPoint.position;
        }
    }

    public void ActivateDefense()
    {
        isActive = true;
        isAttacking = false;

        targetWord = defenseWords[Random.Range(0, defenseWords.Length)];
        currentTypedWord = "";

        if (defenseWordText != null)
        {
            defenseWordText.gameObject.SetActive(true);
            UpdateVisualKata();
        }

        currentDefenseTimer = timeLimit;
        if (defenseTimerSlider != null)
        {
            defenseTimerSlider.maxValue = timeLimit;
            defenseTimerSlider.value = timeLimit;
            defenseTimerSlider.gameObject.SetActive(true);
        }

        if (playerAnimator != null && playerObj != null && playerObj.activeInHierarchy) 
            playerAnimator.SetBool("IsDefending", true);
    }

    void HandleDefenseTimer()
    {
        currentDefenseTimer -= Time.deltaTime;
        if (defenseTimerSlider != null) defenseTimerSlider.value = currentDefenseTimer;

        if (currentDefenseTimer <= 0)
        {
            FailDefense();
        }
    }

    void HandleTypingInput()
    {
        if (!Input.anyKeyDown) return;

        string input = Input.inputString.ToUpper();
        foreach (char c in input)
        {
            if (c == '\b' || c == '\n' || c == '\r') continue;

            int targetIndex = currentTypedWord.Length;
            
            if (targetIndex < targetWord.Length && targetWord[targetIndex] == c)
            {
                currentTypedWord += c;
                if (AudioManager.instance != null) AudioManager.instance.PlaySFX("SFC Clik");
                UpdateVisualKata();

                if (currentTypedWord == targetWord)
                {
                    SuccessDefense();
                }
            }
            else
            {
                if (AudioManager.instance != null) AudioManager.instance.PlaySFX("Miss");
            }
        }
    }

    void UpdateVisualKata()
    {
        if (defenseWordText == null) return;
        string sudahDiketik = $"<color=#00FF00>{currentTypedWord}</color>"; 
        string sisaBelum = targetWord.Substring(currentTypedWord.Length);
        defenseWordText.text = sudahDiketik + sisaBelum;
    }

    void SuccessDefense()
    {
        isActive = false;
        HideUI();

        if (playerAnimator != null) playerAnimator.SetBool("IsDefending", false);
        
        BattleManager.PlayerSuccessDefense();
    }

    void FailDefense()
    {
        isActive = false;
        HideUI();

        if (playerAnimator != null) playerAnimator.SetBool("IsDefending", false);

        if (enemyObj != null && playerObj != null)
        {
            if (attackRoutine != null) StopCoroutine(attackRoutine);
            attackRoutine = StartCoroutine(EnemyAttackVisual(enemyObj.transform, playerObj.transform));
        }
        else
        {
            BattleManager.PlayerTakeDamage();
            BattleManager.EndEnemyAttack();
        }
    }

    void HideUI()
    {
        if (defenseWordText != null) defenseWordText.gameObject.SetActive(false);
        if (defenseTimerSlider != null) defenseTimerSlider.gameObject.SetActive(false);
    }

    IEnumerator EnemyAttackVisual(Transform enemyTr, Transform playerTr)
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

        float moveSpeed = 50f;
        while (Vector3.Distance(enemyTr.position, attackPos) > 0.1f)
        {
            enemyTr.position = Vector3.MoveTowards(enemyTr.position, attackPos, Time.deltaTime * moveSpeed);
            yield return null;
        }
        enemyTr.position = attackPos;

        if (enemyAnimator != null) enemyAnimator.SetTrigger("Attack"); 
        if (AudioManager.instance != null) AudioManager.instance.PlaySFX("Enemy_Attack");
        yield return new WaitForSeconds(1.2f); 

        BattleManager.PlayerTakeDamage();
        yield return new WaitForSeconds(0.8f); 

        float returnSpeed = 40f;
        while (Vector3.Distance(enemyTr.position, startPos) > 0.1f)
        {
            enemyTr.position = Vector3.MoveTowards(enemyTr.position, startPos, Time.deltaTime * returnSpeed);
            yield return null;
        }

        enemyTr.position = startPos;
        isAttacking = false; 
        attackRoutine = null;

        BattleManager.EndEnemyAttack();
    }

    private void OnDisable()
    {
        HideUI();
        if (playerAnimator != null) playerAnimator.SetBool("IsDefending", false);
    }
}