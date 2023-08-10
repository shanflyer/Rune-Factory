using System.IO;
using System.Text;
using LitJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public enum Influence
{
    NPC0=0,
    NPC1=4,
    PLAYER=1,
    MONSTER0=2,
    MONSTER1=3
}
[System.Serializable]
public class InfluenceData
{
    public Influence name;
    public List<Influence> friends;
    public List<Influence> enemies;
} 
public class InfluenceAction : MonoBehaviour
{
    public List<InfluenceData> influenceDates;
    public static List<InfluenceData> influenceDates0;

    public void InitInfluenceAction()
    {
        JsonToInfluenceData();
    }
    public void InfluenceDataToJson()
    {
        string filePath = Application.dataPath + @"/Resources/Data/InfluenceDates.json";
        string jsonStr = JsonMapper.ToJson(influenceDates);
        FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate);
        StreamWriter sw = new StreamWriter(fileStream);
        sw.Write(jsonStr);
        sw.Close();
    }

    public bool IsEmeny(Influence influence1, Influence influence2)
    {
        if (influenceDates.Find(influence => influence.name== influence1).enemies.Contains(influence2))
        {
            return true;
        }
        return false;
    }
    public bool IsFriend(Influence influence1, Influence influence2)
    {
        if (influenceDates.Find(influence => influence.name == influence1).friends.Contains(influence2))
        {
            return true;
        }
        return false;
    }
    public void JsonToInfluenceData()
    {
        string filePath = Application.dataPath + @"/Resources/Data/InfluenceDates.json";
        if (!File.Exists(filePath))
        {
            Debug.LogError("No" + filePath);
        }
        else
        {
            string jsonStr = File.ReadAllText(filePath);
            influenceDates = JsonMapper.ToObject<List<InfluenceData>>(jsonStr);
        }
        influenceDates0 = influenceDates;
    }

}
