using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Reference")]
    [Tooltip("�i�s�󋵃e�L�X�g")]
    [SerializeField] Text progressText;
    [Tooltip("�i�s�󋵃X���C�_�[")]
    [SerializeField] Slider progressSlider;

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
        }
    }

    public void GameOver()
    {
        Debug.Log("Game Over");
    }
}
