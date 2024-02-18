using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreCheck : MonoBehaviour
{
    [SerializeField] Text timeText;

    // Start is called before the first frame update
    void Start()
    {
        timeText.text = Mathf.Floor(SaveSystem.LoadScore().elapsedTime / 60).ToString("00") +
                                    ":" +
                                    ((int)SaveSystem.LoadScore().elapsedTime % 60).ToString("00");
    }
}
