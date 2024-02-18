using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting.Antlr3.Runtime.Tree;

public class GameManager : MonoBehaviour
{
    [Header("Reference")]
    [Tooltip("プレイヤー")]
    [SerializeField] PlayerController player;
    [Tooltip("カメラ")]
    [SerializeField] CameraController playerCamera;
    [Tooltip("ゲーム開始時のテキスト")]
    [SerializeField] Text startText;
    [Tooltip("タイマーテキスト")]
    [SerializeField] Text timerText;
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

    float elapsedTime;
    public float time { get { return elapsedTime; } }

    bool isPlayingGame;

    // Start is called before the first frame update
    void Start()
    {
        garbages = GameObject.FindGameObjectsWithTag("Garbage");
        Debug.Log("Find " + garbages.Length + " garbages");

        progressPerGarbage = 100.0f / garbages.Length;
        progressSlider.maxValue = 100.0f;

        StartCoroutine(StartGame());
    }

    // Update is called once per frame
    void Update()
    {
        if (isPlayingGame)
        {
            elapsedTime += Time.deltaTime;

            timerText.text = (Mathf.Floor(elapsedTime / 60)).ToString("00") + ":" + ((int)elapsedTime % 60).ToString("00");
        }
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

    IEnumerator StartGame()
    {
        startText.text = "Ready...";

        yield return new WaitForSeconds(1.0f);

        startText.text = "GO!";

        player.enabled = true;
        playerCamera.enabled = true;
        isPlayingGame = true;

        yield return new WaitForSeconds(1.0f);

        startText.text = "";
    }

    IEnumerator GameSet(bool isClear)
    {
        player.enabled = false;
        playerCamera.enabled = false;
        isPlayingGame = false;

        string targetScene;

        if (isClear)
        {
            clearUI.SetActive(true);
            targetScene = "GameClear";

            SaveSystem.SaveScore(this);
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
