using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using LitJson;

[System.Serializable]
public class Fish
{
    public string name;
    public int id;
    public int randomValue;
    public List<Season> Seasons;
    public int item;
    public Fish() { }

    public Fish(FishStr fishStr)
    {
        name = fishStr.name;
        id = fishStr.id;
        randomValue = fishStr.randomValue;
        item = fishStr.item;
        Seasons=new List<Season>();
        foreach (var s in fishStr.Seasons.Split(','))
        {
            Season season = (Season) int.Parse(s);
            Seasons.Add(season);
        }
    }
}

[System.Serializable]
public class FishStr
{
    public string name;
    public int id;
    public int randomValue;
    public string Seasons;
    public int item;
    public FishStr() { }

    public FishStr(Fish fish)
    {
        name = fish.name;
        id = fish.id;
        randomValue = fish.randomValue;
        item = fish.item;
        Seasons = "";
        foreach (var fishSeason in fish.Seasons)
        {
            Seasons += fishSeason + ",";
        }
    }
}
public class FishManager : MonoBehaviour
{
    public GameObject fishPanelObj;
    public int ZeroExp, AddUpExp;
    public List<Fish> Fishes;
    [HideInInspector] public List<FishStr> FishStrs;
    public int costRpValue;
    //private Charactor gamerCharactor;
	// Use this for initialization
	void Start ()
	{
	    //gamerCharactor = GameComponentData.gameData.gameManager.playerCharactor;
	    foreach (var fish in Fishes)
	    {
	        fish.name = LanguageManage.SwitchStr(fish.name);
            
	    }
	}

    public void StartFish()
    {
        if (GameComponentData.gameData.gameManager.CostRp(costRpValue))
        {
            
            fishPanelObj.SetActive(true);
            fishPanelObj.GetComponent<FishingPanelAction>().InitFishData();
        }
        
    }
    public void DataToJson()
    {
        FishStrs = new List<FishStr>();
        foreach (var fish in Fishes)
        {
            FishStr fishStr=new FishStr(fish);
            FishStrs.Add(fishStr);
        }

        string filePath = Application.dataPath + @"/Resources/Datas/FishDatas.json";
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        string jsonStr = JsonMapper.ToJson(FishStrs);
        FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate);
        StreamWriter sw = new StreamWriter(fileStream);
        sw.Write(jsonStr);
        sw.Close();
    }
    public void JsonToData()
    {
        TextAsset fileText = Resources.Load<TextAsset>("Datas/" + "FishDatas");
        if (fileText == null)
        {
            // Debug.LogError("No" + "ArmData");
        }
        else
        {
            string jsonStr = fileText.text;
            FishStrs= new List<FishStr>();
            FishStrs = JsonMapper.ToObject<List<FishStr>>(jsonStr);
            Fishes = new List<Fish>();
            foreach (var fishStrData in FishStrs)
            {
                Fish fish = new Fish(fishStrData);
                Fishes.Add(fish);
            }

        }
    }


    // Update is called once per frame
    void Update () {
		
	}
}
