using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public bool isGameOver;

    public GameObject panelGameOver;

    void Awake()
    {
        instance = this;
    }

    public void GameOver()
    {
        Debug.Log("GAME OVER");
        isGameOver = true;
        Time.timeScale = 0f;

        if (panelGameOver != null)
            panelGameOver.SetActive(true);
    }
}