
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;

public class FarmManager:Singleton<FarmManager>
{ 
    MyNativeData<Field> fields=new MyNativeData<Field>();
    MyNativeData<Plant> plants = new MyNativeData<Plant>();

    Dictionary<int2, FieldArea> FieldAreas = new Dictionary<int2, FieldArea>();
    public override async void Init()
    {
        base.Init();
        var allFieldAreas=await GameDataManager.instance.GetAllAsyncData<FieldArea>();
        for(int i = 0; i < allFieldAreas.Count; i++)
        {
            var fieldArea = allFieldAreas[i];
            FieldAreas.Add(new int2(fieldArea.mapId, fieldArea.linkItem), fieldArea);
        }
        GameActionManager.instance.AddListener<TryCreatField>(TryCreatField);
        GameActionManager.instance.AddListener<CheckFieldState>(CheckFieldState);
        GameActionManager.instance.AddListener<TrySmoothField>(TrySmoothField);
    } 
    void TryCreatField(TryCreatField tryCreatField)
    {
        int2 key = new int2(tryCreatField.roomId, tryCreatField.itemInstanceId);
        if(FieldAreas.TryGetValue(key,out var fieldArea))
        {
            if(fieldArea.open)
            {
                for (int i = 0; i < fieldArea.fields.Count; i++)
                {
                    int2 id = new int2(tryCreatField.roomId,fieldArea.fields[i]);
                    if (!fields.Contains(id))
                    {
                        Field field = new Field
                        {
                            mapId=id.x,
                            instanceId = id.y,
                            fieldState = FieldState.待平整
                        };
                        fields.SetData(field);
                    }
                }
            }
        }
    }
    void CheckFieldState(CheckFieldState checkFieldState)
    {
        if (fields.GetData(new int2(checkFieldState.mapId,checkFieldState.instanceid), out var field))
        {
            checkFieldState.setValue((int)field.fieldState);
        }
    }
    void TrySmoothField(TrySmoothField TrySmoothField)
    {
        if (fields.GetData(new int2(TrySmoothField.mapId,TrySmoothField.fieldId), out var field))
        {
            switch (field.fieldState)
            {
                case FieldState.待平整:
                    field.fieldState = FieldState.已平整;
                    TrySmoothField.setResult(true);
                    break;
                case FieldState.已平整:
                    GameNotificationManager.instance.DisplayTips("", "土地已经平整完毕");
                    TrySmoothField.setResult(false);
                    break;
                case FieldState.已栽种:
                    GameManager.instance.ShowTwoSelectAction("注意", "土地上已有作物，是否铲除旧作物？", () =>
                    {
                        plants.RemoveData(field.plantId);
                        field.fieldState = FieldState.已平整;
                        TrySmoothField.setResult(true);
                    }, null);
                    break;
            }
            fields.SetData(field);
        }
    }

    async void CreatPlant(TryCreatPlant creatPlant)
    {
        PlantData plantData = await GameDataManager.instance.GetAsyncData<PlantData>(creatPlant.plantId);
        if(plantData == null)
        {
            creatPlant.setResult(false);
            return;
        }
        int2 key = new int2(creatPlant.mapId, creatPlant.fieldId);
        if(fields.GetData(key,out var field)&&
            WorldMapManager.instance.GetMapItemPos(field.mapId, field.instanceId, out var coordinate))
        { 
            AddMapItem addMapItem = new AddMapItem
            {
                dataId = plantData.mapItem,
                coordinate = coordinate.xy,
                mapId = coordinate.z,
                setValue = SetPlantInstanceId
            };
            void SetPlantInstanceId(int instanceId)
            {
                field.plantId = instanceId;
                fields.SetData(field);

                Plant plant = new Plant
                {
                    instaceId = instanceId,
                    mapId= coordinate.z,
                    dataId = instanceId,
                    field = creatPlant.fieldId,
                    plantState = PlantState.正常,
                    setWater = field.isSetWater
                };
                plants.SetData(plant);
                fields.SetData(field);
            }
        }
        creatPlant.setResult(false);
    }

    async void CreatPlant(int plantId,int mapId,int2 coordinate,int fieldId)
    {
        PlantData plantData = await GameDataManager.instance.GetAsyncData<PlantData>(plantId);
        AddMapItem addMapItem = new AddMapItem
        {
            dataId = plantData.mapItem,
            coordinate = coordinate,
            mapId = mapId,
            setValue=SetPlantInstanceId
        };
        void SetPlantInstanceId(int instanceId)
        {
            if (fields.GetData(fieldId, out var field))
            {
                field.plantId = instanceId;
                fields.SetData(field);

                Plant plant = new Plant
                {
                    instaceId = instanceId,
                    dataId = plantId,
                    field = fieldId,
                    plantState = PlantState.正常,
                    setWater=field.isSetWater
                };
                plants.SetData(plant);
                fields.SetData(field);
            } 
        }
    }
    void SetWaterField(int fieldId)
    {
        if (fields.GetData(fieldId, out var field))
        {
            field.isSetWater = true;
            if(plants.GetData(field.plantId,out var plant))
            {
                plant.setWater= true;
                if (plant.plantState == PlantState.干旱)
                {
                    plant.plantState = PlantState.正常;
                }
                plants.SetData(plant);
            } 
            fields.SetData(field);
        }
    }
    void NewDay()
    {
        foreach(Field data in fields)
        {
            var field = data;
            field.isSetWater = false;
            if (plants.GetData(field.plantId, out var plant))
            { 
                switch (plant.plantState)
                {
                    case PlantState.正常:
                        if (!plant.setWater)
                        {
                            plant.plantState = PlantState.干旱;
                        }
                        else
                        {
                            plant.Grow();
                        }
                        break;
                    case PlantState.干旱:
                        plant.plantState = PlantState.枯死;
                        break; 
                    case PlantState.死亡:
                        plants.RemoveData(data.plantId);
                        break;
                }
                plant.setWater = false;
                plants.SetData(plant);
            }
            fields.SetData(field);
        }
    }
}
public enum FieldState 
{
    待平整,已平整,已栽种
}
public struct Field : INativeData
{
    public int mapId;
    public int instanceId;
    public FieldState fieldState;
    public bool isSetWater;
    public int plantId;

    public int2 Key => new int2(mapId,instanceId);
}
public enum PlantState 
{
    正常,干旱,枯死,死亡,成熟
}
public struct Plant:INativeData
{
    public int mapId;
    public int instaceId;
    public int field;
    public int dataId;
    public int growthStage;
    public int growthDay;
    public bool setWater;
    public PlantState plantState;
    public int nowCycle;

    public int2 Key => new int2(mapId,instaceId);

    public async void Grow()
    {
        if (plantState == PlantState.死亡 || plantState == PlantState.枯死)
        {
            return;
        }
        growthDay++;
        PlantData plantData = await GameDataManager.instance.GetAsyncData<PlantData>(dataId);
        var growthStateData=plantData.growthStages[growthStage];
        if (growthDay >= growthStateData.growthDay)
        {
            int index = growthStage+1;
            if (index < plantData.growthStages.Count-1)
            {
                growthStage = index;
                growthDay = 0;
            }
            if(index== plantData.growthStages.Count - 1)
            {
                plantState = PlantState.成熟;
            }
        }
    }
    public async Task<bool> GetPlantFruit()
    {
        if (plantState == PlantState.成熟)
        {
            PlantData plantData = await GameDataManager.instance.GetAsyncData<PlantData>(dataId);
            
            if(await PackageManager.instance.CheckPackageTryItemIn(CharacterManager.instance.controllerCharacter.characterPackage,
                plantData.fruit, plantData.fruitCount))
            {
                Item item = new Item
                {
                    dataId = plantData.fruit,
                    count = plantData.fruitCount
                };

               
                if (nowCycle >= plantData.cycleStage)
                {
                    plantState = PlantState.死亡;
                }
                else
                {
                    growthStage++;
                    growthDay = 0;
                    nowCycle++;
                }

                PackageManager.instance.SetItemInPackage(item, CharacterManager.instance.controllerCharacter.characterPackage);
                return true;
            }

            return false;
        }
        return false ;
    } 
}