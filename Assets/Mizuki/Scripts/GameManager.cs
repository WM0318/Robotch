using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Reference")]
    [Tooltip("プレイヤー")]
    [SerializeField] PlayerController player;
    [Tooltip("カメラ")]
    [SerializeField] CameraController playerCamera;
    [Tooltip("進行状況テキスト")]
    [SerializeField] Text progressText;
    [Tooltip("進行状況スライダー")]
    [SerializeField] Slider progressSlider;
    [Tooltip("ゲームクリア時に表示するUI")]
    [SerializeField] GameObject clearUI;
    [Tooltip("ゲームオーバー時に表示するUI")]
    [SerializeField] GameObject failureUI;

    GameObject[] garbages;

    int cleaningProgress;
    float progressPerGarbage;

    // Start is called before the first frame update
    void Start()
    {
        garbages = GameObject.FindGameObjectsWithTag("Garbage");
        Debug.Log("Find " + garbages.Length + " garbages");

        progressPerGarbage = 100.0f / garbages.Length;
        progressSlider.maxValue = 100.0f;
    }

    public void CheckProgress()
    {
        cleaningProgress += 1;

        progressText.text = " " + (progressPerGarbage * cleaningProgress).ToString("f1") + " %";
        progressSlider.value = progressPerGarbage * cleaningProgress;

        if (cleaningProgress >= garbages.Length)
        {
            Debug.Log("Clear!");

            StartCoroutine(GameSet(true));
        }
    }

    public void GameOver()
    {
        Debug.Log("Game Over");

        StartCoroutine(GameSet(false));
    }

    IEnumerator GameSet(bool isClear)
    {
        player.enabled = false;
        playerCamera.enabled = false;

        string targetScene;

        if (isClear)
        {
            clearUI.SetActive(true);
            targetScene = "GameClear";
        }
        else 
        {
            failureUI.SetActive(true);
            targetScene = "GameOver";
        }

        yield return new WaitForSeconds(3.0f);

        SceneManager.LoadScene(targetScene);
    }

}
