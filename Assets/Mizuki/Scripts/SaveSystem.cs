using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
    public static void SaveScore(GameManager gM)
    {
        FileStream stream = new FileStream(Application.persistentDataPath + "/robotch.score", FileMode.Create);
        new BinaryFormatter().Serialize(stream, new ScoreData(gM));
        stream.Close();
    }

    public static ScoreData LoadScore()
    {
        if (File.Exists(Application.persistentDataPath + "/robotch.score"))
        {
            FileStream stream = new FileStream(Application.persistentDataPath + "/robotch.score", FileMode.Open);
            ScoreData data = new BinaryFormatter().Deserialize(stream) as ScoreData;
            stream.Close();
            return data;
        }
        else
        {
            Debug.LogError("Save file not found");
            return null;
        }
    }
}
