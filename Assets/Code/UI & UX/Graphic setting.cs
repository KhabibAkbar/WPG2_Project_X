using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Graphicsetting : MonoBehaviour
{
    public TMP_Dropdown graphicdropdown;
    int pilihkualitas;

    // Gunakan OnEnable agar setiap kali panel muncul, data langsung sinkron
    void OnEnable()
    {
        // 1. Bersihkan dan isi ulang opsi (agar daftar kualitas selalu segar)
        graphicdropdown.ClearOptions();
        graphicdropdown.AddOptions(new List<string>(QualitySettings.names));

        // 2. Ambil data tersimpan, default-nya ambil dari settingan sistem saat ini
        pilihkualitas = PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel());

        // 3. Update tampilan Dropdown
        graphicdropdown.value = pilihkualitas;
        graphicdropdown.RefreshShownValue();

        // 4. Pastikan settingan diterapkan ke game
        applygraphic();
        
        // 5. Tambahkan Listener secara dinamis (opsional tapi lebih aman)
        graphicdropdown.onValueChanged.RemoveAllListeners();
        graphicdropdown.onValueChanged.AddListener(OnGraphicChanged);
    }

    public void OnGraphicChanged(int index)
    {
        pilihkualitas = index;
        applygraphic();
    }

    public void applygraphic()
{
    QualitySettings.SetQualityLevel(pilihkualitas, true);

    if (pilihkualitas == 0) // Level LOW
    {
        Application.targetFrameRate = 30;
        // Di sini kamu bisa mematikan efek-efek berat lainnya
    }
    else // Level HIGH/ULTRA
    {
        Application.targetFrameRate = -1; // Tidak dibatasi
    }

    PlayerPrefs.SetInt("QualityLevel", pilihkualitas);
    PlayerPrefs.Save();
}
}