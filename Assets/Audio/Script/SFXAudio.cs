//ini kalau kalian punya banyak sfx , bisa di tambahin di list sfxClips terus di tambahin ke dictionary sfxDict dengan nama yang sesuai
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SFXAudio : MonoBehaviour,
IPointerDownHandler
{
    public string sfxName = "Button Click";//namanya harus sama dengan yang di panggil di PlaySFX di AudioManager
    //kalian kalau mau nambain sfx bisa di tambahin di list sfxClips terus di tambahin ke dictionary sfxDict (ini ada di audio manager ya) dengan nama yang sesuai
    public void OnPointerDown(PointerEventData eventData)
    {
        if (AudioManager.instance != null)
            AudioManager.instance.PlaySFX(sfxName);
    }
}

