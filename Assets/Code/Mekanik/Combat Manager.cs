using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CombatManager : MonoBehaviour
{
    [Header("UI & ICONS")]
    public Transform iconContainer; // Panel tempat icon tombol muncul
    public GameObject[] keyIconPrefabs; // Array prefab: 0:W, 1:A, 2:D, 3:Left, 4:Right, 5:Down
    public Slider timerSlider;
    public Slider healthSlider;
    public Text keysCollectedText;

    [Header("Game State")]
    public int totalKeysToWin = 8;
    public int maxCombats = 12;
    private int currentKeys = 0;
    private int combatCount = 0;
    private int playerHealth = 3;

    [Header("Difficulty Settings")]
    public float baseTimeLimit = 6f;
    private float currentTimeLimit;
    
    private List<int> requiredSequence = new List<int>(); 
    private int currentStep = 0;
    private float timer;
    private bool isCombatActive = false;

    [Header("Animations")]
    public Animator playerAnimator;
    public Animator enemyAnimator;

    void Start()
    {
        healthSlider.maxValue = playerHealth;
        healthSlider.value = playerHealth;
        UpdateUI();
        StartNewCombat();
    }

    void Update()
    {
        if (!isCombatActive || playerHealth <= 0) return;

        timer -= Time.deltaTime;
        timerSlider.value = timer / currentTimeLimit;

        if (timer <= 0) FailCombat();

        CheckInput();
    }

    void StartNewCombat()
    {
        if (combatCount >= maxCombats || currentKeys >= totalKeysToWin) return;

        combatCount++;
        currentStep = 0;
        
        // --- LOGIKA BALANCING ---
        // Menambah jumlah tombol (3 sampai 7) berdasarkan progress
        int sequenceLength = 3 + (currentKeys / 2); 
        // Mengurangi waktu setiap kali berhasil (minimal 2 detik)
        currentTimeLimit = Mathf.Max(2f, baseTimeLimit - (currentKeys * 0.4f));
        
        timer = currentTimeLimit;
        GenerateSequence(sequenceLength);
        isCombatActive = true;
    }

    void GenerateSequence(int length)
    {
        // Hapus icon lama
        foreach (Transform child in iconContainer) Destroy(child.gameObject);
        requiredSequence.Clear();

        for (int i = 0; i < length; i++)
        {
            int randomIndex = Random.Range(0, 6); // 6 jenis tombol
            requiredSequence.Add(randomIndex);
            
            // Munculkan icon di UI
            Instantiate(keyIconPrefabs[randomIndex], iconContainer);
        }
    }

    void CheckInput()
    {
        // Mapping index ke KeyCode
        KeyCode[] codes = { KeyCode.W, KeyCode.A, KeyCode.D, KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.DownArrow };
        
        if (Input.anyKeyDown)
        {
            if (Input.GetKeyDown(codes[requiredSequence[currentStep]]))
            {
                // Visual feedback: Ubah warna icon yang benar jadi hijau
                iconContainer.GetChild(currentStep).GetComponent<Image>().color = Color.green;
                currentStep++;

                if (currentStep >= requiredSequence.Count) WinCombat();
            }
            else if (!Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(1))
            {
                FailCombat();
            }
        }
    }

    void WinCombat()
    {
        isCombatActive = false;
        currentKeys++;
        UpdateUI();
        
        playerAnimator.SetTrigger("Attack"); // Trigger animasi serang
        Debug.Log("Dapat Kunci! Total: " + currentKeys);

        if (currentKeys >= totalKeysToWin)
        {
            Debug.Log("PLAYER MENANG!");
            // Pindah ke scene menang
        }
        else
        {
            Invoke("StartNewCombat", 2f);
        }
    }

    void FailCombat()
    {
        isCombatActive = false;
        playerHealth--;
        healthSlider.value = playerHealth;
        
        enemyAnimator.SetTrigger("EnemyAttack"); // Enemy menyerang
        playerAnimator.SetTrigger("Hurt");       // Player kena damage

        if (playerHealth <= 0)
        {
            Debug.Log("GAME OVER");
        }
        else
        {
            Invoke("StartNewCombat", 2f);
        }
    }

    void UpdateUI()
    {
        keysCollectedText.text = "Kunci: " + currentKeys + "/" + totalKeysToWin;
    }
}