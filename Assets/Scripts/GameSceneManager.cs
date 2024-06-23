using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameSceneManager : MonoBehaviour
{

    public Image startScreen;
    public float startScreenSeconds;
    float startTime;

    List<TextMeshProUGUI> text;

    // Start is called before the first frame update
    void Start()
    {
        startScreen.gameObject.SetActive(true);
        StartCoroutine(closeStartScreen());
        startTime = Time.time;

        text = new List<TextMeshProUGUI>(startScreen.transform.GetComponentsInChildren<TextMeshProUGUI>());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    IEnumerator closeStartScreen()
    {
        while(Time.time - startTime < startScreenSeconds)
        {
            yield return null;
            Color c = startScreen.color;
            float a = 1 - (Time.time - startTime)/startScreenSeconds;
            c.a = a;
            startScreen.color = c;

            foreach (var t in text)
            {
                c = t.color;
                c.a = a;
                t.color = c;
            }

        }
        startScreen.gameObject.SetActive(false);
    }
}
