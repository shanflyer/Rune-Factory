using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
using LitJson;
using UnityEngine;

[System.Serializable]
public class MapStartCoordinate
{
    public int mapId;
    public MyVector2 coordiante;
}
[System.Serializable]
public struct ChangeBGM
{
    public int enevtId;
    public string BGMName;
}
[System.Serializable]
public class Pass
{
    public string name;
    public string EnglishName;
    public List<MapStartCoordinate> ZeroMapStartCoordinates;
    public MyVector2  zeroGoldCoordinate;
    public int id;
    public int  mapSizeX,mapSizeY;
    public string background;
    public string FriendBGM;
    public string passText;
    public string passEnglishiText;
    public string playerCoordinates;
}

public class PassDataManager : MonoBehaviour
{

    public string springBGM, summerBGM, fillBGM, winterBGGM;

    public List<Pass> passes;
    [HideInInspector]
    public Pass NowPassData;
    public int nowPass;
	// Use this for initialization
	void Start () {
	    JsonToData();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void PlayerMapBGM()
    {
        /*
        if (NowPassData.FriendBGM != "")
        {
            AudioManager.PlayBGM(PlayType.CYCLE,NowPassData.FriendBGM);
        }
        else
        {
            switch (GameTimeManager.nowGameTime.gameDate.season)
            {
                    case Season.春:
                        AudioManager.PlayBGM(PlayType.CYCLE, springBGM);
                    break;
                    case Season.夏:
                        AudioManager.PlayBGM(PlayType.CYCLE, summerBGM);
                    break;
                    case Season.秋:
                        AudioManager.PlayBGM(PlayType.CYCLE, fillBGM);
                    break;
                    case Season.冬:
                        AudioManager.PlayBGM(PlayType.CYCLE, winterBGGM);
                    break;
            }
        }*/
    }
    public void InitData()
    {
        JsonToData();
        NowPassData = passes.Find(p => p.id == nowPass);
    }
    public void JsonToData()
    {
        passes = new List<Pass>();
        TextAsset fileText = Resources.Load<TextAsset>("Datas/passData");
        
        if (fileText != null)
        {
            string datas = fileText.text;
            passes = JsonMapper.ToObject<List<Pass>>(datas);
        }
        else
        {
            Debug.LogError( "passData.json 不存在！");
        }
        if (Application.systemLanguage == SystemLanguage.ChineseSimplified||Application.systemLanguage==SystemLanguage.Chinese||Application.systemLanguage ==SystemLanguage.ChineseTraditional)
        {
            
        }
        else
        {
            foreach (var pass in passes)
            {
                pass.name = pass.EnglishName;
                pass.passText = pass.passEnglishiText;
            }
        }
    }
    public void DataToJson()
    {
        string path = Application.dataPath + @"/Resources/Datas/passData.json";
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        string jsonStr = JsonMapper.ToJson(passes);
        FileStream fileStream = new FileStream(path, FileMode.OpenOrCreate);
        StreamWriter sw = new StreamWriter(fileStream);
        sw.Write(jsonStr);
        sw.Close();
    }
}
