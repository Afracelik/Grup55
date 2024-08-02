using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseControl : MonoBehaviour
{
    public static bool gameIsPaused = false;
    public GameObject panel; 

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(gameIsPaused){
                Resume();
            }else{
                Pause();
            }
        }
    }

    public void Pause()
    {
        panel.SetActive(true);
        gameIsPaused = true;
        Time.timeScale = 0f; // Oyun zamanını durdur
    }

    public void Resume()
    {
        panel.SetActive(false);
        gameIsPaused = false;
        Time.timeScale = 1f;
    }
}