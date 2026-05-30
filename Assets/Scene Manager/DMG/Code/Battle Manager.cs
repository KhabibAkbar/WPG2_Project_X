using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;

public enum ElemenTipe { Kosong, Batu, Kertas, Gunting }

public class BattleManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject mainUIContainer; 
    public DefenseManager DefenseManager;
    public Slider playerTimerSlider;
    public TextMeshProUGUI infoText; 
    public TextMeshProUGUI comboNotificationText; 

    [Header("Word Typing System")]
    public GameObject wordUIContainer; 
    public TextMeshProUGUI[] wordTexts; 
    
    // Kata bisa diganti langsung dari Inspector
    public string[] bankBatu = { "STONE", "BOULDER", "PEBBLE", "HARD", "HEAVY", "GRAVEL", "GRANITE", "MINERAL", "SMASH"  };
    public string[] bankKertas = { "SHEET", "PAGE", "THIN", "FLAT", "FOLD", "DOCUMENT", "PARCHMENT", "PULP", "WRAP", "STATIONERY" }; 
    public string[] bankGunting = { "BLADE", "SHARP", "CUT", "SHEAR", "SNIP", "METAL", "HANDLE", "TRIM", "SLICE", "TOOL" }; 

    [Header("Enemy Element Visuals")]
    public GameObject enemyObjectToDestroy; 
    public Image enemyUIPortrait;
    public Sprite portraitBatu;
    public Sprite portraitKertas;
    public Sprite portraitGunting;
    public AnimatorOverrideController animBatu;
    public AnimatorOverrideController animKertas;
    public AnimatorOverrideController animGunting;

    [Header("Scene Transition")]
    [Tooltip("Ketik nama scene yang akan di-load setelah musuh berhasil ditangkap")]
    public string nextSceneAfterCatch = "NamaSceneTujuan";

    [Header("Health & Status UI")]
    public Slider enemyHealthSlider;
    public Image[] playerHearts; 
    public GameObject playerObj; 

    [Header("Player Portraits")]
    public Image playerPortrait;
    public Sprite playerAliveSprite;
    public Sprite playerDeadSprite;

    [Header("Ready Go Cutscene")]
    public CanvasGroup cutscenePanelGroup;
    public GameObject readyImage;
    public GameObject goImage;

    [Header("Catching System")]
    public GameObject catchingUIContainer;
    public TextMeshProUGUI catchingWordText;
    public Slider catchingTimerSlider;
    public float catchingTimeLimit = 3f;
    private float currentCatchingTimer;
    private bool isCatchingPhase = false;
    private string targetCatchingWord = "CATCHING";
    private string currentCatchingWord = "";

    [Header("Caught Inventory UI")]
    public TextMeshProUGUI totalBatuText;
    public TextMeshProUGUI totalKertasText;
    public TextMeshProUGUI totalGuntingText;

    [Header("Battle Logic")]
    private int playerHealth = 3; 
    private float enemyHealth = 80f;
    private float maxEnemyHealth = 80f; 
    private bool isGameOver = false;

    private int enemyAttacksLeft = 0; 
    private string targetWord = ""; 
    private string currentTypedWord = ""; 
    private ElemenTipe currentBossElement; 
    private Dictionary<string, ElemenTipe> activeWordsMap = new Dictionary<string, ElemenTipe>();

    [Header("Timer Settings")]
    public float timePerTurn = 60f;
    private float currentTimer;
    private bool isPlayerTurn = false;
    private bool isStartingCutscene = true;

    [Header("Animators")]
    public Animator playerAnimator; 
    public Animator enemyAnimator;

    void Start()
    {
        isGameOver = false;
        
        if (comboNotificationText != null) comboNotificationText.gameObject.SetActive(false);
        if (wordUIContainer != null) wordUIContainer.SetActive(false);
        if (catchingUIContainer != null) catchingUIContainer.SetActive(false);

        enemyHealth = maxEnemyHealth;
        enemyHealthSlider.maxValue = maxEnemyHealth; 
        enemyHealthSlider.value = enemyHealth;
        playerTimerSlider.maxValue = timePerTurn;
        playerTimerSlider.value = timePerTurn;

        UpdateHeartUI();
        UpdatePlayerPortraits();
        UpdateCaughtInventoryUI();

        StartCoroutine(PlayReadyGo());
    }

    void Update()
    {
        if (isGameOver) return;
        
        if (isCatchingPhase)
        {
            HandleCatchingTimer();
            HandleCatchingInput();
            return;
        }

        if (isPlayerTurn && !isStartingCutscene)
        {
            HandleTimer();
            HandleTypingInput();
        }
    }

    // ================= CATCHING SYSTEM =================

    void StartCatchingPhase()
    {
        isCatchingPhase = true;
        isPlayerTurn = false;
        currentCatchingWord = "";
        
        if (playerTimerSlider != null) playerTimerSlider.gameObject.SetActive(false);
        if (wordUIContainer != null) wordUIContainer.SetActive(false);
        if (DefenseManager != null) DefenseManager.gameObject.SetActive(false);
        
        currentCatchingTimer = catchingTimeLimit; 

        if (catchingUIContainer != null) catchingUIContainer.SetActive(true);
        if (catchingTimerSlider != null) 
        {
            catchingTimerSlider.gameObject.SetActive(true); 
            catchingTimerSlider.maxValue = catchingTimeLimit;
            catchingTimerSlider.value = currentCatchingTimer; 
        }

        UpdateVisualCatchingWord();
        
        if (enemyAnimator != null) enemyAnimator.SetTrigger("Hurt"); 
    }

    void HandleCatchingTimer()
    {
        currentCatchingTimer -= Time.deltaTime; 
        
        if (catchingTimerSlider != null)
        {
            catchingTimerSlider.value = currentCatchingTimer;
        }

        if (currentCatchingTimer <= 0)
        {
            FailCatching();
        }
    }

    void HandleCatchingInput()
    {
        if (!Input.anyKeyDown) return;

        string input = Input.inputString.ToUpper();
        foreach (char c in input)
        {
            if (!isCatchingPhase) break;
            if (c == '\b' || c == '\n' || c == '\r') continue;
            
            int targetIndex = currentCatchingWord.Length;
            
            if (targetIndex >= targetCatchingWord.Length) return;

            if (targetCatchingWord[targetIndex] == c)
            {
                currentCatchingWord += c;
                if (AudioManager.instance != null) AudioManager.instance.PlaySFX("SFC Clik");
                UpdateVisualCatchingWord();

                if (currentCatchingWord == targetCatchingWord)
                {
                    SuccessCatching();
                }
            }
            else
            {
                if (AudioManager.instance != null) AudioManager.instance.PlaySFX("Miss");
            }
        }
    }

    void UpdateVisualCatchingWord()
    {
        if (catchingWordText == null) return;
        string sudahDiketik = $"<color=#00FF00>{currentCatchingWord}</color>";
        string sisaBelum = targetCatchingWord.Substring(currentCatchingWord.Length);
        catchingWordText.text = sudahDiketik + sisaBelum;
    }

    void SuccessCatching()
    {
        isCatchingPhase = false;
        if (catchingUIContainer != null) catchingUIContainer.SetActive(false);
        
        if (playerAnimator != null) playerAnimator.SetTrigger("Catch");

        SimpanDataTangkapan(currentBossElement);
        
        // Eksekusi kehancuran musuh dan pindah scene
        StartCoroutine(EnemyDefeatedSequence());
    }

    IEnumerator EnemyDefeatedSequence()
    {
        isGameOver = true;
        isPlayerTurn = false;

        // Mainkan SFX Win (nama sesuai script lamamu)
        if (AudioManager.instance != null) AudioManager.instance.PlaySFX("WinSound");

        // Hancurkan musuh
        if (enemyObjectToDestroy != null) 
        {
            Destroy(enemyObjectToDestroy);
        }

        // Tunggu sebentar untuk memberi waktu animasi/feedback visual
        yield return new WaitForSeconds(1.5f);

        // Pindah scene
        if (!string.IsNullOrEmpty(nextSceneAfterCatch))
        {
            SceneManager.LoadScene(nextSceneAfterCatch);
        }
        else
        {
            Debug.LogWarning("Nama scene tujuan belum diisi di Inspector!");
        }
    }

    void SimpanDataTangkapan(ElemenTipe elemenTertangkap)
    {
        if (elemenTertangkap == ElemenTipe.Batu)
        {
            int jumlah = PlayerPrefs.GetInt("CaughtBatu", 0);
            PlayerPrefs.SetInt("CaughtBatu", jumlah + 1);
        }
        else if (elemenTertangkap == ElemenTipe.Kertas)
        {
            int jumlah = PlayerPrefs.GetInt("CaughtKertas", 0);
            PlayerPrefs.SetInt("CaughtKertas", jumlah + 1);
        }
        else if (elemenTertangkap == ElemenTipe.Gunting)
        {
            int jumlah = PlayerPrefs.GetInt("CaughtGunting", 0);
            PlayerPrefs.SetInt("CaughtGunting", jumlah + 1);
        }

        PlayerPrefs.Save(); 
        UpdateCaughtInventoryUI(); 
    }

    public void UpdateCaughtInventoryUI()
    {
        if (totalBatuText != null) 
            totalBatuText.text = "Batu: " + PlayerPrefs.GetInt("CaughtBatu", 0);
        
        if (totalKertasText != null) 
            totalKertasText.text = "Kertas: " + PlayerPrefs.GetInt("CaughtKertas", 0);
        
        if (totalGuntingText != null) 
            totalGuntingText.text = "Gunting: " + PlayerPrefs.GetInt("CaughtGunting", 0);
    }

    void FailCatching()
    {
        isCatchingPhase = false;
        if (catchingUIContainer != null) catchingUIContainer.SetActive(false);
        
        enemyHealth = maxEnemyHealth * 0.2f; 
        enemyHealthSlider.value = enemyHealth;
        
        // --- NAMA SFX DIUBAH MENJADI SESUAI YANG LAMA ---
        if (AudioManager.instance != null) AudioManager.instance.PlaySFX("Enemy Terkena Hit"); 
        
        StartCoroutine(ReviveSequence());
    }

    IEnumerator ReviveSequence()
    {
        yield return new WaitForSeconds(1f);
        StartPlayerTurn(); 
    }

    // ================= TYPING & ELEMENT LOGIC =================

    public void StartPlayerTurn()
    {
        if (isStartingCutscene || isGameOver) return;

        isPlayerTurn = true;
        currentTimer = timePerTurn;
        playerTimerSlider.gameObject.SetActive(true);
        
        if (DefenseManager != null) DefenseManager.gameObject.SetActive(false);
        if (enemyAnimator != null) enemyAnimator.SetBool("IsDefending", true);

        if (wordUIContainer != null) wordUIContainer.SetActive(true);

        RandomizeBossElement();
        SpawnKataPilihan();
        UpdateUI();
    }

    void RandomizeBossElement()
    {
        currentBossElement = (ElemenTipe)Random.Range(1, 4); 
        
        if (currentBossElement == ElemenTipe.Batu)
        {
            if (enemyUIPortrait != null) enemyUIPortrait.sprite = portraitBatu;
            if (enemyAnimator != null && animBatu != null) enemyAnimator.runtimeAnimatorController = animBatu;
        }
        else if (currentBossElement == ElemenTipe.Kertas)
        {
            if (enemyUIPortrait != null) enemyUIPortrait.sprite = portraitKertas;
            if (enemyAnimator != null && animKertas != null) enemyAnimator.runtimeAnimatorController = animKertas;
        }
        else if (currentBossElement == ElemenTipe.Gunting)
        {
            if (enemyUIPortrait != null) enemyUIPortrait.sprite = portraitGunting;
            if (enemyAnimator != null && animGunting != null) enemyAnimator.runtimeAnimatorController = animGunting;
        }
    }

    void SpawnKataPilihan()
    {
        activeWordsMap.Clear();
        targetWord = "";
        currentTypedWord = "";

        List<string> selectedWords = new List<string>();

        selectedWords.AddRange(AmbilKataAcak(bankBatu, 2, ElemenTipe.Batu));
        selectedWords.AddRange(AmbilKataAcak(bankKertas, 2, ElemenTipe.Kertas));
        selectedWords.AddRange(AmbilKataAcak(bankGunting, 2, ElemenTipe.Gunting));

        for (int i = 0; i < selectedWords.Count; i++)
        {
            int rnd = Random.Range(0, selectedWords.Count);
            string temp = selectedWords[rnd];
            selectedWords[rnd] = selectedWords[i];
            selectedWords[i] = temp;
        }

        for (int i = 0; i < wordTexts.Length; i++)
        {
            if (i < selectedWords.Count)
            {
                wordTexts[i].text = selectedWords[i];
                wordTexts[i].color = Color.white; 
                wordTexts[i].gameObject.SetActive(true);
            }
        }
    }

    List<string> AmbilKataAcak(string[] bank, int jumlah, ElemenTipe tipe)
    {
        List<string> hasil = new List<string>();
        List<string> copyBank = new List<string>(bank);

        for (int i = 0; i < jumlah; i++)
        {
            if (copyBank.Count == 0) break;
            int idx = Random.Range(0, copyBank.Count);
            string kata = copyBank[idx].ToUpper();
            
            hasil.Add(kata);
            activeWordsMap.Add(kata, tipe); 
            copyBank.RemoveAt(idx); 
        }
        return hasil;
    }

    void HandleTypingInput()
    {
        if (!Input.anyKeyDown) return;

        string input = Input.inputString.ToUpper();

        foreach (char c in input)
        {
            if (!isPlayerTurn) break;
            if (c == '\b' || c == '\n' || c == '\r') continue; 
            ProsesKetikan(c);
        }
    }

    void ProsesKetikan(char hurufDitekan)
    {
        string potentialTypedWord = currentTypedWord + hurufDitekan;
        bool matchFound = false;
        string completedWord = null;

        foreach (string kata in activeWordsMap.Keys)
        {
            if (kata == potentialTypedWord)
            {
                matchFound = true;
                completedWord = kata;
                break;
            }
            else if (kata.StartsWith(potentialTypedWord))
            {
                matchFound = true;
            }
        }

        if (matchFound)
        {
            currentTypedWord = potentialTypedWord;
            if (AudioManager.instance != null) AudioManager.instance.PlaySFX("SFC Clik");
            UpdateVisualKataDiLayar();

            if (completedWord != null)
            {
                targetWord = completedWord; 
                EksekusiSeranganDariKetikan();
            }
        }
        else
        {
            if (AudioManager.instance != null) AudioManager.instance.PlaySFX("Miss");
            currentTypedWord = ""; 
            UpdateVisualKataDiLayar();
        }
    }

    void UpdateVisualKataDiLayar()
    {
        foreach (TextMeshProUGUI txt in wordTexts)
        {
            if (txt.gameObject.activeSelf == false) continue;

            string kataAsli = "";
            foreach (string key in activeWordsMap.Keys)
            {
                string plainText = txt.text.Replace("<color=#00FF00>", "").Replace("</color>", "");
                if (plainText == key)
                {
                    kataAsli = key;
                    break;
                }
            }

            if (!string.IsNullOrEmpty(currentTypedWord) && kataAsli.StartsWith(currentTypedWord))
            {
                string sudahDiketik = $"<color=#00FF00>{currentTypedWord}</color>";
                string sisaBelum = kataAsli.Substring(currentTypedWord.Length);
                txt.text = sudahDiketik + sisaBelum;
                txt.color = Color.white;
            }
            else if (string.IsNullOrEmpty(currentTypedWord))
            {
                txt.text = kataAsli; 
                txt.color = Color.white; 
            }
            else
            {
                txt.text = kataAsli;
                txt.color = Color.gray; 
            }
        }
    }

    void EksekusiSeranganDariKetikan()
    {
        isPlayerTurn = false;

        ElemenTipe elemenPemain = activeWordsMap[targetWord];
        float damageMultiplier = HitungMultiplierDamage(elemenPemain, currentBossElement);
        
        EksekusiSeranganVisual(elemenPemain);

        float damageAkhir = 15f * damageMultiplier; 
        enemyHealth -= damageAkhir;

        if (enemyAnimator != null) 
        {
            enemyAnimator.SetBool("IsDefending", false);
            enemyAnimator.SetTrigger("Hurt");
        }

        // --- NAMA SFX DIUBAH MENJADI SESUAI YANG LAMA ---
        if (AudioManager.instance != null) AudioManager.instance.PlaySFX("Enemy Terkena Hit");

        if (comboNotificationText != null) 
        {
            if (damageMultiplier > 1f) comboNotificationText.text = "SUPER EFFECTIVE!";
            else if (damageMultiplier < 1f) comboNotificationText.text = "NOT EFFECTIVE...";
            else comboNotificationText.text = "GOOD ATTACK!";
            
            comboNotificationText.gameObject.SetActive(true);
            StartCoroutine(HideNotification());
        }

        enemyHealthSlider.value = Mathf.Max(0, enemyHealth);
        
        if (wordUIContainer != null) wordUIContainer.SetActive(false);

        if (enemyHealth <= 0) 
        {
            StartCatchingPhase();
            return;
        }

        Invoke(nameof(StartEnemyTurn), 0.8f); 
    }

    IEnumerator HideNotification()
    {
        yield return new WaitForSeconds(1.2f);
        if (comboNotificationText != null) comboNotificationText.gameObject.SetActive(false);
    }

    float HitungMultiplierDamage(ElemenTipe elemenPemain, ElemenTipe elemenBos)
    {
        if (elemenPemain == elemenBos) return 1.0f; 

        if ((elemenPemain == ElemenTipe.Gunting && elemenBos == ElemenTipe.Kertas) ||
            (elemenPemain == ElemenTipe.Kertas && elemenBos == ElemenTipe.Batu) ||
            (elemenPemain == ElemenTipe.Batu && elemenBos == ElemenTipe.Gunting))
        {
            return 1.5f; 
        }

        return 0.5f; 
    }

    // ================= CORE BATTLE LOGIC & VISUALS =================

    void HandleTimer()
    {
        currentTimer -= Time.deltaTime;
        playerTimerSlider.value = currentTimer;
        
        if (currentTimer <= 0 && isPlayerTurn) 
        {
            if (enemyAnimator != null) enemyAnimator.SetBool("IsDefending", false);
            targetWord = "";
            currentTypedWord = "";
            if (wordUIContainer != null) wordUIContainer.SetActive(false);

            StartEnemyTurn();
        }
    }

    public void StartEnemyTurn()
    {
        if (isGameOver) return;
        
        UpdateUI();

        isPlayerTurn = false;
        playerTimerSlider.gameObject.SetActive(false);

        if (DefenseManager != null)
        {
            DefenseManager.gameObject.SetActive(true);
            enemyAttacksLeft = 1; 
            DefenseManager.ActivateDefense();
        }
    }

    void TriggerNextDefense()
    {
        if (!isGameOver && DefenseManager != null) DefenseManager.ActivateDefense();
    }

    public void PlayerTakeDamage()
    {
        if (isGameOver) return;

        playerHealth--; 
        if (playerAnimator != null) playerAnimator.SetTrigger("Hurt"); 
        if (playerObj != null) StartCoroutine(BlinkEffect(playerObj)); 
        
        // --- NAMA SFX DIUBAH MENJADI SESUAI YANG LAMA ---
        if (AudioManager.instance != null) AudioManager.instance.PlaySFX("Player terkena hit");
        
        UpdateHeartUI();
        UpdatePlayerPortraits();
        CheckPlayerLives();
    }

    public void EndEnemyAttack()
    {
        if (isGameOver) return;

        if (playerHealth <= 0)
        {
            PlayerDied();
            return;
        }

        enemyAttacksLeft--;
        if (enemyAttacksLeft > 0) Invoke(nameof(TriggerNextDefense), 1.0f); 
        else StartPlayerTurn(); 
    }

    public void PlayerSuccessDefense() 
    { 
        if (isGameOver) return;

        if (playerAnimator != null) playerAnimator.SetTrigger("Dodge");
        
        // --- TAMBAHAN SFX DEFENSE SESUAI SCRIPT LAMA ---
        if (AudioManager.instance != null) AudioManager.instance.PlaySFX("Player Defense");

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
        
        // Memakai PlaySFXDirect seperti di BattleController
        if (AudioManager.instance != null) AudioManager.instance.PlaySFXDirect("Ready");
        yield return new WaitForSeconds(1.2f);
        readyImage.SetActive(false);

        goImage.SetActive(true);
        
        // Memakai PlaySFXDirect seperti di BattleController
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

    void EksekusiSeranganVisual(ElemenTipe elemenSerangan)
    {
       StartCoroutine(EfekMaju(playerObj.transform, playerAnimator, elemenSerangan));
    }

    IEnumerator EfekMaju(Transform playerTr, Animator anim, ElemenTipe elemenSerangan)
    {
        Vector3 posAwal = playerTr.position;
        Transform enemyTr = DefenseManager.enemyObj.transform;
        SpriteRenderer pSR = playerTr.GetComponent<SpriteRenderer>();
        SpriteRenderer eSR = enemyTr.GetComponent<SpriteRenderer>();
        
        float pWidth = pSR != null ? pSR.bounds.extents.x : 0.5f;
        float eWidth = eSR != null ? eSR.bounds.extents.x : 0.5f;
        float spacing = 0.5f;
        Vector3 posSerang = new Vector3(enemyTr.position.x - (eWidth + pWidth + spacing), posAwal.y, posAwal.z);

        float moveSpeed = 50f;
        
        while (Vector3.Distance(playerTr.position, posSerang) > 0.1f)
        {
            playerTr.position = Vector3.MoveTowards(playerTr.position, posSerang, Time.deltaTime * moveSpeed);
            yield return null;
        }
        playerTr.position = posSerang;

        if (anim != null) 
        {
            anim.SetInteger("AttackType", (int)elemenSerangan);
            anim.SetTrigger("Attack"); 
        }
        
        // --- NAMA SFX DIUBAH MENJADI SESUAI YANG LAMA ---
        // (Semua serangan sekarang memanggil "P1 Attack" tanpa membedakan batu/kertas/gunting)
        if (AudioManager.instance != null) 
        {
            AudioManager.instance.PlaySFX("P1 Attack");
        }

        yield return new WaitForSeconds(0.4f);

        while (Vector3.Distance(playerTr.position, posAwal) > 0.1f)
        {
            playerTr.position = Vector3.MoveTowards(playerTr.position, posAwal, Time.deltaTime * 40f);
            yield return null;
        }
        playerTr.position = posAwal;
    }

    void PlayerDied()
    {
        if (isGameOver) return;
        isGameOver = true;
        isPlayerTurn = false;
        
        // Langsung restart scene karena panel lose dihapus
        RetryGame();
    }

    void CheckPlayerLives()
    {
        if (playerHealth <= 0)
        {
            if (playerObj != null) playerObj.SetActive(false);
            PlayerDied();
        }
    }

    void UpdatePlayerPortraits()
    {
        if (playerPortrait != null) playerPortrait.sprite = (playerHealth > 0) ? playerAliveSprite : playerDeadSprite;
    }

    void UpdateHeartUI()
    {
        for (int i = 0; i < playerHearts.Length; i++) playerHearts[i].enabled = (i < playerHealth);
    }

    public void RetryGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void UpdateUI()
    {
        string elemenString = currentBossElement == ElemenTipe.Kosong ? "?" : currentBossElement.ToString();
        
        if (infoText != null)
        {
            infoText.text = $"Elemen Bos Saat Ini: {elemenString}";
        }
    }
}