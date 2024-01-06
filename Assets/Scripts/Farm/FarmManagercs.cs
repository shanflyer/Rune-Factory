 
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

    Dictionary<int,List<FieldArea>> FieldAreas = new Dictionary<int, List<FieldArea>>();
    protected override void Clear()
    {
        base.Clear();
        fields.Dispose();
        plants.Dispose();
    }
    public override async void Init()
    {
        base.Init();
        fields.Init(16);
        plants.Init(16);

        var allFieldAreas=await GameDataManager.instance.GetAllAsyncData<FieldArea>();
        for(int i = 0; i < allFieldAreas.Count; i++)
        {
            var fieldArea = allFieldAreas[i];
            if(!FieldAreas.TryGetValue(fieldArea.mapId,out var fieldAreas))
            {
                fieldAreas = new List<FieldArea>();
                FieldAreas[fieldArea.mapId] = fieldAreas;
            }
            fieldAreas.Add(fieldArea); 
        }
        GameActionManager.instance.AddListener<TryCreatField>(TryCreatField);
        GameActionManager.instance.AddListener<CheckFieldState>(CheckFieldState);
        GameActionManager.instance.AddListener<TrySmoothField>(TrySmoothField);
        GameActionManager.instance.AddListener<TryCreatPlant>(TryCreatPlant);
        GameActionManager.instance.AddListener<SetWaterField>(SetWaterField);
        GameActionManager.instance.AddListener<NewDay>(NewDay);
        GameActionManager.instance.AddListener<TryGetPlantFruit>(TryGetPlantFruit);
        GameActionManager.instance.AddListener<RefreshField>(RefreshField);
        GameActionManager.instance.AddListener<RefreshPlant>(RefreshPlant);
    }
    void RefreshPlant(RefreshPlant refreshPlant)
    { 
        if (plants.GetData(refreshPlant.plantId, out var plant))
        {
            SetItemAnimation setItemAnimation = new SetItemAnimation
            {
                keyX=plant.growthStage,
                keyY = (int)plant.plantState,
                id=plant.instaceId
            };
            GameActionManager.instance.QueueAction(setItemAnimation);
        }
    }
    void RefreshField(RefreshField refreshField)
    { 
        if(fields.GetData(refreshField.fieldId, out var field))
        {
            SetItemAnimation setItemAnimation = new SetItemAnimation
            {
                keyX = (int)field.fieldState,
                id = refreshField.fieldId, 
            };
            GameActionManager.instance.QueueAction(setItemAnimation);
        }
    }
    void TryCreatField(TryCreatField tryCreatField)
    { 
        
        if(FieldAreas.TryGetValue(tryCreatField.roomId,out var fieldAreas))
        {
            for(int i = 0; i < fieldAreas.Count; i++)
            {
                if (fieldAreas[i].fields.Contains(tryCreatField.itemInstanceId))
                {
                    int2 editorKey = new int2(tryCreatField.roomId, tryCreatField.itemInstanceId);
                    int instanceId = WorldMapManager.instance.GetInstanceFromEditorId(editorKey);
                    if (!fields.Contains(instanceId))
                    {
                        Field field = new Field
                        {
                            instanceId = instanceId,
                            fieldState = FieldState.待平整
                        };
                        fields.SetData(field);
                    }
                }
            }
            /*
            if(fieldArea.open)
            {
                for (int i = 0; i < fieldArea.fields.Count; i++)
                {
                    int2 editorKey = new int2(tryCreatField.roomId,fieldArea.fields[i]);
                    int instanceId = WorldMapManager.instance.GetInstanceFromEditorId(editorKey);
                    if (!fields.Contains(instanceId))
                    {
                        Field field = new Field
                        {
                            instanceId = instanceId,
                            fieldState = FieldState.待平整
                        };
                        fields.SetData(field);
                    }
                }
            }*/
        }
    }
    void CheckFieldState(CheckFieldState checkFieldState)
    {
        if (fields.GetData(checkFieldState.instanceid, out var field))
        {
            checkFieldState.setValue((int)field.fieldState);
        }
    }
    void TrySmoothField(TrySmoothField TrySmoothField)
    {
        if (fields.GetData(TrySmoothField.fieldId, out var field))
        {
            switch (field.fieldState)
            {
                case FieldState.待平整:
                    field.fieldState = FieldState.已平整;
                    TrySmoothField.setResult(true);
                    break;
                case FieldState.已平整:
                   // GameNotificationManager.instance.DisplayTips("", "土地已经平整完毕");
                    TrySmoothField.setResult(false);
                    break;
                case FieldState.已栽种:
                    GameManager.instance.ShowTwoSelectAction("注意", "土地上已有作物，是否铲除旧作物？", () =>
                    {
                        plants.RemoveData(field.Key);
                        field.plantId = 0;
                        field.fieldState = FieldState.已平整;
                        TrySmoothField.setResult(true);
                    }, null);
                    break;
            }
            fields.SetData(field);
            //RefreshField refreshField = new RefreshField { fieldId = field.instanceId };
           // RefreshField(refreshField);
        }
        else
        {
            TrySmoothField.setResult(false);
        }
     
    }
    async void TryCreatPlant(TryCreatPlant creatPlant)
    {
        PlantData plantData = await GameDataManager.instance.GetAsyncData<PlantData>(creatPlant.plantId);
        if(plantData == null)
        {
            creatPlant.setResult(false);
            return;
        } 
        if(fields.GetData(creatPlant.fieldId, out var field)&&
            WorldMapManager.instance.GetMapItemPos(field.instanceId, out var coordinate))
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
                creatPlant.setResult(true); 
            }

            GameActionManager.instance.QueueAction(addMapItem);
        }
        else
        {
            creatPlant.setResult(false);
        }
       
    }
    void SetWaterField(SetWaterField setWaterField)
    {
        if (fields.GetData(setWaterField.fieldId, out var field))
        {
            field.isSetWater = true; 
            if (plants.GetData(setWaterField.fieldId, out var plant))
            {
                plant.setWater= true;
                switch (plant.plantState)
                {
                    case PlantState.正常:
                    case PlantState.干旱:
                        plant.plantState = PlantState.正常;
                        setWaterField.setResult(true);
                        break;
                    case PlantState.枯死:
                        GameNotificationManager.instance.DisplayTips("提示", "植物已经死亡，请先铲除!");
                        setWaterField.setResult(false);
                        break;
                    case PlantState.死亡:
                        GameNotificationManager.instance.DisplayTips("提示", "请先平整土地!");
                        setWaterField.setResult(false);
                        break;
                    case PlantState.成熟:
                        setWaterField.setResult(true);
                        break; 
                }
                plants.SetData(plant);
                RefreshPlant refreshPlant = new RefreshPlant
                { plantId = plant.instaceId};
                RefreshPlant(refreshPlant);
            } 
            fields.SetData(field);
            RefreshField refreshField = new RefreshField { fieldId = field.instanceId};
            RefreshField(refreshField);
        }
        else
        {
            setWaterField.setResult(false);
        }
    }
    void NewDay(NewDay newDay)
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
                        DeleteMapItem deleteMapItem = new DeleteMapItem
                        {
                            mapItemInstanceId = plant.instaceId,
                            triggerClear = true
                        };
                        GameActionManager.instance.QueueAction(deleteMapItem);
                        plants.RemoveData(data.plantId);
                        field.plantId = 0;
                        field.fieldState = FieldState.待平整;
                        break;
                }
                plant.setWater = false;
                plants.SetData(plant);

                RefreshPlant refreshPlant = new RefreshPlant
                { plantId = plant.instaceId };
                RefreshPlant(refreshPlant);
            }
            fields.SetData(field);
            RefreshField refreshField = new RefreshField { fieldId = field.instanceId };
            RefreshField(refreshField);
        }
    }
    async void TryGetPlantFruit(TryGetPlantFruit tryGetPlantFruit)
    { 
        if(fields.GetData(tryGetPlantFruit.fieldId, out var field))
        {
            if(plants.GetData(field.plantId,out var plant))
            {
                bool result = await plant.GetPlantFruit();
                if (result&&plant.plantState==PlantState.死亡)
                {
                    field.fieldState = FieldState.待平整;
                    field.plantId = 0;

                    DeleteMapItem deleteMapItem = new DeleteMapItem
                    {
                        mapItemInstanceId = plant.instaceId,
                        triggerClear = true
                    };
                    GameActionManager.instance.QueueAction(deleteMapItem);
                   // RefreshPlant refreshPlant = new RefreshPlant
                   //{ plantId = plant.instaceId };
                   // RefreshPlant(refreshPlant);

                    plants.RemoveData(plant.Key);
                }else
                {
                    PlantData plantData = await GameDataManager.instance.GetAsyncData<PlantData>(plant.dataId);
                    plant.nowCycle++;
                    if(plant.nowCycle>= plantData.pickTimes)
                    {
                        plant.plantState = PlantState.死亡;
                        field.fieldState = FieldState.待平整;
                        field.plantId = 0;

                        DeleteMapItem deleteMapItem = new DeleteMapItem
                        {
                            mapItemInstanceId = plant.instaceId,
                            triggerClear = true
                        };
                        GameActionManager.instance.QueueAction(deleteMapItem);
                        plants.RemoveData(plant.Key);
                    }
                    else
                    {
                        plant.plantState = PlantState.正常;
                        plant.growthStage = plantData.cycleStage;

                        RefreshPlant refreshPlant = new RefreshPlant
                        { plantId = plant.instaceId };
                        RefreshPlant(refreshPlant);
                    }
                }
                

                tryGetPlantFruit.setResult(result); 

                fields.SetData(field);
                RefreshField refreshField = new RefreshField { fieldId = field.instanceId };
                RefreshField(refreshField); 

                return;
            }
        }
        tryGetPlantFruit.setResult(false);
    }
}
public enum FieldState 
{
    待平整,已平整,已栽种
}
public struct Field : INativeData
{ 
    public int instanceId;
    public FieldState fieldState;
    public bool isSetWater;
    public int plantId;
    public void Dispose()
    {
    }
    public int Key => instanceId;
}
public enum PlantState 
{
    正常=0,干旱=1,枯死=2,死亡=3,成熟=4
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

    public int Key => instaceId;

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

    public void Dispose()
    {
    }
}