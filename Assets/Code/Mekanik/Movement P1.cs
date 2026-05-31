using UnityEngine;

public class MovementP1 : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 8f;
    public float slowmoDorong = 2f;
    public float jarakDorong = 0.6f;

    [Header("Audio Settings")]
    public float footstepInterval = 0.4f;
    private float footstepTimer;

    public float pushSFXInterval = 0.5f;
    private float pushSFXTimer;

    [Header("Visual Effects")]
    public ParticleSystem footstepParticles;
    public ParticleSystem pushWindParticles; // Partikel angin saat mendorong

    public float jumpBufferTime = 0.1f;
    private float jumpBufferCounter;

    public float animationInputDelay = 0.15f;
    private float inputBufferTimer;
    private int lastLockedInput = 0;

    private Rigidbody2D rb;
    private float moveInput;
    public bool isGrounded;
    private bool isDorong;

    public LayerMask pushLayer;
    public LayerMask groundLayer;
    public LayerMask platformLayer;

    private Collider2D playerCol;
    private Collider2D targetCol;

    private bool nahanPlatform;
    private bool kenaDorongKiri;
    private bool kenaDorongKanan;

    private Animator anim;

    private bool isTeleporting = false;
    
    public bool isCutscene = false; 
    public bool isGroundedForCutscene = false;

    [Header("Cutscene Status")]
    public bool isLocked = false; 

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCol = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();

        string currentLevel = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString("LastScene", currentLevel);
        PlayerPrefs.Save();
    }

    // Dipanggil otomatis saat script dimatikan oleh CutsceneManager
    void OnDisable()
    {
        if (footstepParticles != null)
        {
            var emFootstep = footstepParticles.emission;
            emFootstep.enabled = false;
            footstepParticles.Clear();
        }

        if (pushWindParticles != null)
        {
            var emWind = pushWindParticles.emission;
            emWind.enabled = false;
            pushWindParticles.Clear();
        }
    }

    public void SetTeleportLock(bool state)
    {
        isTeleporting = state;
        if (state)
        {
            rb.linearVelocity = Vector2.zero;
            moveInput = 0;
            lastLockedInput = 0;
            
            if (footstepParticles != null) footstepParticles.Clear();
            if (pushWindParticles != null) pushWindParticles.Clear();
        }
    }

   public void SetCutsceneLock(bool state)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (anim == null) anim = GetComponent<Animator>();

        isLocked = state; 
        isCutscene = state; 
        
        if (state)
        {
            if (footstepParticles != null)
            {
                var em = footstepParticles.emission;
                em.enabled = false;
                footstepParticles.Clear();
            }
            if (pushWindParticles != null)
            {
                var em = pushWindParticles.emission;
                em.enabled = false;
                pushWindParticles.Clear();
            }
        }
        
        if (rb != null) 
        {
            if (state) 
            {
                rb.linearVelocity = Vector2.zero;
                rb.simulated = false; 
            }
            else
            {
                rb.simulated = true;  
            }
        }
        
        if (anim != null)
        {
            anim.SetFloat("Speed", 0); 
        }
    }

    void Update()
    {
        HandleParticleEmission();

        if (isLocked) return; 
        
        if (GameManager.instance != null && GameManager.instance.isGameOver)
            return;

        if (isTeleporting) return;

        if (inputBufferTimer > 0)
        {
            inputBufferTimer -= Time.deltaTime;
            moveInput = lastLockedInput;
        }
        else
        {
            moveInput = 0;

            if (Input.GetKey(KeyCode.A))
            {
                moveInput = -1;
                if (lastLockedInput != -1)
                {
                    inputBufferTimer = animationInputDelay;
                    lastLockedInput = -1;
                }
            }
            else if (Input.GetKey(KeyCode.D))
            {
                moveInput = 1;
                if (lastLockedInput != 1)
                {
                    inputBufferTimer = animationInputDelay;
                    lastLockedInput = 1;
                }
            }
            else
            {
                lastLockedInput = 0;
            }
        }

        if (isGrounded && moveInput != 0 && !isDorong)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0)
            {
                PlaySFX("Player Move");
                footstepTimer = footstepInterval;
            }
        }
        else
        {
            footstepTimer = 0;
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            jumpBufferCounter = jumpBufferTime;
        }

        jumpBufferCounter -= Time.deltaTime;

        UpdateAnimation();
    }

    void HandleParticleEmission()
    {
        // 1. LOGIKA PARTIKEL PIJAKAN KAKI
        if (footstepParticles != null)
        {
            if (!footstepParticles.isPlaying) footstepParticles.Play();

            var footstepEmission = footstepParticles.emission;
            
            if (isLocked || isTeleporting || isCutscene)
            {
                footstepEmission.enabled = false;
            }
            else
            {
                // Matikan debu kaki saat sedang mendorong agar visual tidak berantakan
                footstepEmission.enabled = isGrounded && !isDorong;
            }
        }

        // 2. LOGIKA PARTIKEL ANGIN DORONG
        if (pushWindParticles != null)
        {
            if (!pushWindParticles.isPlaying) pushWindParticles.Play();

            var windEmission = pushWindParticles.emission;

            if (isGrounded && !isLocked && !isTeleporting && !isCutscene && isDorong && moveInput != 0)
            {
                windEmission.enabled = true;

                if (moveInput > 0) 
                {
                    pushWindParticles.transform.localRotation = Quaternion.Euler(0, 0, 0);
                    pushWindParticles.transform.localPosition = new Vector3(0.5f, 0, 0); 
                }
                else if (moveInput < 0) 
                {
                    pushWindParticles.transform.localRotation = Quaternion.Euler(0, 180, 0);
                    pushWindParticles.transform.localPosition = new Vector3(-0.5f, 0, 0); 
                }
            }
            else
            {
                windEmission.enabled = false;
            }
        }
    }

    void UpdateAnimation()
    {
        if (anim == null) return;

        anim.SetFloat("Speed", Mathf.Abs(moveInput));

        // Partikel kaki tidak di-flip manual lagi. Pastikan bentuk partikel kaki sudah diubah ke 'Box' di Editor
        if (moveInput > 0)
        {
            anim.SetBool("FacingRight", true);
        }
        else if (moveInput < 0)
        {
            anim.SetBool("FacingRight", false);
        }
    }

    void FixedUpdate()
    {
        if (GameManager.instance != null && GameManager.instance.isGameOver)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        CheckGrounded();

        if (isTeleporting && isGrounded)
        {
            isTeleporting = false;
        }

        if (isTeleporting)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        CheckDorong();

        if (isDorong && moveInput != 0)
        {
            pushSFXTimer -= Time.fixedDeltaTime;
            if (pushSFXTimer <= 0)
            {
                PlaySFX("Push");
                pushSFXTimer = pushSFXInterval;
            }
        }
        else
        {
            pushSFXTimer = 0;
        }

        float currentSpeed = isDorong ? slowmoDorong : speed;
        float finalMove = moveInput * currentSpeed;

        rb.linearVelocity = new Vector2(finalMove, rb.linearVelocity.y);

        if (kenaDorongKiri && rb.linearVelocity.x < 0)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
        if (kenaDorongKanan && rb.linearVelocity.x > 0)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }

        if (jumpBufferCounter > 0 && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferCounter = 0;
            
            // Matikan semua partikel langsung saat melompat
            isGrounded = false; 
            
            if (footstepParticles != null) 
            {
                var emFootstep = footstepParticles.emission;
                emFootstep.enabled = false; 
            }
            
            if (pushWindParticles != null) 
            {
                var emWind = pushWindParticles.emission;
                emWind.enabled = false;
            }

            PlaySFX("Player Jump");
        }
    }

    private void PlaySFX(string sfxName)
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX(sfxName);
        }
    }

    void CheckGrounded()
    {
        Vector2 boxSize = new Vector2(0.8f, 0.1f);
        Vector2 castOrigin = (Vector2)transform.position + (Vector2.down * 0.45f);

        RaycastHit2D hit = Physics2D.BoxCast(
            castOrigin,
            boxSize,
            0f,
            Vector2.down,
            0.1f,
            groundLayer | pushLayer | platformLayer
        );

        isGrounded = hit.collider != null;
    }

    void CheckDorong()
    {
        isDorong = false;

        if (!isGrounded || moveInput == 0)
            return;

        Vector2 direction = new Vector2(moveInput, 0);
        Vector2 origin = (Vector2)transform.position + (Vector2.down * 0.2f);

        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            direction,
            jarakDorong,
            pushLayer
        );

        if (hit.collider != null && hit.collider.CompareTag("Obstacle"))
        {
            isDorong = true;
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Target"))
        {
            targetCol = collision.collider;
            kenaDorongKiri = false;
            kenaDorongKanan = false;
            nahanPlatform = false;

            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.x > 0.5f)
                {
                    kenaDorongKiri = true;
                    nahanPlatform = true;
                }
                else if (contact.normal.x < -0.5f)
                {
                    kenaDorongKanan = true;
                    nahanPlatform = true;
                }
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Target"))
        {
            nahanPlatform = false;
            kenaDorongKiri = false;
            kenaDorongKanan = false;
        }
    }

    void OnCollisonEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGroundedForCutscene = true;
        }
    }
}