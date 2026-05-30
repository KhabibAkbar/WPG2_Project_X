using UnityEngine;
using System.Collections;

public class ShakeOnStart : MonoBehaviour
{
    public float duration = 1.0f;
    public float magnitude = 0.3f;
    public string sfxName = "Platform1";

    void Awake()
    {
        Debug.Log("<color=white>1. Awake: ShakeOnStart nempel di: " + gameObject.name + "</color>");
    }

    IEnumerator Start()
    {
        Debug.Log("<color=white>2. Start Coroutine Dimulai</color>");

        // Kita tunggu lebih lama (0.5 detik) supaya benar-benar lewat dari masa transisi
        yield return new WaitForSeconds(0.5f);

        if (AudioManager.instance != null)
        {
            Debug.Log("<color=white>3. Memanggil PlayLoopingSFXDirect untuk: " + sfxName + "</color>");
            AudioManager.instance.PlayLoopingSFXDirect(sfxName);
        }
        else
        {
            Debug.LogError("AudioManager instance tidak ditemukan!");
        }

        if (CameraShake.instance != null)
        {
            CameraShake.instance.Shake(duration, magnitude);
            Debug.Log("<color=white>4. Camera Shake Terpanggil</color>");
        }
    }
}