using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.IO;
[System.Serializable]
public class VillageData
{
    public string name;
    public int id;
    public List<int> sellingGoods;
    public List<int> needGoods;
    public string text;
}
public class VillageManager : MonoBehaviour
{
    public List<VillageData> VillageDatas;
	// Use this for initialization
	void Start () {
		
	}

    public void DataToJson()
    {
        string path = Application.dataPath + @"/Resources/Data/" + "VillageDatas.json";
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        string jsonStr = JsonMapper.ToJson(VillageDatas);
        FileStream fileStream=new FileStream(path,FileMode.Create);
        StreamWriter streamWriter=new StreamWriter(fileStream);
        streamWriter.Write(jsonStr);
        streamWriter.Close();
    }

    public void JsonToData()
    {
        string path = "/Datas/VillageDatas";
        TextAsset textAsset = Resources.Load<TextAsset>(path);
        if (textAsset != null)
        {
            string jsonStr = textAsset.text;
            VillageDatas = JsonMapper.ToObject<List<VillageData>>(jsonStr);
        }
    }
	// Update is called once per frame
	void Update () {
		
	}
}
