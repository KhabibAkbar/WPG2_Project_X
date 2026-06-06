using UnityEngine;

public class MovementP2 : MonoBehaviour
{
    [Header("Input Settings (Controller/Keyboard)")]
    public string horizontalAxis = "Horizontal_P2"; 
    [Tooltip("Pastikan nama ini sama dengan slot Joystick di Input Manager!")]
    public string joyAxis = "Horizontal_P2_Joy";
    
    public string jumpButton = "Jump_P2";

    [Tooltip("Batas toleransi analog. Naikkan ke 0.25 atau 0.3 jika karakter jalan sendiri")]
    public float inputDeadzone = 0.25f; 

    public float speed = 5f;
    public float jumpForce = 8f;
    public float slowmoDorong = 2f;
    public float jarakDorong = 0.6f;

    [Header("Audio Settings")]
    public float footstepInterval = 0.4f;
    private float footstepTimer;

    public float pushSFXInterval = 0.5f;
    private float pushSFXTimer;

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
    private bool isCutscene = false;

    [Header("Cutscene Status")]
    public bool isLocked = false; 

    [Header("Visual Effects")]
    public ParticleSystem footstepParticles; 

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCol = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();

        // Mempertahankan gravitasi terbalik khusus Player 2
        rb.gravityScale = -Mathf.Abs(rb.gravityScale);

        string currentLevel = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString("LastScene", currentLevel);
        PlayerPrefs.Save();
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
        }
        
        if (rb != null) 
        {
            if (state) 
            {
                rb.linearVelocity = Vector2.zero;
                rb.simulated = false; 
            }
            else rb.simulated = true;  
        }
        
        if (anim != null) anim.SetFloat("Speed", 0); 
    }

    void Update()
    {
        HandleParticleEmission(); 

        if (isLocked) return; 
        if (GameManager.instance != null && GameManager.instance.isGameOver) return;
        if (isTeleporting || isCutscene) return;

        // --- SISTEM ANTI-GHOSTING INPUT P2 ---
        float axisInput = Input.GetAxisRaw(horizontalAxis);

        bool isP2ControllerConnected = false;
        string[] connectedJoys = Input.GetJoystickNames();
        
        // P2 secara spesifik hanya mengecek urutan kedua (index 1) di Windows daftar controller
        if (connectedJoys.Length > 1 && !string.IsNullOrEmpty(connectedJoys[1]))
        {
            isP2ControllerConnected = true;
        }

        if (isP2ControllerConnected)
        {
            float joyInput = Input.GetAxisRaw(joyAxis);
            if (Mathf.Abs(joyInput) > inputDeadzone)
            {
                axisInput = joyInput;
            }
        }
        // -------------------------------------

        if (inputBufferTimer > 0)
        {
            inputBufferTimer -= Time.deltaTime;
            moveInput = lastLockedInput;
        }
        else
        {
            moveInput = 0;

            if (axisInput < -inputDeadzone)
            {
                moveInput = -1;
                if (lastLockedInput != -1)
                {
                    inputBufferTimer = animationInputDelay;
                    lastLockedInput = -1;
                }
            }
            else if (axisInput > inputDeadzone)
            {
                moveInput = 1;
                if (lastLockedInput != 1)
                {
                    inputBufferTimer = animationInputDelay;
                    lastLockedInput = 1;
                }
            }
            else lastLockedInput = 0;
        }

        if (isGrounded && moveInput != 0 && !isDorong)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0)
            {
                PlaySFX("Player Move 2");
                footstepTimer = footstepInterval;
            }
        }
        else footstepTimer = 0;

        if (Input.GetButtonDown(jumpButton)) jumpBufferCounter = jumpBufferTime;
        jumpBufferCounter -= Time.deltaTime;

        UpdateAnimation();
    }

    void UpdateAnimation()
    {
        if (anim == null) return;
        anim.SetFloat("Speed", Mathf.Abs(moveInput));
        if (moveInput > 0) anim.SetBool("FacingRight", true);
        else if (moveInput < 0) anim.SetBool("FacingRight", false);
    }

    void FixedUpdate()
    {
        if (GameManager.instance != null && GameManager.instance.isGameOver)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        CheckGrounded();

        if (isTeleporting && isGrounded) isTeleporting = false;
        if (isTeleporting || isCutscene)
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
        else pushSFXTimer = 0;

        float currentSpeed = isDorong ? slowmoDorong : speed;
        float finalMove = moveInput * currentSpeed;

        rb.linearVelocity = new Vector2(finalMove, rb.linearVelocity.y);

        if (kenaDorongKiri && rb.linearVelocity.x < 0) rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        if (kenaDorongKanan && rb.linearVelocity.x > 0) rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        if (jumpBufferCounter > 0 && isGrounded)
        {
            // Loncat ke bawah (negatif) karena gravitasi P2 berada di atap
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -jumpForce);
            jumpBufferCounter = 0;
            
            isGrounded = false;
            if (footstepParticles != null) 
            {
                var emFootstep = footstepParticles.emission;
                emFootstep.enabled = false; 
            }

            PlaySFX("Jump 2");
        }
    }

    private void PlaySFX(string sfxName)
    {
        if (AudioManager.instance != null) AudioManager.instance.PlaySFX(sfxName);
    }

    void CheckGrounded()
    {
        Vector2 boxSize = new Vector2(0.8f, 0.1f);
        // Cast ke arah atas (Vector2.up) karena posisi tanah P2 berada di langit-langit
        Vector2 castOrigin = (Vector2)transform.position + (Vector2.up * 0.45f);
        RaycastHit2D hit = Physics2D.BoxCast(castOrigin, boxSize, 0f, Vector2.up, 0.1f, groundLayer | pushLayer | platformLayer);
        isGrounded = hit.collider != null;
    }

    void CheckDorong()
    {
        isDorong = false;
        if (!isGrounded || moveInput == 0) return;
        Vector2 direction = new Vector2(moveInput, 0);
        Vector2 origin = (Vector2)transform.position + (Vector2.up * 0.2f);
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, jarakDorong, pushLayer);
        if (hit.collider != null && hit.collider.CompareTag("Obstacle")) isDorong = true;
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
                if (contact.normal.x > 0.5f) { kenaDorongKiri = true; nahanPlatform = true; }
                else if (contact.normal.x < -0.5f) { kenaDorongKanan = true; nahanPlatform = true; }
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

    // Fungsi OnCollisionEnter2D dihapus sepenuhnya untuk membuang System Land yang mengacaukan Level 1 & 2

    void HandleParticleEmission()
    {
        if (footstepParticles == null) return;
        var emission = footstepParticles.emission;
        if (isGrounded && moveInput != 0 && !isDorong && !isLocked && !isTeleporting && !isCutscene)
        {
            emission.enabled = true;
            if (!footstepParticles.isPlaying) footstepParticles.Play();
        }
        else emission.enabled = false;
    }

    void OnDisable()
    {
        if (footstepParticles != null)
        {
            var emission = footstepParticles.emission;
            emission.enabled = false;
            footstepParticles.Clear();
        }
    }
}