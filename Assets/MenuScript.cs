using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NewBehaviourScript : MonoBehaviour
{

    public GameObject popupPanel;

    void Start()
    {
        // Oyun başladığında popup kapalı olmalı
        popupPanel.SetActive(false);
        Cursor.visible = true;
    }

    public void ShowPopup()
    {
        popupPanel.SetActive(true);
    }

    public void ClosePopup()
    {
        popupPanel.SetActive(false);
    }
    
    public void PlayButton()
    {
        SceneManager.LoadScene(1);
    }

    public void ControlsButton()
    {

    }

    public void QuitButton()
    {
        Application.Quit();
    }
}
