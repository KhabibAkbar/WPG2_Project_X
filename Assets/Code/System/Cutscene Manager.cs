using UnityEngine;
using System.Collections;

public class CutsceneManager : MonoBehaviour
{
    public Camera cam;

    public Transform player1;
    public Transform player2; 

    public Transform middlePoint;

    public MovementP1 movementP1;
    public MovementP2 movementP2;

    public DialogueManager dialogue;

    [Header("Character Sprite")]
    public Sprite spriteP1;
    public Sprite spriteP2;

    [Header("Dialogue Text (Isi di Inspector)")]
    [TextArea] public string dialogP1;
    [TextArea] public string dialogP2;

    [Header("Blur Effect")]
    public GameObject blurEffect;

    Vector3 originalCamPos;

    void Start()
    {
        originalCamPos = cam.transform.position;

        if (middlePoint != null)
            cam.transform.position = new Vector3(middlePoint.position.x, middlePoint.position.y, -10);

        StartCoroutine(StartCutscene());
    }

    IEnumerator StartCutscene()
    {
        if (movementP1 != null)
            movementP1.enabled = false;

        if (movementP2 != null)
            movementP2.enabled = false;

        yield return null;

        if (blurEffect != null)
            blurEffect.SetActive(true);

        dialogue.ShowDialogue(dialogP1, spriteP1, true);
        yield return new WaitUntil(() => dialogue.selesai);

        yield return new WaitForSeconds(0.5f);

        if (player2 != null && movementP2 != null && spriteP2 != null)
        {
            dialogue.ShowDialogue(dialogP2, spriteP2, false);
            yield return new WaitUntil(() => dialogue.selesai);

            yield return new WaitForSeconds(0.5f);
        }

        if (blurEffect != null)
            blurEffect.SetActive(false);

        cam.transform.position = originalCamPos;

        if (movementP1 != null)
            movementP1.enabled = true;

        if (movementP2 != null)
            movementP2.enabled = true;
    }
}