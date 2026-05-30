using UnityEngine;
using System.Collections.Generic;

public class InjectTriggerSingle : MonoBehaviour
{
    public enum MoveDirection { Down, Up }

    public Transform platform;
    public MoveDirection direction;

    public float distance = 2f;
    public float speed = 3f;

    public LayerMask playerLayer;
    public float cekJarak = 0.7f;

    private Vector2 posisiAwal;
    private Vector2 posisiTarget;

    private HashSet<GameObject> objectsInside = new HashSet<GameObject>();
    private Rigidbody2D rb;

    private bool isInitialized = false;

    // 🔥 STATE AUDIO
    private bool isMoving = false;
    private bool isSafeToPlay = false;
    float spawnTime;

    void Start()
    {
        spawnTime = Time.time;

        rb = platform.GetComponent<Rigidbody2D>();

        // 🔥 TAMBAHAN: pastikan tidak ada velocity sisa
        rb.linearVelocity = Vector2.zero;

        posisiAwal = platform.position;

        posisiTarget = (direction == MoveDirection.Down)
            ? posisiAwal + Vector2.down * distance
            : posisiAwal + Vector2.up * distance;

        objectsInside.Clear();

        Invoke(nameof(EnableTrigger), 0.7f);
    }

    void EnableTrigger()
    {
        isInitialized = true;
        isSafeToPlay = true;
    }

    void FixedUpdate()
    {
        if (!isInitialized) return;
        if (IsEarlySpawn()) return;

        objectsInside.RemoveWhere(obj => obj == null);

        float step = speed * Time.fixedDeltaTime;
        Vector2 arahGerak = (direction == MoveDirection.Down) ? Vector2.down : Vector2.up;

        BoxCollider2D col = platform.GetComponent<BoxCollider2D>();

        RaycastHit2D hit = Physics2D.BoxCast(
            col.bounds.center,
            col.bounds.size,
            0f,
            arahGerak,
            0.05f,
            playerLayer
        );

        if (hit.collider != null)
        {
            rb.linearVelocity = Vector2.zero;
            StopPlatformSFX();
            return;
        }

        bool playerDiBawah = Physics2D.Raycast(
            platform.position,
            arahGerak,
            cekJarak,
            playerLayer
        );

        Vector2 destination;

        if (playerDiBawah)
            destination = posisiAwal;
        else
            destination = (objectsInside.Count > 0) ? posisiTarget : posisiAwal;

        Vector2 newPos = Vector2.MoveTowards(rb.position, destination, step);
        Vector2 velocity = (newPos - rb.position) / Time.fixedDeltaTime;

        rb.MovePosition(newPos);

        bool currentlyMoving = velocity.magnitude > 0.005f;

        // 🔥 FIX UTAMA DI SINI
        if (currentlyMoving && !isMoving && isSafeToPlay && objectsInside.Count > 0)
        {
            if (AudioManager.instance != null)
                AudioManager.instance.PlayLoopingSFX("Platform1");

            isMoving = true;
        }
        else if (!currentlyMoving && isMoving)
        {
            StopPlatformSFX();
        }
    }

    void StopPlatformSFX()
    {
        if (AudioManager.instance != null)
            AudioManager.instance.StopLoopingSFX();

        isMoving = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isInitialized) return;
        if (IsEarlySpawn()) return;

        if (other.CompareTag("Player") || other.CompareTag("Obstacle"))
        {
            if (objectsInside.Count == 0 && isSafeToPlay)
            {
                if (AudioManager.instance != null)
                    AudioManager.instance.PlaySFX("Inject");
            }

            objectsInside.Add(other.gameObject);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!isInitialized) return;
        if (IsEarlySpawn()) return;

        if (other.CompareTag("Player") || other.CompareTag("Obstacle"))
        {
            objectsInside.Remove(other.gameObject);

            if (objectsInside.Count == 0 && isSafeToPlay)
            {
                if (AudioManager.instance != null)
                    AudioManager.instance.PlaySFX("Inject");
            }
        }
    }

    private void OnDisable()
    {
        PrepareForSceneTransition();
    }

    public void PrepareForSceneTransition()
    {
        isSafeToPlay = false;
        isInitialized = false;
        StopPlatformSFX();
        if (objectsInside != null) objectsInside.Clear();
    }

    bool IsEarlySpawn()
    {
        return Time.time - spawnTime < 0.4f;
    }
}