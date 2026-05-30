using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class TeleportPair
{
    public string id;
    public Transform target;
}

public class Teleport : MonoBehaviour
{
    public List<TeleportPair> targets = new List<TeleportPair>();
    private Dictionary<string, Transform> targetDict;
    private bool canTeleport = true;

    [Header("SFX Configuration")]
    [Tooltip("Nama file audio yang ada di list SFX Clips AudioManager")]
    public string teleportSFXName = "Teleport";

    void Awake()
    {
        targetDict = new Dictionary<string, Transform>();
        foreach (var pair in targets)
        {
            if (pair.target != null && !targetDict.ContainsKey(pair.id))
            {
                targetDict.Add(pair.id, pair.target);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canTeleport) return;

        Teleportable tp = other.GetComponent<Teleportable>();
        if (tp == null) return;

        // Cek cooldown per-objek agar tidak teleport berulang kali secara instan
        if (Time.time - tp.lastTeleportTime < 0.2f) return;
        tp.lastTeleportTime = Time.time;

        foreach (string id in tp.teleportIDs)
        {
            if (targetDict.TryGetValue(id, out Transform target))
            {
                // Kunci pergerakan player jika ada
                MovementP1 p1 = other.GetComponent<MovementP1>();
                if (p1 != null) p1.SetTeleportLock(true);

                MovementP2 p2 = other.GetComponent<MovementP2>();
                if (p2 != null) p2.SetTeleportLock(true);

                StartCoroutine(TeleportSequence(other, target));
                return;
            }
        }
    }

    IEnumerator TeleportSequence(Collider2D other, Transform target)
    {
        canTeleport = false;
        Transform obj = other.transform;
        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();

        // 1. Matikan fisika jika objek adalah Obstacle
        if (rb != null && other.CompareTag("Obstacle"))
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // 2. Mainkan Sound melalui AudioManager
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX(teleportSFXName);
        }

        // 3. Pindahkan posisi
        obj.position = target.position;

        // 4. Tunggu cooldown agar tidak terjadi loop teleport antar portal
        yield return new WaitForSeconds(0.5f);

        // 5. Kembalikan fisika ke semula
        if (rb != null && other.CompareTag("Obstacle"))
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
        }

        canTeleport = true;
    }
}