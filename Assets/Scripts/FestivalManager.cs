using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.IO;

[System.Serializable]
public enum FestivalType
{
    比赛=1,
    售卖=2,
    纪念=3,
    狂欢=4
}
[System.Serializable]
public class FestivalData
{
    public string name;
    public string englishName;
    public int id;
    public Season season;
    public int date;
    public FestivalType festivalType;
    public string value;
    public string text;
    public string englishText;

}
public class FestivalManager : MonoBehaviour
{
    public List<FestivalData> FestivalDatas;

    [HideInInspector]
    public List<FestivalData> customFestivalDatas;
    
	// Use this for initialization
	void Start () {
		
	}

    public void InitData()
    {
        FestivalDatas = new List<FestivalData>();
        CreatNPCBrothDay();
    }

    public void LoadBrothDay()
    {
        FestivalDatas = new List<FestivalData>();
        FestivalData festivalData0 =
            new FestivalData
            {
                name = GameComponentData.gameData.gameManager.gamePlayer.name + LanguageManage.SwitchStr(" 的生日"),
                season = GameComponentData.gameData.gameManager.gamePlayer.season,
                date = GameComponentData.gameData.gameManager.gamePlayer.date,
                id = 8
            };
        FestivalDatas.Add(festivalData0);

        foreach (var npcx in GameComponentData.gameData.NpcManager.Npcxs)
        {
            FestivalData festivalData = new FestivalData
            {
                date = npcx.npcData.brothDate,
                season = npcx.npcData.brothSeason,
                id = npcx.id,
                name = npcx.npcData.name + LanguageManage.SwitchStr(" 的生日"),
                festivalType = FestivalType.纪念
            };
            FestivalDatas.Add(festivalData);
        }

    }
    public void CreatNPCBrothDay()
    {
        List<GameTime> gameTimes=new List<GameTime>();
        FestivalData festivalData0 =
            new FestivalData
            {
                name = PlayerDate.playerName + LanguageManage.SwitchStr(" 的生日"),
                season = PlayerDate.season,
                date = PlayerDate.date,
                id = 8
            };
        FestivalDatas.Add(festivalData0);
        GameTime gameTime=new GameTime(0,festivalData0.season,festivalData0.date,0,0);
        gameTimes.Add(gameTime);
       

        foreach (var npcManagerNpcData in GameComponentData.gameData.NpcManager.NpcDatas)
        {
            GameTime x;
            
            while (true)
            {
                Season season = (Season)Random.Range(1, 5);
                int day = Random.Range(1, 31);
                x = new GameTime(0, season, day, 0, 0);
                if (!gameTimes.Contains(x))
                {
                    break;
                }
            }
            gameTimes.Add(x);
            FestivalData festivalData = new FestivalData
            {
                date = x.gameDate.date,
                season = x.gameDate.season,
                id = npcManagerNpcData.id,
                name = npcManagerNpcData.name + LanguageManage.SwitchStr(" 的生日"),
                festivalType = FestivalType.纪念
            };
            FestivalDatas.Add(festivalData);
        }
        


    }
    public void DataToJson()
    {
        string path = Application.dataPath + "/Resources/Datas/FestivalDatas.json";
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        string jsonStr = JsonMapper.ToJson(FestivalDatas);
        FileStream fileStream=new FileStream(path,FileMode.OpenOrCreate);
        StreamWriter stream=new StreamWriter(fileStream);
        stream.Write(jsonStr);
        stream.Close();
    }

    public void JsonToData()
    {
        string path = "Datas/FestivalDatas";
        TextAsset textAsset = Resources.Load<TextAsset>(path);
        if (textAsset != null)
        {
            string jsonStr = textAsset.text;
            FestivalDatas = JsonMapper.ToObject<List<FestivalData>>(jsonStr);
        }
    }
	// Update is called once per frame
	void Update () {
		
	}
}
