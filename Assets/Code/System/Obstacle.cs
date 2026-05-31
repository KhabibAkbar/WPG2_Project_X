using UnityEngine;

public class PushableObstacle : MonoBehaviour
{
    [Header("Visual Effects")]
    [Tooltip("Masukkan SEMUA partikel yang ada di sisi KIRI kotak (menyembur ke kanan)")]
    public ParticleSystem[] leftWindParticles; // Perhatikan tanda [] ini artinya Array (banyak)
    
    [Tooltip("Masukkan SEMUA partikel yang ada di sisi KANAN kotak (menyembur ke kiri)")]
    public ParticleSystem[] rightWindParticles;

    private Rigidbody2D rb;
    
    [Header("Pengaturan Deteksi")]
    [Tooltip("Kecepatan minimal kotak dianggap sedang terdorong")]
    public float movementThreshold = 0.1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Matikan semua partikel saat mulai
        SetParticleGroupEmission(leftWindParticles, false);
        SetParticleGroupEmission(rightWindParticles, false);
    }

    void Update()
    {
        if (rb == null) return;

        float velocityX = rb.linearVelocity.x;

        if (velocityX > movementThreshold)
        {
            // Bergerak ke KANAN (didorong dari KIRI)
            SetParticleGroupEmission(leftWindParticles, true);
            SetParticleGroupEmission(rightWindParticles, false);
        }
        else if (velocityX < -movementThreshold)
        {
            // Bergerak ke KIRI (didorong dari KANAN)
            SetParticleGroupEmission(leftWindParticles, false);
            SetParticleGroupEmission(rightWindParticles, true);
        }
        else
        {
            // Diam
            SetParticleGroupEmission(leftWindParticles, false);
            SetParticleGroupEmission(rightWindParticles, false);
        }
    }

    // Fungsi bantuan untuk menyalakan/mematikan satu grup (Array) partikel sekaligus
    private void SetParticleGroupEmission(ParticleSystem[] particleGroup, bool state)
    {
        // Cegah error jika array kosong
        if (particleGroup == null || particleGroup.Length == 0) return;

        // Looping: Lakukan perintah ini ke setiap partikel yang ada di dalam grup
        foreach (ParticleSystem ps in particleGroup)
        {
            if (ps != null)
            {
                var emission = ps.emission;
                
                if (emission.enabled != state)
                {
                    emission.enabled = state;
                }

                if (state && !ps.isPlaying)
                {
                    ps.Play();
                }
            }
        }
    }
}