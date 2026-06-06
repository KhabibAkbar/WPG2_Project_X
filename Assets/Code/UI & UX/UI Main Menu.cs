using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.EventSystems; 
using UnityEngine.UI; // [BARU] Wajib ditambahkan untuk memanipulasi komponen UI Image

public class UIMainMenu : MonoBehaviour
{
    [Header("UI Controller Setup")]
    [Tooltip("Masukkan tombol PLAY ke sini agar langsung tersorot saat game mulai")]
    public GameObject firstSelectedButton; 

    [Header("Controller Indicator Setup")]
    [Tooltip("Masukkan komponen Image UI yang akan berubah warna ketika kontroller aktif")]
    public Image controllerIndicatorImage;
    public Color connectedColor = Color.green;      // Warna saat stick terdeteksi (Hijau)
    public Color disconnectedColor = Color.gray;    // Warna saat tidak ada stick (Abu-abu)

    [Header("Zoom Transisi Setup")]
    public RectTransform mainUIBackground; 
    public float zoomMultiplier = 1.3f;
    public float animationDuration = 1.2f; 
    public GameObject menuButtonsContainer;

    private bool lastControllerStatus = false;
    private float checkTimer = 0f;

    void Start()
    {
        if (firstSelectedButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null); 
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        }

        // Cek status pertama kali saat scene dimuat
        CheckControllerConnection(true);
    }

    void Update()
    {
        // Mengoptimalkan performa: Cek koneksi setiap 1 detik saja, tidak setiap frame
        checkTimer += Time.deltaTime;
        if (checkTimer >= 1f)
        {
            checkTimer = 0f;
            CheckControllerConnection(false);
        }
    }

    private void CheckControllerConnection(bool forceUpdate)
    {
        bool isConnected = false;
        string[] joysticks = Input.GetJoystickNames();
        
        foreach (string joy in joysticks)
        {
            // Jika nama joystick ditemukan dan tidak kosong, berarti ada controller aktif
            if (!string.IsNullOrEmpty(joy)) 
            {
                isConnected = true;
                break;
            }
        }

        // Hanya ubah warna UI jika ada perubahan status atau dipaksa saat Start
        if (isConnected != lastControllerStatus || forceUpdate)
        {
            lastControllerStatus = isConnected;
            if (controllerIndicatorImage != null)
            {
                controllerIndicatorImage.color = isConnected ? connectedColor : disconnectedColor;
            }
        }
    }

    public void Play_Game()
    {
        StartCoroutine(PlayZoomEffectAndLoad());
        Debug.Log("Play Game - Memulai Efek Zoom Layar Utama");
    }

    private IEnumerator PlayZoomEffectAndLoad()
    {
        if (menuButtonsContainer != null) menuButtonsContainer.SetActive(false);

        if (mainUIBackground != null)
        {
            Vector3 initialScale = mainUIBackground.localScale;
            Vector3 targetScale = initialScale * zoomMultiplier;
            float elapsedTime = 0f;

            while (elapsedTime < animationDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / animationDuration;
                float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);
                mainUIBackground.localScale = Vector3.Lerp(initialScale, targetScale, smoothProgress);
                yield return null; 
            }
        }
        else
        {
            yield return new WaitForSeconds(animationDuration);
        }

        FadeManager.instance.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void Exit() { Application.Quit(); }
    public void Back_To_Menu() { SceneManager.LoadScene("Main Menu"); }
    public void Pilih(string namaSceneTujuan) { SceneManager.LoadScene(namaSceneTujuan); }
    public void Back()
    {
        int previousSceneIndex = SceneManager.GetActiveScene().buildIndex - 1;
        SceneManager.LoadScene(previousSceneIndex);
    }
}