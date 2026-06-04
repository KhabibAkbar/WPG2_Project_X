using UnityEngine;

public class MovementP2 : MonoBehaviour
{
    [Header("Input Settings (Controller/Keyboard)")]
    [Tooltip("Ketik 'Horizontal_P2' untuk Player 2")]
    public string horizontalAxis = "Horizontal_P2"; 
    
    [Tooltip("Ketik 'Jump_P2' untuk Player 2")]
    public string jumpButton = "Jump_P2";

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

    public bool isGroundedForCutscene = false;

    [Header("Cutscene Status")]
    public bool isLocked = false; 

    [Header("Visual Effects")]
    public ParticleSystem footstepParticles; 

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCol = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();

        // Mempertahankan gravitasi terbalik
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
        }
    }

    public void SetCutsceneLock(bool state)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (anim == null) anim = GetComponent<Animator>();

        isLocked = state; 
        
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
        if (isLocked) return; 
        if (GameManager.instance != null && GameManager.instance.isGameOver) return;
        if (isTeleporting || isCutscene) return;

        // Membaca input dari Controller atau Keyboard untuk Player 2
        float axisInput = Input.GetAxisRaw(horizontalAxis);

        if (inputBufferTimer > 0)
        {
            inputBufferTimer -= Time.deltaTime;
            moveInput = lastLockedInput;
        }
        else
        {
            moveInput = 0;

            if (axisInput < -0.1f)
            {
                moveInput = -1;
                if (lastLockedInput != -1)
                {
                    inputBufferTimer = animationInputDelay;
                    lastLockedInput = -1;
                }
            }
            else if (axisInput > 0.1f)
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
                PlaySFX("Player Move 2");
                footstepTimer = footstepInterval;
            }
        }
        else
        {
            footstepTimer = 0;
        }

        // Loncat menggunakan input yang didaftarkan di Input Manager
        if (Input.GetButtonDown(jumpButton))
            jumpBufferCounter = jumpBufferTime;

        jumpBufferCounter -= Time.deltaTime;

        UpdateAnimation();
        HandleParticleEmission(); 
    }

    void UpdateAnimation()
    {
        if (anim == null) return;

        anim.SetFloat("Speed", Mathf.Abs(moveInput));

        if (moveInput > 0)
            anim.SetBool("FacingRight", true);
        else if (moveInput < 0)
            anim.SetBool("FacingRight", false);
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

        // Loncat ke arah bawah (-jumpForce) karena gravitasi terbalik
        if (jumpBufferCounter > 0 && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -jumpForce);
            jumpBufferCounter = 0;
            PlaySFX("Jump 2");
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
        Vector2 castOrigin = (Vector2)transform.position + (Vector2.up * 0.45f);

        RaycastHit2D hit = Physics2D.BoxCast(
            castOrigin,
            boxSize,
            0f,
            Vector2.up,
            0.1f,
            groundLayer | pushLayer | platformLayer
        );

        isGrounded = hit.collider != null;

        Color debugColor = isGrounded ? Color.green : Color.red;
        Debug.DrawRay(castOrigin + new Vector2(-boxSize.x / 2, 0), Vector2.right * boxSize.x, debugColor);
        Debug.DrawRay(castOrigin + new Vector2(-boxSize.x / 2, 0.1f), Vector2.right * boxSize.x, debugColor);
    }

    void CheckDorong()
    {
        isDorong = false;

        if (!isGrounded || moveInput == 0)
            return;

        Vector2 direction = new Vector2(moveInput, 0);
        Vector2 origin = (Vector2)transform.position + (Vector2.up * 0.2f);

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

    void OnCollisonEnter2D(Collision2D collision) // Sudah diperbaiki dari typo OnCollisonEnter2D
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGroundedForCutscene = true;
        }
    }

    void HandleParticleEmission()
    {
        if (footstepParticles == null) return;

        var emission = footstepParticles.emission;

        if (isGrounded && moveInput != 0 && !isDorong && !isLocked && !isTeleporting && !isCutscene)
        {
            emission.enabled = true;
            if (!footstepParticles.isPlaying)
            {
                footstepParticles.Play();
            }
        }
        else
        {
            emission.enabled = false;
        }
    }

    void OnDisable()
    {
        if (footstepParticles != null)
        {
            var emission = footstepParticles.emission;
            emission.enabled = false;
        }
    }
}