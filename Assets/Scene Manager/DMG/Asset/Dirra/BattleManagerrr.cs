using UnityEngine;
using TMPro;
using System.Collections;

public class BattleManagerrr : MonoBehaviour
{
    public TMP_Text resultText;
    public TMP_Text roundText; 
    public TMP_Text[] roundTexts;
    public GameObject bracketPanel; 
    public GameObject gameOverPanel; 

    public GameObject[] playerHearts; 
    public GameObject[] enemyHearts;  

    public ParticleSystem winParticle;

    public SpriteRenderer playerSprite;
    public SpriteRenderer enemySprite;
    public Sprite batuSprite;
    public Sprite kertasSprite;
    public Sprite guntingSprite;
    public Sprite kepalSprite; 
    public Sprite idleSprite;  

    int playerHP = 5;
    int enemyHP = 5;
    int round = 1; 

    string[] choices = { "Batu", "Kertas", "Gunting" };
    bool isCooldown = false; 

    void Start()
    {
        roundText.text = "Round " + round;
        UpdateBracket(); 
        
        if (bracketPanel != null) bracketPanel.SetActive(false); 
        if (gameOverPanel != null) gameOverPanel.SetActive(false); 
        
        if (playerSprite != null) playerSprite.sprite = idleSprite;
        if (enemySprite != null) enemySprite.sprite = idleSprite;

        UpdateUI(); 
    }

    public void ChooseRock()
    {
        if (!isCooldown && playerHP > 0 && round <= 4) 
            StartCoroutine(BattleSequence("Batu"));
    }

    public void ChoosePaper()
    {
        if (!isCooldown && playerHP > 0 && round <= 4) 
            StartCoroutine(BattleSequence("Kertas"));
    }

    public void ChooseScissors()
    {
        if (!isCooldown && playerHP > 0 && round <= 4) 
            StartCoroutine(BattleSequence("Gunting"));
    }

    IEnumerator BattleSequence(string playerChoice)
    {
        isCooldown = true; 
        if (bracketPanel != null) bracketPanel.SetActive(false);
        resultText.text = "..."; 

        Sprite[] gambarAcak = { batuSprite, kertasSprite, guntingSprite, idleSprite, kepalSprite };

        // 1. EFEK ROULETTE / ACAK GAMBAR
        // Gunakan fitur looping dari AudioManager kamu agar suara tidak terpotong aneh
        if (AudioManager.instance != null) 
        {
            AudioManager.instance.PlayLoopingSFX("Random");
        }

        for (int i = 0; i < 30; i++) 
        {
            playerSprite.sprite = gambarAcak[Random.Range(0, gambarAcak.Length)];
            enemySprite.sprite = gambarAcak[Random.Range(0, gambarAcak.Length)];
            
            yield return new WaitForSeconds(0.1f); 
        }

        // --- Hentikan efek suara looping Random/Gacha secara presisi ---
        if (AudioManager.instance != null) 
        {
            AudioManager.instance.StopLoopingSFX();
        }

        // 2. MUNCULKAN PILIHAN FINAL TANGAN PLAYER
        if (playerChoice == "Batu") playerSprite.sprite = batuSprite;
        else if (playerChoice == "Kertas") playerSprite.sprite = kertasSprite;
        else if (playerChoice == "Gunting") playerSprite.sprite = guntingSprite;

        // 3. MUNCULKAN PILIHAN FINAL TANGAN ENEMY
        string enemyChoice = choices[Random.Range(0, 3)];
        
        if (enemyChoice == "Batu") enemySprite.sprite = batuSprite;
        else if (enemyChoice == "Kertas") enemySprite.sprite = kertasSprite;
        else if (enemyChoice == "Gunting") enemySprite.sprite = guntingSprite;

       // 4. HITUNG PEMENANG & MAINKAN SFX HASIL RONDE
        if(playerChoice == enemyChoice)
        {
            resultText.text = "SERI!";
        }
        else if(
            (playerChoice == "Batu" && enemyChoice == "Gunting") ||
            (playerChoice == "Kertas" && enemyChoice == "Batu") ||
            (playerChoice == "Gunting" && enemyChoice == "Kertas")
        )
        {
            enemyHP--;
            resultText.text = "MENANG!";
            
            // Panggil sfx "Win" melalui instance
            if (AudioManager.instance != null) AudioManager.instance.PlaySFX("Win");
        }
        else
        {
            playerHP--;
            resultText.text = "KALAH!";
            
            // Panggil sfx "Lose" melalui instance
            if (AudioManager.instance != null) AudioManager.instance.PlaySFX("Lose");
        }
        
        UpdateUI();
        CheckWinner();

        // 5. TUNGGU SEBELUM RONDE BERIKUTNYA
        yield return new WaitForSeconds(2.5f); 
        
        playerSprite.sprite = idleSprite;
        enemySprite.sprite = idleSprite;
        
        isCooldown = false; 
    }

    void UpdateUI()
    {
        for (int i = 0; i < playerHearts.Length; i++)
        {
            if (i < playerHP) playerHearts[i].SetActive(true);
            else playerHearts[i].SetActive(false);
        }

        for (int i = 0; i < enemyHearts.Length; i++)
        {
            if (i < enemyHP) enemyHearts[i].SetActive(true);
            else enemyHearts[i].SetActive(false);
        }
    }
    
    void CheckWinner()
    {
        if(playerHP <= 0)
        {
            resultText.text = "GAME OVER! ANDA KALAH";
            
            // SFX saat nyawa habis (Kalah total)
            if (AudioManager.instance != null) AudioManager.instance.PlaySFX("Kalah");
            
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
            return; 
        }

        if(enemyHP <= 0)
        {
            round++; 
            if(round > 4)
            {
                resultText.text = "ANDA JUARA GAME!";
                roundText.text = "Selesai!";
                
                // SFX saat memenangkan keseluruhan turnamen
                if (AudioManager.instance != null) AudioManager.instance.PlaySFX("Win");

                if (winParticle != null) winParticle.Play(); 
                UpdateBracket(); 
                if (bracketPanel != null) bracketPanel.SetActive(true);
                if (gameOverPanel != null) gameOverPanel.SetActive(true);
            }
            else
            {
                resultText.text = "LANJUT KE BABAK " + round;
                roundText.text = "Round " + round; 
                
                enemyHP = 5;
                playerHP = 5;
                UpdateUI(); 
                
                if (winParticle != null) winParticle.Play(); 
                UpdateBracket(); 
                if (bracketPanel != null) bracketPanel.SetActive(true);
            }
        }
    }

    void UpdateBracket()
    {
        if (roundTexts == null || roundTexts.Length == 0) return;
        for(int i = 0; i < roundTexts.Length; i++)
        {
            roundTexts[i].gameObject.SetActive(false); 
        }
        if (round - 1 < roundTexts.Length)
        {
            roundTexts[round - 1].gameObject.SetActive(true);
            roundTexts[round - 1].color = Color.yellow; 
        }
    }
}