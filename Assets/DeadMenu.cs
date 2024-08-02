using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeadMenu : MonoBehaviour
{
    void Start(){
        Cursor.visible = true;
    }
    
    public void LoadMainMenu()
    {
        SceneManager.LoadScene(0);
    }
}
