using UnityEngine;

public class CameraMultipleTarget : MonoBehaviour
{
    [Header("Targets")]
    [Tooltip("Wajib diisi. Target utama (P1)")]
    public Transform player1;
    
    [Tooltip("Kosongkan slot ini di Inspector jika level hanya untuk 1 player")]
    public Transform player2;

    [Header("Camera Movement")]
    public Vector3 offset = new Vector3(0f, 0f, -10f); // Posisi Z kamera
    public float smoothTime = 0.3f; // Kehalusan gerak kamera
    private Vector3 velocity;

    [Header("Camera Zoom (Hanya aktif untuk 2 Player)")]
    public float minZoom = 5f; // Zoom standar untuk 1 player, atau zoom terdekat untuk 2 player
    public float maxZoom = 12f; // Zoom paling jauh (saat berjauhan)
    public float zoomLimiter = 15f; 
    
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        
        // Atur zoom awal agar transisi lebih mulus saat mulai
        cam.orthographicSize = minZoom;
    }

    void LateUpdate()
    {
        // Jika kedua player tidak dimasukkan sama sekali, hentikan fungsi
        if (player1 == null && player2 == null) return;

        MoveCamera();
        ZoomCamera();
    }

    void MoveCamera()
    {
        Vector3 targetPosition;

        // Cek apakah KEDUA player ada
        if (player1 != null && player2 != null)
        {
            // Logika 2 Player: Cari titik tengah
            Vector3 centerPoint = (player1.position + player2.position) / 2f;
            targetPosition = centerPoint + offset;
        }
        else
        {
            // Logika 1 Player: Cari mana player yang aktif (biasanya P1)
            Transform activePlayer = player1 != null ? player1 : player2;
            targetPosition = activePlayer.position + offset;
        }

        // Menerapkan pergerakan secara halus
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }

    void ZoomCamera()
    {
        float targetZoom;

        // Cek apakah KEDUA player ada
        if (player1 != null && player2 != null)
        {
            // Logika 2 Player: Zoom dinamis berdasarkan jarak P1 dan P2
            float distance = Vector3.Distance(player1.position, player2.position);
            targetZoom = Mathf.Lerp(minZoom, maxZoom, distance / zoomLimiter);
        }
        else
        {
            // Logika 1 Player: Zoom dikunci pada nilai minZoom
            targetZoom = minZoom;
        }

        // Menerapkan efek zoom secara halus
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, Time.deltaTime);
    }
}