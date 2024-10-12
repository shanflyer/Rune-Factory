using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Mathematics;

public class FarmManager : Singleton<FarmManager>
{
    private Dictionary<int, Field> fields = new Dictionary<int, Field>();
    //private Dictionary<int, Plant> plants = new Dictionary<int, Plant>();

    private Dictionary<int, List<FieldArea>> FieldAreas = new Dictionary<int, List<FieldArea>>();

    protected override void Clear()
    {
        fields.Clear();
        // plants.Clear();
        base.Clear();
    }

    public override async void Init()
    {
        base.Init();
        fields.Clear();
        // plants.Clear();

        var allFieldAreas = await GameDataManager.instance.GetAllAsyncData<FieldArea>();
        for (int i = 0; i < allFieldAreas.Count; i++)
        {
            var fieldArea = allFieldAreas[i];
            if (!FieldAreas.TryGetValue(fieldArea.mapId, out var fieldAreas))
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
        GameActionManager.instance.AddListener<SetWeather>(SetWeather);
    }
    void SetWeather(SetWeather setWeather)
    {
        if (setWeather.weather.waterFall > 0)
        {
            using(var e = fields.Values.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    e.Current.SetWaterField();
                    GameDataSaveManager.instance.UserGameSaveData.SetFieldData(e.Current);
                }
            }
        }
    }
    public void RefreshField(RefreshField refreshField)
    {
        if(fields.TryGetValue(refreshField.fieldId,out var field))
        {
            field.RefreshField();
        }
    }
    public async void CreatField(FieldSaveData fieldSaveData)
    {
        int instanceId = fieldSaveData.instanceId;
        if (!fields.ContainsKey(instanceId))
        {
            Field field = new Field
            {
                instanceId = instanceId,
                mapInstance = fieldSaveData.mapInstance,
                editorInstanceId = fieldSaveData.editorInstanceId,
                fieldState = fieldSaveData.fieldState,
                isSetWater = fieldSaveData.isSetWater,
                coordinate = fieldSaveData.coordinate,
               
            };
            if (fieldSaveData.PlantinstaceId != 0)
            {
                field.plant = new Plant
                {
                    instaceId = fieldSaveData.PlantinstaceId,
                    PlantData = await GameDataManager.instance.GetAsyncData<PlantData>(fieldSaveData.PlantDataId),
                    growthDay = fieldSaveData.growthDay,
                    growthStage = fieldSaveData.growthStage,
                    plantState = fieldSaveData.plantState,
                    nowCycle = fieldSaveData.nowCycle,
                };
                
                AddMapItem addMapItem = new AddMapItem
                {
                    dataId = field.plant.PlantData.mapItem,
                    coordinate = field.coordinate,
                    mapId = field.mapInstance,
                    instanceId = fieldSaveData.PlantinstaceId
                };
                GameActionManager.instance.QueueAction(addMapItem);
            } 
            fields.Add(instanceId, field); 
            //GameDataSaveManager.instance.UserGameSaveData.SetFieldData(field);
        }
    }
    private void TryCreatField(TryCreatField tryCreatField)
    {
        if (FieldAreas.TryGetValue(tryCreatField.roomId, out var fieldAreas))
        {
            for (int i = 0; i < fieldAreas.Count; i++)
            {
                if (fieldAreas[i].fields.Contains(tryCreatField.itemInstanceId))
                {
                    int2 editorKey = new int2(tryCreatField.roomId, tryCreatField.itemInstanceId);
                    int instanceId = WorldMapManager.instance.GetInstanceFromEditorId(editorKey);
                    if (!fields.ContainsKey(instanceId))
                    {
                        Field field = new Field
                        {
                            instanceId = instanceId,
                            mapInstance = tryCreatField.roomId,
                            coordinate = tryCreatField.coordinate,
                            editorInstanceId=tryCreatField.itemInstanceId,
                            fieldState = FieldState.待平整
                        };
                        fields.Add(instanceId, field);

                        GameDataSaveManager.instance.UserGameSaveData.SetFieldData(field);
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

    private void CheckFieldState(CheckFieldState checkFieldState)
    {
        if (fields.TryGetValue(checkFieldState.instanceid, out var field))
        {
            checkFieldState.setValue((int)field.fieldState);
        }
    }

    private void TrySmoothField(TrySmoothField TrySmoothField)
    {
        if (fields.TryGetValue(TrySmoothField.fieldId, out var field))
        {
            switch (field.fieldState)
            {
                case FieldState.待平整:
                    field.fieldState = FieldState.已平整;
                    TrySmoothField.setResult(true);
                    break;

                case FieldState.已平整:
                    if (field.plant != null)
                    {
                        if (field.plant.plantState != PlantState.枯死 || field.plant.plantState != PlantState.死亡)
                        {
                            GameManager.instance.ShowTwoSelectAction("注意", "土地上已有作物，是否铲除旧作物？", () =>
                            {
                                GameActionManager.instance.QueueAction(new DeleteMapItem
                                {
                                    mapItemInstanceId = field.plant.instaceId,
                                    triggerClear = true
                                });
                                field.plant = null;
                                field.fieldState = FieldState.已平整;
                                TrySmoothField.setResult(true);
                            }, () => { TrySmoothField.setResult(false); });
                        }
                        else
                        {
                            GameActionManager.instance.QueueAction(new DeleteMapItem
                            {
                                mapItemInstanceId = field.plant.instaceId,
                                triggerClear = true
                            });
                            field.plant = null;
                            field.fieldState = FieldState.已平整;
                            TrySmoothField.setResult(true);
                        }
                    }
                    else
                    {
                        field.fieldState = FieldState.已平整;
                        TrySmoothField.setResult(true);
                    }
                    break;
            }
            //RefreshField refreshField = new RefreshField { fieldId = field.instanceId };
            // RefreshField(refreshField);

            GameDataSaveManager.instance.UserGameSaveData.SetFieldData(field);
        }
        else
        {
            TrySmoothField.setResult(false);
        }
    }

    private async void TryCreatPlant(TryCreatPlant creatPlant)
    {
        PlantData plantData = await GameDataManager.instance.GetAsyncData<PlantData>(creatPlant.plantId);
        if (plantData == null)
        {
            creatPlant.setResult(false);
            return;
        }
        if (fields.TryGetValue(creatPlant.fieldId, out var field) &&
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
                Plant plant = new Plant
                {
                    instaceId = instanceId, 
                    PlantData = plantData,
                    field = creatPlant.fieldId,
                    plantState = PlantState.正常,
                    setWater = field.isSetWater
                };
                field.fieldState = FieldState.已平整;
                field.plant = plant;
                creatPlant.setResult(true);

                GameDataSaveManager.instance.UserGameSaveData.SetFieldData(field);
            }

            GameActionManager.instance.QueueAction(addMapItem);
        }
        else
        {
            creatPlant.setResult(false);
        }
    }

    private void SetWaterField(SetWaterField setWaterField)
    {
        if (fields.TryGetValue(setWaterField.fieldId, out var field))
        {
            field.SetWaterField();

            GameDataSaveManager.instance.UserGameSaveData.SetFieldData(field);
            setWaterField.setResult(true);
        }
        else
        {
            setWaterField.setResult(false);
        }
    }

    private void NewDay(NewDay newDay)
    {
        foreach (var data in fields)
        {
            data.Value.NewDay(); 
        }
    }

    private async void TryGetPlantFruit(TryGetPlantFruit tryGetPlantFruit)
    {
        if (fields.TryGetValue(tryGetPlantFruit.fieldId, out var field))
        {
            bool result = await field.TryGetPlantFruit();
            tryGetPlantFruit.setResult(result); 
        }
        tryGetPlantFruit.setResult(false);
    }
}

public enum FieldState
{
    待平整, 已平整
}

public class Field
{
    public int instanceId;
    public int mapInstance;
    public int editorInstanceId;
    public int2 coordinate;
    public FieldState fieldState;
    public bool isSetWater;
    public Plant plant;

    public async Task<bool> TryGetPlantFruit()
    {
        if (plant != null)
        {
            bool result = await plant.GetPlantFruit();
            if (result && plant.plantState == PlantState.死亡)
            {
                /* field.fieldState = FieldState.待平整;

              field.plantId = 0;

              DeleteMapItem deleteMapItem = new DeleteMapItem
              {
                  mapItemInstanceId = plant.instaceId,
                  triggerClear = true
              };
              GameActionManager.instance.QueueAction(deleteMapItem);

              plants.RemoveData(plant.Key);*/
            }
            else
            {
                PlantData plantData = plant.PlantData;
                plant.nowCycle++;
                if (plant.nowCycle >= plantData.pickTimes)
                {
                    plant.plantState = PlantState.死亡;
                    /*
                    field.fieldState = FieldState.待平整;
                    field.plantId = 0;

                    DeleteMapItem deleteMapItem = new DeleteMapItem
                    {
                        mapItemInstanceId = plant.instaceId,
                        triggerClear = true
                    };
                    GameActionManager.instance.QueueAction(deleteMapItem);
                    plants.RemoveData(plant.Key);*/
                }
                else
                {
                    plant.plantState = PlantState.正常;
                    plant.growthStage = plantData.cycleStage;
                }
            }
            plant.RefreshPlant();
            RefreshField();

            GameDataSaveManager.instance.UserGameSaveData.SetFieldData(this);
            return result;
        }
        return false;
    }

    public void RefreshField()
    {
        SetItemAnimation setItemAnimation = new SetItemAnimation
        {
            keyX = (int)fieldState,
            keyY = isSetWater ? 1 : 0,
            id = instanceId,
        };
        GameActionManager.instance.QueueAction(setItemAnimation);
    }

    public void NewDay()
    {
        isSetWater = false;
        if (plant != null)
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
                    plant = null;
                    fieldState = FieldState.待平整;
                    break;
            }

            if (plant != null)
            {
                plant.setWater = false;
                plant.RefreshPlant();
            }
        }
        RefreshField();

        GameDataSaveManager.instance.UserGameSaveData.SetFieldData(this);
    }

    public void SetWaterField()
    {
        isSetWater = true;
        if (plant != null)
        {
            plant.setWater = true;
            switch (plant.plantState)
            {
                case PlantState.正常:
                case PlantState.干旱:
                    plant.plantState = PlantState.正常;
                    //setWaterField.setResult(true);
                    break;

                case PlantState.枯死:
                    // GameNotificationManager.instance.DisplayTips("提示", "植物已经死亡，请先铲除!");
                    // setWaterField.setResult(false);
                    break;

                case PlantState.死亡:
                    //GameNotificationManager.instance.DisplayTips("提示", "请先平整土地!");
                    // setWaterField.setResult(false);
                    break;

                case PlantState.成熟:
                    //setWaterField.setResult(true);
                    break;
            }
            plant.RefreshPlant();
        }
    }
}

public enum PlantState
{
    正常 = 0, 干旱 = 1, 枯死 = 2, 死亡 = 3, 成熟 = 4
}

public class Plant
{ 
    public int instaceId;
    public int field;
    public PlantData PlantData;
    public int growthStage;
    public int growthDay;
    public bool setWater;
    public PlantState plantState;
    public int nowCycle;

    public int Key => instaceId;

    public void RefreshPlant()
    {
        int keyY = 0;
        switch (plantState)
        {
            case PlantState.正常:
            case PlantState.成熟:
                keyY = 0;
                break;

            case PlantState.干旱:
                keyY = 1;
                break;

            case PlantState.枯死:
            case PlantState.死亡:
                keyY = 2;
                break;
        }

        SetItemAnimation setItemAnimation = new SetItemAnimation
        {
            keyX = growthStage,
            keyY = keyY,
            id = instaceId
        };
        GameActionManager.instance.QueueAction(setItemAnimation);
    }

    public void Grow()
    {
        if (plantState == PlantState.死亡 || plantState == PlantState.枯死)
        {
            return;
        }
        growthDay++;
        var growthStateData = PlantData.growthStages[growthStage];
        if (growthDay >= growthStateData.growthDay)
        {
            int index = growthStage + 1;
            if (index < PlantData.growthStages.Count - 1)
            {
                growthStage = index;
                growthDay = 0;
            }
            else if (index >= PlantData.growthStages.Count)
            {
                growthStage = PlantData.cycleStage;
            }
            if (index == PlantData.growthStages.Count - 1)
            {
                plantState = PlantState.成熟;
                AddMapItemOperate addMapItemOperate = new AddMapItemOperate
                {
                    mapItemId = field,
                    addeOperateId = 22
                };
                GameActionManager.instance.QueueAction(addMapItemOperate);
            }
        }
    }

    public async Task<bool> GetPlantFruit()
    {
        if (plantState == PlantState.成熟)
        {
            if (await PackageManager.instance.CheckPackageTryItemIn(CharacterManager.instance.controllerCharacter.characterPackage,
                PlantData.fruit, PlantData.fruitCount))
            {
                Item item = new Item
                {
                    dataId = PlantData.fruit,
                    count = PlantData.fruitCount
                };

                if (nowCycle >= PlantData.pickTimes)
                {
                    plantState = PlantState.死亡;
                }
                else
                {
                    growthStage++;
                    growthDay = 0;
                    nowCycle++;
                }

                await PackageManager.instance.SetItemInPackage(item, CharacterManager.instance.controllerCharacter.characterPackage);
                return true;
            }

            return false;
        }
        return false;
    }

    public void Dispose()
    {
    }
}