using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreCheck : MonoBehaviour
{
    [SerializeField] ScoreToRank<string, Color, int, int>[] rankList = default;
    [SerializeField] Text timeText;
    [SerializeField] Text rankText;

    [System.Serializable] public class ScoreToRank<Rank, Color, Minute, Second>
    {
        [SerializeField] Rank _rank;
        [SerializeField] Color _color;
        [SerializeField] Minute _minute;
        [SerializeField] Second _second;

        public Rank rank => _rank;
        public Color color => _color;
        public Minute minute => _minute;
        public Second second => _second;
    }

    // Start is called before the first frame update
    void Start()
    {
        int elapsedTime = Mathf.FloorToInt(SaveSystem.LoadScore().elapsedTime);

        timeText.text = Mathf.Floor(elapsedTime / 60).ToString("00") + ":" + (elapsedTime % 60).ToString("00");

        for (int i = 0; i < rankList.Length ; i++)
        {
            if (elapsedTime < (rankList[i].minute * 60) + rankList[i].second)
            {
                rankText.text = rankList[i].rank;
                rankText.color = rankList[i].color;
                break;
            }
        }
    }
}
