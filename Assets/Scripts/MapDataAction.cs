using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LitJson;
using UnityEngine.Tilemaps;

[System.Serializable]
public enum TileType
{
    草地,
    坑地,
    道路,
    森林,
    山地,
    高山,
    深水,
    城墙,
    宫殿,
    墙壁,
    室内,
    神殿,
    王位,
    桥梁,
    吊桥,
    天空,
    洞穴,
    熔岩,
    墓地,
    甲板,
    浅滩,
    屋顶,
    船舷,
    废墟,
    墙内,
    墙顶,
    边界,
}

[System.Serializable]
public enum TileLayer
{
    地面=0,
    饰物=1
}
[System.Serializable]
public class ColorData
{
    public int r, g, b,a;
    public ColorData()
    {
        r = 255;
        g = 255;
        b = 255;
        a = 122;
    }
    public ColorData(float _r,float _g,float _b,float _a)
    {
        r = (int)_r;
        g = (int)_g;
        b = (int)_b;
        a = (int)_a;
    }
}
[System.Serializable]
public class _Tile
{
    public string name;
    public string EnglishName;
    public TileType tileType;
    public int barrierValue;
    public int AttackAddition;
    public int DefenseAddition;
    public int eventId;
    public string fightSceneName; 
    public TileLayer tileLayer;
    public ColorData colorData;
    public _Tile()
    {
        name = "默认地形";
        tileType = TileType.道路;
        barrierValue = 1;
        AttackAddition = 0;
        DefenseAddition = 0;
        eventId = 0;
        tileLayer=TileLayer.地面;
    }
    public _Tile(MyTile tile)
    {
        name = tile.name;
        EnglishName = tile.EnglishName;
        tileType = tile.tileType;
        barrierValue = tile.barrierValue;
        AttackAddition = tile.AttackAddition;
        DefenseAddition = tile.DefenseAddition;
        eventId = tile.eventId;
        fightSceneName = tile.fightSceneName;
        tileLayer = tile.tileLayer;
        colorData = MapDataAction.ColorToData(tile.TileColor);
    }
}
[System.Serializable]
public struct MyTile
{
    public string name;
    public string EnglishName;
    public TileType tileType;
    public int barrierValue;
    public int AttackAddition;
    public int DefenseAddition;
    public int eventId;
    public string fightSceneName;
    
    public TileLayer tileLayer;
    public Color TileColor;
    public MyTile(_Tile _tile)
    {
        name = _tile.name;
        EnglishName = _tile.EnglishName;
        tileType = _tile.tileType;
        barrierValue = _tile.barrierValue;
        AttackAddition = _tile.AttackAddition;
        DefenseAddition = _tile.DefenseAddition;
        eventId = _tile.eventId;
        fightSceneName=_tile.fightSceneName;
        tileLayer = _tile.tileLayer;
        TileColor = MapDataAction.DataToColor(_tile.colorData);
    }

   
}
public class MapDataAction : MonoBehaviour {
    
    public Transform tileToggleContent;
    public GameObject tileToggle;
    private List<_Tile> _tiles;
    public List<MyTile> tiles;
    // Use this for initialization
    void Start()
    {
       



    }
    public static Color DataToColor(ColorData _colorData)
    {
        return new Color((float)_colorData.r / 255.0f, (float)_colorData.g / 255.0f, (float)_colorData.b / 255.0f, (float)_colorData.a / 255.0f);
    }
    public static ColorData ColorToData(Color _color)
    {
        return new ColorData(_color.r * 255.0f, _color.g* 255.0f, _color.b* 255.0f, _color.a * 255.0f);
    }
    public static Color DataToToogleColor(ColorData _colorData)
    {
        return new Color(_colorData.r / 255.0f, _colorData.g / 255.0f, _colorData.b / 255.0f, 1);
    }
    public static Color DataToToogleColor(Color _color)
    {
        return new Color(_color.r,_color.g,_color.b, 1);
    }
    void CreatTileToggle()
    {
         for (int i = 0; i < tileToggleContent.childCount;i++ )
         {
            Debug.Log("count:" + i);
             DestroyImmediate(tileToggleContent.GetChild(i).gameObject);
         }

        foreach (var tile in tiles)
        {
            GameObject tileObj = Instantiate(tileToggle, Vector3.zero, Quaternion.identity) as GameObject;
            tileObj.transform.parent = tileToggleContent;
            tileObj.GetComponent<Toggle>().group = tileToggleContent.gameObject.GetComponent<ToggleGroup>();
            tileObj.name = tile.name;
            tileObj.GetComponentInChildren<Image>().color = tile.TileColor;
            tileObj.GetComponentInChildren<Text>().text = tile.name;
        }

    }
    public void TileDataToJson()
    {
        _tiles=new List<_Tile>();
        foreach(var t in tiles)
        {
            _Tile _tile = new _Tile(t);
            _tiles.Add(_tile);
        }
        string filePath = Application.dataPath + @"/Resources/Datas/TileData.json";
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        string jsonStr = JsonMapper.ToJson(_tiles);
        FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate);
        StreamWriter sw = new StreamWriter(fileStream);
        sw.Write(jsonStr);
        sw.Close();
        //    CreatTileToggle();
       /* File.WriteAllText(FILEPATH + tileDataFileName, jsonStr, Encoding.UTF8);*/
    }
    public void JsonToTileData()
    {
        string filePath = Application.dataPath + @"/Resources/Datas/TileData.json";
        if (!File.Exists(filePath))
        {
            Debug.LogError("No" + filePath);
        }
        else
        {
            string jsonStr = File.ReadAllText(filePath);
            _tiles = JsonMapper.ToObject<List<_Tile>>(jsonStr);
        }
        tiles.Clear();
        foreach (var _t in _tiles)
        {
            MyTile tile = new MyTile(_t);
            tiles.Add(tile);
        }
        CreatTileToggle();
    }
	
	
	// Update is called once per frame
	void Update () {
		
	}
}
