
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LitJson;
using OldName;
using UnityEngine;
[System.Serializable]
public class MonsterGroup
{
    public int id;
    public string map;
    public int time;
    public int weight;
    public int monsterId0;
    public int monsterId1;
    public int monsterId2;
    public int attributeType0;
    public int attributeType1;
    public int attributeType2;
    public int probability0;
    public int probability1;
    public int probability2;


}
public class MonsterManager : MonoBehaviour
{
    public List<MonsterData> MonsterDatas;
    private List<MonsterStrData> MonsterStrDatas;
    public void DataToJson()
    {
        MonsterStrDatas=new List<MonsterStrData>();
        foreach (var monsterData in MonsterDatas)
        {
            MonsterStrData monsterStrData=new MonsterStrData(monsterData);
            MonsterStrDatas.Add(monsterStrData);
        }

        string filePath = Application.dataPath + @"/Resources/Datas/MonsterDatas.json";
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        string jsonStr = JsonMapper.ToJson(MonsterStrDatas);
        FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate);
        StreamWriter sw = new StreamWriter(fileStream);
        sw.Write(jsonStr);
        sw.Close();
    }
    public void JsonToData()
    {
        TextAsset fileText = Resources.Load<TextAsset>("Datas/" + "MonsterDatas");
        if (fileText == null)
        {
            // Debug.LogError("No" + "ArmData");
        }
        else
        {
            string jsonStr = fileText.text;
            MonsterStrDatas=new List<MonsterStrData>();
            MonsterStrDatas = JsonMapper.ToObject<List<MonsterStrData>>(jsonStr);
            MonsterDatas=new List<MonsterData>();
            foreach (var monsterStrData in MonsterStrDatas)
            {
                MonsterData monsterData = new MonsterData(monsterStrData);
                monsterData.name = LanguageManage.SwitchStr(monsterData.name);
                MonsterDatas.Add(monsterData);
            }
            
        }
    }
    // Use this for initialization
    void Start () {
		JsonToData();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
