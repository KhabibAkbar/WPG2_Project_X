//Ini kalau kalian punya banyak scene yang butuh bgm yang beda - beda
//Karena di projectku sebelumnya ada custcene dan punya bgm yag beda jadi aku buat ini. Nah kode ini itu kalau kalian punya Menu Level/cutscene yang setiap evel dan custcene punya bgm yang beda beda bisa di tambahkan di sini.
//Sama jangan lupa setiap BGM dan scen namanya harus sama ya teman-teman
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class AudioScene : MonoBehaviour
{
    IEnumerator Start()
    {
        yield return null;

        if (AudioManager.instance == null)
        {
            Debug.LogError("AudioManager tidak ditemukan!");
            yield break;
        }

        string scene = SceneManager.GetActiveScene().name;

        if (scene == "Main Menu")//ini sesuaiin sama nama scene kalian
            AudioManager.instance.PlayBGM("Main Menu");//ini juga sesuain sama nama BGM yang ada di Audio Manager kalian
        else if (scene == "Cutscene")//ini sesuaiin sama nama scene kalian
            AudioManager.instance.PlayBGM("Cutscene");//ini juga sesuain sama nama BGM yang ada di Audio Manager kalian
        //di sini kalian bisa tambahin bgm lagi jika punya pakai else if
        else if (scene == "Level 2")//ini sesuaiin sama nama scene kalian
            AudioManager.instance.PlayBGM("Gameplay");
        else if (scene == "Level 1")//ini sesuaiin sama nama scene kalian
            AudioManager.instance.PlayBGM("Gameplay");
        else if (scene == "Level 3")//ini sesuaiin sama nama scene kalian
            AudioManager.instance.PlayBGM("Gameplay");
        else if (scene == "Level 4")//ini sesuaiin sama nama scene kalian
            AudioManager.instance.PlayBGM("Gameplay");
        else if (scene == "Level 5")//ini sesuaiin sama nama scene kalian
            AudioManager.instance.PlayBGM("Fighting");
    }
}