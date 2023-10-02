using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using OldName;
[System.Serializable]
public class Grass
{
    public int Mapid;
    public Vector2Int startCoordinate, endCoordinate;
    [HideInInspector]
    public List<Cell> cells;
    [HideInInspector]
    public GameObject Obj;
    public bool isClear;
    public void InitCells()
    {
        cells=new List<Cell>();
        for (int i = startCoordinate.x; i <=endCoordinate.x ; i++)
        {
            for (int j = startCoordinate.y; j <=endCoordinate.y; j++)
            {
                Cell cell = AStarTest.GetCellWithCoordinate(new Vector2(i, j));
                cells.Add(cell);
            }
        }
    }

    public void ClearFarm()
    {
        isClear = true;
        Obj.SetActive(false);
        foreach (var cell in cells)
        {
            cell.isArable = true;
        }
    }
}

[System.Serializable]
public class FieldStr
{
    public int mapId;
    public int coordinateX,coordinateY;
    public FieldStatus fieldStatus;
    public FieldStr() { }
    public FieldStr(Field field)
    {
        mapId = field.mapId;
        coordinateX=field.coordinate.x;
        coordinateY = field.coordinate.y;
        fieldStatus = field.fieldStatus;
    }
}

[System.Serializable]
public class Field
{
    public int mapId;
    public Vector2Int coordinate;
    public FieldStatus fieldStatus;
    public Field() { }

    public Field(FieldStr fieldStr)
    {
        mapId = fieldStr.mapId;
        coordinate=new Vector2Int(fieldStr.coordinateX,fieldStr.coordinateY);
        fieldStatus = fieldStr.fieldStatus;
    }
    public Field(Cell cell,int _MapId )
    {
        mapId = _MapId;
        coordinate = cell.coordinate;
        fieldStatus = cell.fieldStatus;
    }
}

[System.Serializable]
public enum FarmToolType
{
    Default = 0,
    锄头 =1,
    水壶=2,
    种子=3,
    
}
public class FarmAction : MonoBehaviour
{
    public int GrassRp,workRp;
    public int clearZeroCost;
    public int clearAddCost;
    public int clearFieldNum;
    public Sprite normalField,dryField, wetField;
    public List<Grass> Grasses;
    [HideInInspector]
    public List<Field> Fields;
    [HideInInspector]
    public Item farmTool;
    //private ItemData farmToolData;
    private FarmToolType farmToolType;
    public GameObject warehouseObj;
    public Image farmToolSprite,WaterValueImage;
    private Grass clearGrass;
    public Text SeedCounText,TitleText;
    private Plant SelectPlant;
    public float waterValue;
    public GameObject hoePro, kettlePro, seedPro;
    private GameObject hoeObj, kettleObj, seedobj;
	// Use this for initialization
	void Start ()
	{
        
	}

    public void ZeroInit()
    {
        Fields = new List<Field>();
        farmTool =default(Item);
    }
    public void SelectFarmToolType(int i)
    {
        AudioController.instance.PlayAudio(SE.click);
        farmToolType = (FarmToolType) i;
        if (farmToolType == FarmToolType.种子)
        {
            warehouseObj.SetActive(true);
            //List<WareDisplayType> wareDisplayTypes = new List<WareDisplayType>();
           // wareDisplayTypes.Add(WareDisplayType.FarmTool);
           // warehouseObj.GetComponentInChildren<WarehouseAction>().InitWareHouseData(PackageType.背包, wareDisplayTypes, DisplayType.Equip);
        }
    }

    public void SelectSeed()
    {
        farmToolType=FarmToolType.种子;
        if (farmTool.instanceId == 0 || farmTool.count <= 0)
        {
            TitleText.text="空";
        }
    }

    public void InitWaterValue()
    {
        if (GameComponentData.gameData.gameManager.CostRp(GrassRp))
        {
            AudioController.instance.PlayAudio(SE.waterFull); 
            waterValue = 1;
            WaterValueImage.fillAmount = 1;
        }
    }
    public void InitFarm()
    {
        WaterValueImage.fillAmount = waterValue;
        int nowPass = GameComponentData.gameData.passDataManager.nowPass;
        foreach (var field in Fields)
        {
            if (field.mapId == nowPass)
            {
                Cell cell = AStarTest.GetCellWithCoordinate(field.coordinate);
                ChangeTileFieldStatus(cell,field.fieldStatus);
            }
        }
        if (farmTool.instanceId!=0)
        {
            InitFarmTool(farmTool);
        }

        GameComponentData.gameData.plantAction.InitPlantDisplay();
    }
    public async void InitFarmTool(Item item)
    {
        farmTool = item;
        if (item.instanceId != 0)
        { 
            ItemData toolData = await GameDataManager.instance.GetAsyncData<ItemData>(item.dataId);
            //farmToolData = toolData;
            farmToolSprite.sprite = toolData.icon;
            farmToolSprite.enabled = true;
            TitleText.text = toolData.name;
            if (toolData.typeValue == 3)
            {
                SeedCounText.enabled = true;
                SeedCounText.text = item.count.ToString();
            }
            if (toolData.typeValue == 3)
            {
                SeedCounText.enabled = true;
            }
            else
            {
                SeedCounText.enabled = false;
            }
        }
        else
        {
            SeedCounText.enabled = false;
            farmToolSprite.enabled = false;
        }
        
    }

    public void ReduceSeed()
    {
        farmTool.count--;
        
    }
    public void ClickSelectTool()
    {
       
    }

    public void DryAction(Field _field)
    {
        _field.fieldStatus=FieldStatus.Dry;
        if (_field.mapId == GameComponentData.gameData.passDataManager.nowPass)
        {
            Cell cell = AStarTest.GetCellWithCoordinate(_field.coordinate);
            cell.fieldStatus = FieldStatus.Dry;
            Pass nowPass = GameComponentData.gameData.passDataManager.NowPassData;
            Vector2Int tileCoordinate =
                cell.coordinate - new Vector2Int(nowPass.mapSizeX / 2, nowPass.mapSizeY / 2);

            Tilemap tilemap = GameComponentData.gameData.mapParent.GetComponentInChildren<Tilemap>();
            Tile tile = tilemap.GetTile<Tile>(new Vector3Int(tileCoordinate.x, tileCoordinate.y, 0));
            tile.sprite = dryField;
            tilemap.RefreshTile(new Vector3Int(tileCoordinate.x, tileCoordinate.y, 0));
        }
       

    }
    public void WaterAction(Cell cell)
    {
        ChangeTileFieldStatus(cell, FieldStatus.Wet);

        Plant plant = GameComponentData.gameData.plantAction.Plants.Find(p => p.coordinate == cell.coordinate);
        if (plant != null)
        {
            plant.plantStatus=PlantStatus.Grow;
            var x = plant.Obj.GetComponentsInChildren<SpriteRenderer>();
            foreach (var spriteRenderer in x)
            {
                spriteRenderer.color = new Color(1f, 1f, 1f, 1);
            }
        }
    }

    public void ChangeTileFieldStatus(Cell cell,FieldStatus fieldStatus)
    {
        cell.fieldStatus = fieldStatus;
        Pass nowPass = GameComponentData.gameData.passDataManager.NowPassData;
        Vector2Int tileCoordinate =
            cell.coordinate - new Vector2Int(nowPass.mapSizeX / 2, nowPass.mapSizeY / 2);

        Tilemap tilemap = GameComponentData.gameData.mapParent.GetComponentInChildren<Tilemap>();
        Tile tile = tilemap.GetTile<Tile>(new Vector3Int(tileCoordinate.x, tileCoordinate.y, 0));
        switch (fieldStatus)
        {
                case FieldStatus.Barren:
                    tile.sprite = normalField;
                break;
                case FieldStatus.Dry:
                    tile.sprite = dryField;
                break;
                case FieldStatus.Wet:
                    tile.sprite = wetField;
                break;
        }
        
        tilemap.RefreshTile(new Vector3Int(tileCoordinate.x, tileCoordinate.y, 0));
    }

    public void ClearSelectPlant()
    {
        GameComponentData.gameData.plantAction.Plants.Remove(SelectPlant);
        
        Cell plantCell = AStarTest.GetCellWithCoordinate(SelectPlant.coordinate);
        if (hoeObj != null)
        {
            Destroy(hoeObj);
        }
        hoeObj = Instantiate(hoePro);
        hoeObj.transform.position = AStarTest.CoordinateToPos(plantCell.coordinate);
        plantCell.myGameObjects.Remove(SelectPlant);
        Destroy(SelectPlant.Obj);
        SelectPlant = null;

    }
    public void FieldAction(Cell cell)
    {
        Plant plant = GameComponentData.gameData.plantAction.Plants.Find(p => p.coordinate == cell.coordinate);
        int nowPass = GameComponentData.gameData.passDataManager.nowPass;
        if (plant != null)
        {
            if (plant.plantStatus == PlantStatus.Mature)
            {
                if (GameComponentData.gameData.gameManager.CostRp(workRp))
                {
                    AudioController.instance.PlayAudio(SE.click);
                    GameComponentData.gameData.plantAction.PlantReward(plant);
                    Field field =
                        Fields.Find(f => f.mapId == nowPass &&
                                         f.coordinate == cell.coordinate);
                    if (field == null)
                    {
                        field = new Field(cell, nowPass);
                        Fields.Add(field);
                    }
                    else
                    {
                        field.fieldStatus = cell.fieldStatus;
                    }

                }
            }
            else if (farmToolType == FarmToolType.锄头)
            {
              //  GameComponentData.gameData.gameManager.InitCareSelectData(LanguageManage.SwitchStr("清除植物"),LanguageManage.SwitchStr("是否确定清除选择地块植物？"),CareType.ClearPlant);
                SelectPlant = plant;
            }
            else if (farmToolType == FarmToolType.水壶)
            {
                if (cell.fieldStatus == FieldStatus.Dry)
                {

                    if (waterValue > 0)
                    {

                        if (GameComponentData.gameData.gameManager.CostRp(workRp))
                        {
                            if (kettleObj != null)
                            {
                                Destroy(kettleObj);
                            }
                            kettleObj = Instantiate(kettlePro);
                            kettleObj.GetComponent<kettleAction>().InitKettle();
                            kettleObj.transform.position = AStarTest.CoordinateToPos(cell.coordinate);
                            AudioController.instance.PlayAudio(SE.watering);
                            waterValue -= 0.05f;
                            WaterValueImage.fillAmount = waterValue;
                            WaterAction(cell);
                            Field field =
                                Fields.Find(f => f.mapId == nowPass &&
                                                 f.coordinate == cell.coordinate);
                            if (field == null)
                            {
                                field = new Field(cell, nowPass);
                                Fields.Add(field);
                            }
                            else
                            {
                                field.fieldStatus = cell.fieldStatus;
                            }

                        }
                    }
                    else
                    {
                        AudioController.instance.PlayAudio(SE.Return);
                        InformationController.instance.AddInformation("*"+LanguageManage.SwitchStr("水量不足，需到池塘装水！"));
                        GameNotificationManager.instance.DisplayTips(LanguageManage.SwitchStr("提示"),LanguageManage.SwitchStr("水量不足，请到池塘装水！"));
                    }



                }
                else
                {
                    AudioController.instance.PlayAudio(SE.Fail);
                    InformationController.instance.AddInformation(LanguageManage.SwitchStr("*此地块不需要浇水了"));
                }

            }
        }
        else
        {
            if (farmToolType ==FarmToolType.Default)
            {
                
            }
            if (farmToolType==FarmToolType.锄头)
            {
                if (cell.fieldStatus == FieldStatus.Barren)
                {
                   
                    if (GameComponentData.gameData.gameManager.CostRp(workRp))
                    {
                        if (hoeObj != null)
                        {
                            Destroy(hoeObj);
                        }
                        hoeObj = Instantiate(hoePro);
                        hoeObj.transform.position = AStarTest.CoordinateToPos(cell.coordinate);
                        AudioController.instance.PlayAudio(SE.waJue); 
                        ChangeTileFieldStatus(cell, FieldStatus.Dry);
                        Field field =
                            Fields.Find(f => f.mapId == nowPass &&
                                             f.coordinate == cell.coordinate);
                        if (field == null)
                        {
                            field = new Field(cell, nowPass);
                            Fields.Add(field);
                        }
                        else
                        {
                            field.fieldStatus = cell.fieldStatus;
                        }

                    }

                }
                else 
                {
                    AudioController.instance.PlayAudio(SE.Fail);
                    InformationController.instance.AddInformation(LanguageManage.SwitchStr("*此地块不需要使用锄头"));
                }

            }
            else if (farmToolType == FarmToolType.水壶)
            {
                if (cell.fieldStatus == FieldStatus.Dry)
                {
                    
                    if (waterValue > 0)
                    {
                        
                        if (GameComponentData.gameData.gameManager.CostRp(workRp))
                        {
                            if (kettleObj != null)
                            {
                                Destroy(kettleObj);
                            }
                            kettleObj = Instantiate(kettlePro);
                            kettleObj.GetComponent<kettleAction>().InitKettle();
                            kettleObj.transform.position = AStarTest.CoordinateToPos(cell.coordinate);

                            AudioController.instance.PlayAudio(SE.watering);
                            waterValue -= 0.05f;
                            WaterValueImage.fillAmount = waterValue;
                            WaterAction(cell);
                            Field field =
                                Fields.Find(f => f.mapId == nowPass &&
                                                 f.coordinate == cell.coordinate);
                            if (field == null)
                            {
                                field = new Field(cell, nowPass);
                                Fields.Add(field);
                            }
                            else
                            {
                                field.fieldStatus = cell.fieldStatus;
                            }

                        }
                    }
                    else
                    {
                        AudioController.instance.PlayAudio(SE.Return);
                        InformationController.instance.AddInformation("*"+LanguageManage.SwitchStr("水量不足，需到池塘装水！"));
                        GameNotificationManager.instance.DisplayTips(LanguageManage.SwitchStr("提示"),LanguageManage.SwitchStr("水量不足，需到池塘装水！"));
                    }
                    
                    

                }
                else
                {
                    AudioController.instance.PlayAudio(SE.Fail);
                    InformationController.instance.AddInformation("*" + LanguageManage.SwitchStr("水量不足，需到池塘装水！"));
                }

            }
            else if (farmToolType == FarmToolType.种子)
            {
                
                if (farmTool.dataId != 0)
                {
                    if (cell.fieldStatus != FieldStatus.Barren && cell.myGameObjects.Count == 0)
                    {
                        if (GameComponentData.gameData.gameManager.CostRp(workRp))
                        {

                            if (seedobj == null)
                            {
                                seedobj = Instantiate(seedPro);
                            }
                           
                            seedobj.GetComponent<Seedaction>().InitSeed();
                            seedobj.transform.position = AStarTest.CoordinateToPos(cell.coordinate);

                            AudioController.instance.PlayAudio(SE.bo);
                            Field field = Fields.Find(f => f.coordinate == cell.coordinate);
                            GameComponentData.gameData.plantAction.Planting(cell, farmTool.dataId, field);
                            farmTool.count--;
                            SeedCounText.text = farmTool.count.ToString();
                            if (farmTool.count == 0)
                            {
                                farmTool = default(Item);
                                SeedCounText.enabled = false;
                                GameComponentData.gameData.gameManager.fieldTool.GetComponent<FarmToolAction>().DefaultSeedAction();
                            }

                        }

                    }
                }
               
            }
        }
          
    }
    public void InitGrassObj(Transform grassParent)
    {
        for (int i = 0; i < Grasses.Count; i++)
        {
            Grasses[i].InitCells();
            Grasses[i].Obj = grassParent.GetChild(i).gameObject;
            if (Grasses[i].isClear)
            {
                Grasses[i].ClearFarm();
                clearFieldNum++;
            }
        }

    }

    public void ClickGrass(GameObject obj)
    {
        if (DataSaveAndLoadTest.isJsonData)
        {
            Grass _grass = Grasses.Find(g => g.Obj == obj);
            ClearFieldAction(_grass);
        }
        else
        if (GameComponentData.gameData.guideController.nowGuide==null&&GameComponentData.gameData.filmManager.nowFilm==null)
        {
            Grass _grass = Grasses.Find(g => g.Obj == obj);
            ClearFieldAction(_grass);
        }
       
        

    }
    public void ClearFieldAction(Grass grass)
    {
        var fs = Grasses.FindAll(g => g.isClear);
        int costValue = clearZeroCost + clearAddCost * (fs.Count);
        clearGrass = grass;
        AudioController.instance.PlayAudio(SE.click);
        GameComponentData.gameData.gameManager.InitCostData(LanguageManage.SwitchStr("开发荒地"),costValue,LanguageManage.SwitchStr("开发一块荒地作为新农田？"),CostType.增加田地,ShopMoneyType.金币);
    }

    public void GrassClearAction()
    {
        clearFieldNum++;
        clearGrass.ClearFarm();

    }
	// Update is called once per frame
	void Update () {
		
	}
}
