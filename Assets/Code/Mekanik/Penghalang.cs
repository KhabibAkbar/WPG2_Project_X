using UnityEngine;

public class Penghalang : MonoBehaviour
{
    public enum MoveDirection { Up, Down }

    public MoveDirection direction;
    public float distance = 3f;
    public float speed = 4f;

    [Header("Shake Settings")]
    public bool shakeOnActivate = true;
    public float shakeDuration = 0.3f;
    public float shakeMagnitude = 0.2f;

    public LayerMask playerLayer;

    private Vector2 startPos;
    private Vector2 targetPos;

    private Rigidbody2D rb;
    private bool active = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startPos = transform.position;

        targetPos = (direction == MoveDirection.Down)
            ? startPos + Vector2.down * distance
            : startPos + Vector2.up * distance;

        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    public void ActivatePlatform()
    {
        if (active) return; // Mencegah aktivasi ganda

        active = true;

        // --- TAMBAHAN CAMERA SHAKE ---
        if (shakeOnActivate && CameraShake.instance != null)
        {
            CameraShake.instance.Shake(shakeDuration, shakeMagnitude);
        }

        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX("Platform1");
        }
    }

    void FixedUpdate()
    {
        if (!active) return;

        Vector2 destination = targetPos;

        // Cek jika sudah sampai target (opsional: matikan active jika sudah sampai)
        if (Vector2.Distance(rb.position, destination) < 0.01f)
        {
            rb.position = destination;
            active = false;
            return;
        }

        if (AdaPlayerDiBawah())
            return;

        Vector2 next = Vector2.MoveTowards(rb.position, destination, speed * Time.fixedDeltaTime);
        rb.MovePosition(next);
    }

    bool AdaPlayerDiBawah()
    {
        Collider2D col = GetComponent<Collider2D>();
        RaycastHit2D hit = Physics2D.BoxCast(
            col.bounds.center,
            col.bounds.size,
            0f,
            Vector2.down,
            0.05f,
            playerLayer
        );
        return hit.collider != null;
    }
}