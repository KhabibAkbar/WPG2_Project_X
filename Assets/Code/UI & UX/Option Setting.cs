using UnityEngine;

public class OptionSetting : MonoBehaviour
{
    //Option setting lumayan ribet karea memerlukan banya script untuk mengatur settingan yang ada.
    public GameObject Resolution;//ini kalau mau pakai resollution di setting dan harus edit juga ya di project manager biar bisa tampil
    public GameObject Graphic;//ini uga sama dengan resolution harus di atyr duku di project manager


    //ini untuk atur audio manager ini juga lumayan ribet tapi gak perlu ke project manager (biasanya isinya cuma bgm dan sound effect untuk mengatur volume)
    public GameObject BGM;
    public GameObject SFX;

    //selesai

    private void Start()
    {
        Tampilkan_Resolustion();//ini untuk menampilkan apa yang ingin di tampilkan pertama kali ketika masuk ke menu setting (ini bisa di atur sesuai keinginan ya)
    }

    public void Tampilkan_Resolustion()//ini untuk menampilkan resolustion ketika masuk ke menu setting
    {
        //ini yang tapil yang true saja yah
        Resolution.SetActive(true);
        Graphic.SetActive(true);
        //ini flase karena agar menu yang lain tidak tampil ketika masuk ke menu setting (ini bisa di atur sesuai keinginan ya)
        BGM.SetActive(false);
        SFX.SetActive(false);
    }

    public void Tampilkan_Auidio()//ini untuk aduio (kenapa bikin 2? agar tidak tabrakan dengan menu resolution)
    {
        //ini yang tapil yang true saja yah
        BGM.SetActive(true);
        SFX.SetActive(true);
        //ini flase karena agar menu yang lain tidak tampil ketika masuk ke menu setting (ini bisa di atur sesuai keinginan ya)
        Resolution.SetActive(false);
        Graphic.SetActive(false);
    }


}

