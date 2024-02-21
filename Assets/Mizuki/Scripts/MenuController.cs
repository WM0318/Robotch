using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("GameOver")) StartCoroutine(Back());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator Back()
    {
        yield return new WaitForSeconds(10.0f);

        SceneManager.LoadScene("MainMenu");
    }

    public void PlayButton()
    {
        SceneManager.LoadScene("MainGame");
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
