using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.IO;

public enum PlantStatus
{
    Grow=0,
    Mature=1,
    Withered=2
}
[System.Serializable]
public class PlantBaseDataStr
{
    public string name;
    public int SeedId;
    public int fruitId;
    public int fruitIdNum;
    public int GrowthDays;
    public int turnCount;
    public string AdvantageSeasons;
    public string SeedGameObjectName;
    public string MatureGameObjectName;
    public string GrowthGameObjectNames;
    public PlantBaseDataStr() { }

    public PlantBaseDataStr(PlantBaseData plantBaseData)
    {
        name = plantBaseData.name;
        SeedId = plantBaseData.SeedId;
        fruitId = plantBaseData.fruitId;
        fruitIdNum = plantBaseData.fruitIdNum;
        GrowthDays = plantBaseData.GrowthDays;
        turnCount = plantBaseData.turnCount;
        AdvantageSeasons = "";
        foreach (var advantageSeason in plantBaseData.AdvantageSeasons)
        {
            AdvantageSeasons += ((int)(advantageSeason)).ToString()+",";
        }
        SeedGameObjectName = plantBaseData.SeedGameObjectName;
        MatureGameObjectName = plantBaseData.MatureGameObjectName;
        GrowthGameObjectNames = "";
        foreach (var growthGameObjectName in plantBaseData.GrowthGameObjectNames)
        {
            GrowthGameObjectNames += growthGameObjectName+",";
        }
    }
}
[System.Serializable]
public class PlantBaseData
{
    public string name;
    public int SeedId;
    public int fruitId;
    public int fruitIdNum;
    public int GrowthDays;
    public int turnCount;
    public List<Season> AdvantageSeasons;
    public string SeedGameObjectName;
    public string MatureGameObjectName;
    public List<string> GrowthGameObjectNames;
    public PlantBaseData() { }

    public PlantBaseData(PlantBaseDataStr plantBaseDataStr)
    {
        name = plantBaseDataStr.name;
        SeedId = plantBaseDataStr.SeedId;
        fruitId = plantBaseDataStr.fruitId;
        fruitIdNum = plantBaseDataStr.fruitIdNum;
        GrowthDays = plantBaseDataStr.GrowthDays;
        turnCount = plantBaseDataStr.turnCount;
        AdvantageSeasons=new List<Season>();
        var x = plantBaseDataStr.AdvantageSeasons.Split(',');
        foreach (var s in x)
        {
            Season season = (Season) int.Parse(s);
            AdvantageSeasons.Add(season);
        }
        SeedGameObjectName = plantBaseDataStr.SeedGameObjectName;
        MatureGameObjectName = plantBaseDataStr.MatureGameObjectName;
        GrowthGameObjectNames=new List<string>();
        foreach (var objName in plantBaseDataStr.GrowthGameObjectNames.Split(','))
        {
           GrowthGameObjectNames.Add(objName);
        }

    }
}

public class Plant : MyGameObject
{
    public PlantBaseData plantBaseData;
    public int growedDays;
    public int turnCount;
    public PlantStatus plantStatus;
    public int dryDays;
    public int statusIndex;
    public Field field;
    public Plant(int _id, string _name, GameObject obj,int _mapId, Vector2Int _coordinate, PlantBaseData _plantBaseData):base(_id,obj,_name,_mapId,_coordinate)
    {
        plantBaseData = _plantBaseData;
        growedDays = 0;
        turnCount =plantBaseData.turnCount;
        plantStatus=PlantStatus.Grow;
        dryDays = 0;
        statusIndex = 0;
    }

    public Plant(int _id, int _mapId, Vector2Int _coordinate, int _growedDays, int _turnCount,
        PlantStatus _plantStatus,int _dryDays,int _statusIndex)
    {
        id = _id;
        plantBaseData = GameComponentData.gameData.plantAction.PlantBaseDatas.Find(p => p.SeedId == _id / 10000);
        mapId=_mapId;
        coordinate = _coordinate;
        growedDays = _growedDays;
        turnCount = _turnCount;
        plantStatus = _plantStatus;
        dryDays = _dryDays;
        statusIndex = _statusIndex;
        field = GameComponentData.gameData.farmAction.Fields.Find(
            f => f.mapId == _mapId && f.coordinate == _coordinate);
    }
    public Plant(int _id, int _mapId, Vector2Int _coordinate,Field _field)
    {
        id = _id;
        plantBaseData = GameComponentData.gameData.plantAction.PlantBaseDatas.Find(p => p.SeedId == _id / 10000);
        Name = plantBaseData.name;
        mapId = _mapId;
        coordinate = _coordinate;
        field = _field;
        growedDays =0;
        turnCount =plantBaseData.turnCount;
        plantStatus = PlantStatus.Grow;
        dryDays = 0;
        statusIndex = 0;
    }

    public void InitNewTurn()
    {
        int statusNum = plantBaseData.GrowthGameObjectNames.Count + 2;
        int statusDays;
        if (plantBaseData.AdvantageSeasons.Contains(GameTimeManager.nowGameTime.gameDate.season))
        {
            statusDays = plantBaseData.GrowthDays / statusNum;
        }
        else
        {
            statusDays = plantBaseData.GrowthDays * 2 / statusNum;
        }
        growedDays = statusDays * (statusNum - 1);
        InitPlantObj();
    }

    public void SetPlantStatus()
    {
        float statusNum = plantBaseData.GrowthGameObjectNames.Count + 2;
        float statusDays;
        if (plantBaseData.AdvantageSeasons.Contains(GameTimeManager.nowGameTime.gameDate.season))
        {
            statusDays = plantBaseData.GrowthDays / statusNum;
        }
        else
        {
            statusDays = plantBaseData.GrowthDays * 2 / statusNum;
        }
        if (statusDays > 0)
        {
            statusIndex = Mathf.RoundToInt(growedDays / statusDays);
        }
        
       if (statusIndex >= plantBaseData.GrowthDays - 1)
        {
            plantStatus=PlantStatus.Mature;
        }
        if (GameComponentData.gameData.passDataManager.NowPassData.id == mapId)
        {
            Cell cell = AStarTest.GetCellWithCoordinate(coordinate);
            if (!cell.myGameObjects.Exists(m => m.id == id))
            {
                cell.myGameObjects.Add(this);
            }
            InitPlantObj();
        }


    }
    public void InitPlantObj()
    {
        MonoBehaviour.Destroy(Obj);
        Sprite ObjPro;
        if (plantStatus == PlantStatus.Mature)
        {
            InformationController.instance.AddInformation(LanguageManage.SwitchStr("*一株")+ LanguageManage.SwitchStr(plantBaseData.name)+ LanguageManage.SwitchStr("已经成熟。"));
            ObjPro = GameComponent.PlantSprites.Find(p => p.name == plantBaseData.MatureGameObjectName);
        }
        else
        {
            int statusNum = plantBaseData.GrowthGameObjectNames.Count + 2;
            if (statusIndex == 0)
            {
                ObjPro = GameComponent.PlantSprites.Find(p=>p.name==plantBaseData.SeedGameObjectName);
            }
            else if (statusIndex >= statusNum - 1)
            {
                ObjPro = GameComponent.PlantSprites.Find(p => p.name == plantBaseData.MatureGameObjectName);
            }
            else
            {
                ObjPro = GameComponent.PlantSprites.Find(p => p.name == plantBaseData.GrowthGameObjectNames[statusIndex - 1]);
            }
        }
        Vector3 pos = AStarTest.CoordinateToPos(coordinate);
        Obj = new GameObject(Name);
        Obj.AddComponent<SpriteRenderer>();
        SpriteRenderer plantRenderer = Obj.GetComponent<SpriteRenderer>();
        plantRenderer.sprite = ObjPro;
        plantRenderer.transform.localScale=new Vector3(2,2,2);
        plantRenderer.sortingLayerName = "Map";
        plantRenderer.sortingOrder = 3;
        Obj.transform.SetParent(GameComponentData.gameData.plantAction.plantParent);
        Obj.transform.position = pos;
        if (plantStatus == PlantStatus.Withered)
        {
            InformationController.instance.AddInformation(LanguageManage.SwitchStr("*一株") + LanguageManage.SwitchStr(plantBaseData.name) + LanguageManage.SwitchStr("处于干旱状态。"));
            WitheredAction();
        }
    }


    public void ReWitheredAction()
    {
        plantStatus = PlantStatus.Grow;
        var x = Obj.GetComponentsInChildren<SpriteRenderer>();
        foreach (var spriteRenderer in x)
        {
            spriteRenderer.color = new Color(1f, 1f, 1f, 1);
        }
    }
    public void WitheredAction()
    {
        plantStatus=PlantStatus.Withered;
        var x = Obj.GetComponentsInChildren<SpriteRenderer>();
        foreach (var spriteRenderer in x)
        {
            spriteRenderer.color=new Color(0.23f,0.25f,0.01f,1);
        }
    }
}
public class PlantAction : MonoBehaviour
{
    [HideInInspector]
    public List<PlantBaseDataStr> PlantBaseDataStrs;
    
    public List<PlantBaseData> PlantBaseDatas;
    [HideInInspector]
    public List<Plant> Plants;
    public Transform plantParent;

    private PassDataManager passDataManager;
	// Use this for initialization
	void Start ()
	{
       // Plants=new List<Plant>();
	    passDataManager = GameComponentData.gameData.passDataManager;
	    JsonToData();

    }

    public void ZeroInitData()
    {
        Plants = new List<Plant>();
        
    }
    public void DataToJson()
    {
        string filePath = Application.dataPath + @"/Resources/Datas/" + "PlantDatas.json";
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        PlantBaseDataStrs = new List<PlantBaseDataStr>();
        foreach (var plantBase in PlantBaseDatas)
        {
            PlantBaseDataStr plantBaseDataStr = new PlantBaseDataStr(plantBase);
            PlantBaseDataStrs.Add(plantBaseDataStr);
        }
        string jsonStr = JsonMapper.ToJson(PlantBaseDataStrs);
        FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate);
        StreamWriter sw = new StreamWriter(fileStream);
        sw.Write(jsonStr);
        sw.Close();
    }
    public void JsonToData()
    {
        TextAsset file = Resources.Load<TextAsset>("Datas/PlantDatas");
        if (file != null)
        {
            PlantBaseDataStrs = JsonMapper.ToObject<List<PlantBaseDataStr>>(file.text);

            PlantBaseDatas = new List<PlantBaseData>();
            foreach (var plantBaseDataStr in PlantBaseDataStrs)
            {
                PlantBaseData plantBase = new PlantBaseData(plantBaseDataStr);
                plantBase.name = LanguageManage.SwitchStr(plantBase.name);
                
                PlantBaseDatas.Add(plantBase);
            }
        }
        else
        {
            Debug.Log(file.name + "不存在");
        }


    }
    public void ClearPlantDisplay()
    {
        foreach (Transform plantChild in plantParent)
        {
            Destroy(plantChild.gameObject);
        }   
    }

    public void InitPlantDisplay()
    {
        if (Plants != null)
        {
            foreach (var plant in Plants)
            {
                plant.SetPlantStatus();
            }
        }
        
    }
    public void Planting(Cell cell, int SeedId,Field _field)
    {
        int plantId = SeedId/1000 *10000+ cell.coordinate.x*100+cell.coordinate.y;
        Plant plant=new Plant(plantId,passDataManager.nowPass,cell.coordinate,_field);
       
        InformationController.instance.AddInformation(LanguageManage.SwitchStr("*一株")+plant.Name+LanguageManage.SwitchStr("被种下！"));
        plant.SetPlantStatus();
        Plants.Add(plant);
        cell.myGameObjects.Add(plant);
        GameComponentData.gameData.charactorTitleAction.AddPlantExp(1);
    }

    public void CleraPlant(Plant _plant)
    {
        Plants.Remove(_plant);
        if (_plant.mapId == GameComponentData.gameData.passDataManager.nowPass)
        {
            Destroy(_plant.Obj);
            Cell cell = AStarTest.GetCellWithCoordinate(_plant.coordinate);
            cell.myGameObjects.RemoveAt(0);
        }
        
        
    }
    public async void PlantReward(Cell cell)
    {
        Plant plant = Plants.Find(p => p.id == cell.myGameObjects[0].id);
        ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(plant.plantBaseData.fruitId);
        Item item= ItemManager.instance.CreatItem(itemData.id,plant.plantBaseData.fruitIdNum);
        PackageManager.instance.SetItemInPackage(item,0);
        plant.turnCount--;
        if (plant.turnCount <= 0)
        {
            CleraPlant(plant);
        }
        else
        {
            plant.InitNewTurn();
        }
        
    }
    public async void PlantReward(Plant plant)
    {
        ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(plant.plantBaseData.fruitId);
        Item item = ItemManager.instance.CreatItem(itemData.id, plant.plantBaseData.fruitIdNum);
        
        GameComponentData.gameData.charactorTitleAction.AddPlantExp(1);
        if (await PackageManager.instance.CheckPackageTryItemIn(0,itemData.id, plant.plantBaseData.fruitIdNum))
        {
            PackageManager.instance.SetItemInPackage(item, 0); 
            plant.turnCount--;
            if (plant.turnCount <= 0)
            {
                CleraPlant(plant);
                Cell cell = AStarTest.GetCellWithCoordinate(plant.coordinate);
                GameComponentData.gameData.farmAction.ChangeTileFieldStatus(cell, FieldStatus.Barren);
            }
            else
            {
                plant.InitNewTurn();
            }
            InformationController.instance.AddInformation(LanguageManage.SwitchStr("*收获了") + plant.plantBaseData.fruitIdNum + LanguageManage.SwitchStr("个") + itemData.name);
        }
        else
        {
            GameNotificationManager.instance.DisplayTips(LanguageManage.SwitchStr("提示"),LanguageManage.SwitchStr("背包空间不足，无法收获！"));
        }
        
        
    }
    public void PlantGrowing()
    {
        List<Plant> dryDeadPlants=new List<Plant>();
        foreach (var plant in Plants)
        {
            if (plant.field.fieldStatus == FieldStatus.Wet)
            {
                plant.growedDays++;
            }
            else
            {
                plant.plantStatus = PlantStatus.Withered;
                plant.dryDays++;
                if (plant.dryDays >= plant.plantBaseData.GrowthDays/ 2.0f)
                {
                    dryDeadPlants.Add(plant);
                }
            }


             GameComponentData.gameData.farmAction.DryAction(plant.field);
            plant.SetPlantStatus();
        }
        foreach (var dryDeadPlant in dryDeadPlants)
        {
            InformationController.instance.AddInformation(LanguageManage.SwitchStr("*一株")+ LanguageManage.SwitchStr(dryDeadPlant.plantBaseData.name) +LanguageManage.SwitchStr("枯死。"));
            CleraPlant(dryDeadPlant);
        }
        foreach (var farmActionField in GameComponentData.gameData.farmAction.Fields)
        {
            if (farmActionField.fieldStatus == FieldStatus.Wet)
            {
                farmActionField.fieldStatus = FieldStatus.Dry;
            }
        }
    }
    // Update is called once per frame
    void Update () {
		
	}
}
