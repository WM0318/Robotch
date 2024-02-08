using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Reference")]
    [Tooltip("�v���C���[")]
    [SerializeField] PlayerController player;
    [Tooltip("�J����")]
    [SerializeField] CameraController playerCamera;
    [Tooltip("�i�s�󋵃e�L�X�g")]
    [SerializeField] Text progressText;
    [Tooltip("�i�s�󋵃X���C�_�[")]
    [SerializeField] Slider progressSlider;
    [Tooltip("�Q�[���N���A���ɕ\������UI")]
    [SerializeField] GameObject clearUI;
    [Tooltip("�Q�[���I�[�o�[���ɕ\������UI")]
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
