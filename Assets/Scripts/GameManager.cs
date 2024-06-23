using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager gameManager;
    // Start is called before the first frame update
    void Start()
    {
        if(gameManager != null)
        {
            Destroy(this);
            return;
        }
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
        SceneManager.LoadScene("ControlsScreen");

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
