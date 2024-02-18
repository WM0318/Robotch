using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ScoreData
{
    public float elapsedTime;

    public ScoreData(GameManager gM)
    {
        elapsedTime = gM.time;
    }
}
