using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void playButton()
    {
        SceneManager.LoadScene("GameScene");
    }
    public void controlsButton()
    {
        SceneManager.LoadScene("ControlsScene");

    }
    public void creditsButton()
    {
        SceneManager.LoadScene("CreditsScene");

    }
    public void quitButton()
    {
        Application.Quit();
    }
    
}
