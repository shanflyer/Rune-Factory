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
        GameActionManager.instance.AddListener<NewHour>(NewHour);
        GameActionManager.instance.AddListener<TryGetPlantFruit>(TryGetPlantFruit);
        GameActionManager.instance.AddListener<RefreshField>(RefreshField);
        GameActionManager.instance.AddListener<SetWeather>(SetWeather);
        GameActionManager.instance.AddListener<TrySicklePlant>(TrySicklePlant);
        GameActionManager.instance.AddListener<CheckPlant>(CheckPlant);
        GameActionManager.instance.AddListener<ChangeMapRoom>(ChangeMapRoom);
    }

    private void ChangeMapRoom(ChangeMapRoom changeMapRoom)
    {
        foreach (var field in fields.Values)
        {
            if (field.plant != null && field.mapInstance == changeMapRoom.newRoom)
            {
                field.plant.RefreshPlant();
            }
        }
    }

    private void SetWeather(SetWeather setWeather)
    {
        if (setWeather.weather.waterFall > 0)
        {
            using (var e = fields.Values.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    e.Current.SetWaterField();
                }
            }
        }
    }

    public void RefreshField(RefreshField refreshField)
    {
        if (fields.TryGetValue(refreshField.fieldId, out var field))
        {
            field.RefreshField();
        }
    }

    public async void CreatField(FieldSaveData fieldSaveData)
    {
        int instanceId = fieldSaveData.instanceId;
        if (!fields.TryGetValue(instanceId, out var field))
        {
            field = new Field
            {
                instanceId = instanceId,
                mapInstance = fieldSaveData.mapInstance,
                editorInstanceId = fieldSaveData.editorInstanceId,
                fieldState = fieldSaveData.fieldState,
                isSetWater = fieldSaveData.isSetWater,
                coordinate = fieldSaveData.coordinate,
                waterHour = fieldSaveData.waterHour
            };
            var PlantData = await GameDataManager.instance.GetAsyncData<PlantData>(fieldSaveData.PlantDataId);
            if (fieldSaveData.PlantinstaceId != 0)
            {
                field.plant = new Plant(fieldSaveData.PlantinstaceId, PlantData, field.instanceId, field.isSetWater, fieldSaveData.plantState, fieldSaveData.nowCycle);

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
            field.RefreshField();
            //GameDataSaveManager.instance.UserGameSaveData.SetFieldData(field);
        }
        else
        {
            if (fieldSaveData.PlantinstaceId != 0)
            {
                field.CreatePlant(fieldSaveData.PlantinstaceId, fieldSaveData.PlantDataId, fieldSaveData.growthHour, fieldSaveData.growthStage,
                    fieldSaveData.plantState, fieldSaveData.nowCycle, fieldSaveData.isSetWater);
            }
            field.SetData(fieldSaveData.fieldState, fieldSaveData.isSetWater, fieldSaveData.waterHour);
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
                    GameDataSaveManager.instance.UserGameSaveData.SaveSpecialMapItem(editorKey, instanceId);
                    if (!fields.ContainsKey(instanceId))
                    {
                        Field field = new Field
                        {
                            instanceId = instanceId,
                            mapInstance = tryCreatField.roomId,
                            coordinate = tryCreatField.coordinate,
                            editorInstanceId = tryCreatField.itemInstanceId,
                            fieldState = FieldState.待平整
                        };
                        fields.Add(instanceId, field);
                        if (!tryCreatField.noSaveRefresh)
                        {
                            field.RefreshField();
                        }
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

    private void CheckPlant(CheckPlant checkPlant)
    {
        if (fields.TryGetValue(checkPlant.instanceId, out var field))
        {
            if (checkPlant.setResult != null)
            {
                checkPlant.setResult(field.plant != null);
            }
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
                                    mapItemInstanceId = field.plant.instanceId,
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
                                mapItemInstanceId = field.plant.instanceId,
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

            field.RefreshField();
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
                Plant plant = new Plant(instanceId, plantData, field.instanceId, field.isSetWater);
                field.fieldState = FieldState.已平整;
                field.plant = plant;
                creatPlant.setResult(true);
                field.RefreshField();
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
            field.SetWaterField(true);

            setWaterField.setResult(true);
        }
        else
        {
            setWaterField.setResult(false);
        }
    }

    private void NewDay(NewDay newDay)
    {
        /*
        foreach (var data in fields)
        {
            data.Value.NewDay();
        }*/
    }

    private void NewHour(NewHour newHour)
    {
        foreach (var data in fields)
        {
            data.Value.NewHour();
        }
    }

    private async void TrySicklePlant(TrySicklePlant trySicklePlant)
    {
        if (fields.TryGetValue(trySicklePlant.fieldId, out var field))
        {
            bool result = await field.TrySicklePlant();
            if (trySicklePlant.setResult != null)
                trySicklePlant.setResult(result);
        }
        if (trySicklePlant.setResult != null)
            trySicklePlant.setResult(false);
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

    public Plant plant
    {
        get => _plant;
        set
        {
            _plant = value;
            RefreshField();
        }
    }

    public int waterHour;

    private Plant _plant;

    public void SetData(FieldState fieldState, bool isSetWater, int waterHour)
    {
        this.fieldState = fieldState;
        this.isSetWater = isSetWater;
        this.waterHour = waterHour;
        RefreshField();
    }

    public async void CreatePlant(int instanceId, int dataId, float growthHour, int growthStage, PlantState plantState, int nowCycle, bool setWater)
    {
        var PlantData = await GameDataManager.instance.GetAsyncData<PlantData>(dataId);
        _plant = new Plant(instanceId, PlantData, this.instanceId, setWater, plantState, nowCycle);
        plant.growthHour = growthHour;
        plant.growthStage = growthStage;

        AddMapItem addMapItem = new AddMapItem
        {
            dataId = dataId,
            coordinate = coordinate,
            mapId = mapInstance,
            fixeInstanceId = _plant.instanceId,
            setResult = (bool result) =>
            {
                plant.RefreshPlant();
            }
        };
        GameActionManager.instance.QueueAction(addMapItem, true);
    }

    public async Task<bool> TrySicklePlant()
    {
        if (plant != null)
        {
            bool result = await plant.GetSicklePlant();

            plant.RefreshPlant();
            GameActionManager.instance.QueueAction(new DeleteMapItem
            {
                mapItemInstanceId = plant.instanceId,
                triggerClear = true
            });
            plant = null;

            RefreshField();
            return result;
        }
        return false;
    }

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
            return result;
        }
        return false;
    }

    public void RefreshField()
    {
        List<int> Operates = new List<int>();
        switch (fieldState)
        {
            case FieldState.待平整:
                Operates.Add(GameCommon.SmoothField);
                Operates.Add(GameCommon.Watering);
                break;

            case FieldState.已平整:
                //Operates.Add(GameCommon.SmoothField);
                Operates.Add(GameCommon.Watering);
                if (plant == null)
                {
                    Operates.Add(GameCommon.Seeding);
                }
                else
                {
                    switch (plant.plantState)
                    {
                        case PlantState.成熟:
                            Operates.Add(GameCommon.Harvesting);
                            Operates.Add(GameCommon.Reaping);
                            break;

                        case PlantState.干旱:
                        case PlantState.正常:
                            Operates.Add(GameCommon.Eradicate);
                            if (plant.growthStage >= 2)
                                Operates.Add(GameCommon.Reaping);
                            break;

                        case PlantState.枯死:
                            Operates.Add(GameCommon.Eradicate);
                            if (plant.growthStage >= 2)
                                Operates.Add(GameCommon.Reaping);
                            break;

                        case PlantState.死亡:
                            Operates.Add(GameCommon.SmoothField);
                            break;
                    }
                }
                break;
        }

        ResetOperateData resetOperateData = new ResetOperateData
        {
            mapItemInstanceId = instanceId,
            operates = Operates
        };
        GameActionManager.instance.QueueAction(resetOperateData);

        SetItemAnimation setItemAnimation = new SetItemAnimation
        {
            keyX = (int)fieldState,
            keyY = isSetWater ? 1 : 0,
            id = instanceId,
        };
        GameActionManager.instance.QueueAction(setItemAnimation);

        GameDataSaveManager.instance.UserGameSaveData.SetFieldData(this);
    }

    public void NewHour()
    {
        if (EnvironmentManger.instance.nowWaterFall > 0)
        {
            waterHour = 0;
        }
        if (waterHour >= (!isSetWater ? 12 : 24))
        {
            waterHour = 0;
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
                        break;

                    case PlantState.干旱:
                        plant.plantState = PlantState.枯死;
                        break;

                    case PlantState.死亡:
                        DeleteMapItem deleteMapItem = new DeleteMapItem
                        {
                            mapItemInstanceId = plant.instanceId,
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
            if (plant != null && plant.plantState == PlantState.正常)
            {
                plant.Grow();
            }
            RefreshField();
        }
        else
        {
            waterHour++;
            if (plant != null && plant.plantState == PlantState.正常 && plant.setWater)
            {
                plant.Grow();
            }
            RefreshField();
        }
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
                    break;

                case PlantState.干旱:
                    plant.plantState = PlantState.枯死;
                    break;

                case PlantState.死亡:
                    DeleteMapItem deleteMapItem = new DeleteMapItem
                    {
                        mapItemInstanceId = plant.instanceId,
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
    }

    public void SetWaterField(bool isNotify = false)
    {
        waterHour = 0;
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
                    if (isNotify)
                        GameNotificationManager.instance.DisplayTips("提示", "植物已经死亡，请铲除!");
                    //setWaterField.setResult(false);
                    break;

                case PlantState.死亡:
                    if (isNotify)
                        GameNotificationManager.instance.DisplayTips("提示", "请平整土地!");
                    // setWaterField.setResult(false);
                    break;

                case PlantState.成熟:
                    //setWaterField.setResult(true);
                    break;
            }
            plant.RefreshPlant();
        }
        RefreshField();
    }
}

public enum PlantState
{
    正常 = 0, 干旱 = 1, 枯死 = 2, 死亡 = 3, 成熟 = 4
}

public class Plant
{
    public int instanceId;
    public int field;
    public PlantData PlantData;
    public HashSet<Season> goodSeason = new HashSet<Season>();
    public HashSet<Season> badSeason = new HashSet<Season>();
    public int growthStage;

    //public int growthDay;
    public float growthHour;

    public bool setWater;
    public PlantState plantState;
    public int nowCycle;

    public Plant(int instanceId, PlantData PlantData, int field, bool setWater, PlantState plantState = PlantState.正常, int nowCycle = 0)
    {
        this.instanceId = instanceId;
        this.PlantData = PlantData;
        this.field = field;
        this.setWater = setWater;
        this.plantState = plantState;
        this.nowCycle = nowCycle;
        for (int i = 0; i < PlantData.goodSeason.Count; i++)
        {
            goodSeason.Add((Season)PlantData.goodSeason[i]);
        }
        for (int i = 0; i < PlantData.badSeason.Count; i++)
        {
            badSeason.Add((Season)PlantData.badSeason[i]);
        }
    }

    public int Key => instanceId;

    public void RefreshPlant()
    {
        int keyY = 0;

        bool needShowDryEmote = false;
        GameActionManager.instance.QueueAction(new TryRecycleItemEmote { id = instanceId });
        switch (plantState)
        {
            case PlantState.正常:
                keyY = setWater ? 3 : 0;
                needShowDryEmote = !setWater;
                break;

            case PlantState.成熟:
                keyY = setWater ? 3 : 0;
                TryUpDataItemEmote tryUpDataItemEmote = new TryUpDataItemEmote
                {
                    id = instanceId,
                    showTime = -1,
                    emote = GameCommon.fritEmote
                };
                GameActionManager.instance.QueueAction(tryUpDataItemEmote);
                break;

            case PlantState.干旱:
                needShowDryEmote = true;
                keyY = 1;
                break;

            case PlantState.枯死:
            case PlantState.死亡:
                keyY = 2;
                tryUpDataItemEmote = new TryUpDataItemEmote
                {
                    id = instanceId,
                    showTime = -1,
                    emote = GameCommon.plantDeath
                };
                GameActionManager.instance.QueueAction(tryUpDataItemEmote);
                break;
        }
        if (needShowDryEmote)
        {
            TryUpDataItemEmote tryUpDataItemEmote = new TryUpDataItemEmote
            {
                id = instanceId,
                showTime = -1,
                emote = GameCommon.dryPlantEmote
            };
            GameActionManager.instance.QueueAction(tryUpDataItemEmote);
        }
        SetItemAnimation setItemAnimation = new SetItemAnimation
        {
            keyX = growthStage,
            keyY = keyY,
            id = instanceId
        };
        GameActionManager.instance.QueueAction(setItemAnimation);
    }

    public void Grow()
    {
        if (goodSeason.Contains(GameTimeManager.instance.Season))
        {
            growthHour = growthHour + 2;
        }
        else if (badSeason.Contains(GameTimeManager.instance.Season))
        {
            growthHour = growthHour + 0.5f;
        }
        else
        {
            growthHour = growthHour + 1;
        }

        var growthStateData = PlantData.growthStages[growthStage];
        if (growthHour >= growthStateData.growthHour)
        {
            int index = growthStage + 1;
            if (index < PlantData.growthStages.Count - 1)
            {
                growthStage = index;
                growthHour = 0;
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
        RefreshPlant();
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
                    growthHour = 0;
                    nowCycle++;
                }
                GameDataSaveManager.instance.SetPlantFruitCount(PlantData.id, PlantData.fruitCount);
                await PackageManager.instance.SetItemInPackage(item, CharacterManager.instance.controllerCharacter.characterPackage, true);
                return true;
            }

            return false;
        }
        return false;
    }

    public async Task<bool> GetSicklePlant()
    {
        var growthStateData = PlantData.growthStages[growthStage];
        if (growthStateData.productValue > 0)
        {
            if (await PackageManager.instance.CheckPackageTryItemIn(CharacterManager.instance.controllerCharacter.characterPackage,
                GameCommon.grassItem, growthStateData.productValue))
            {
                Item item = new Item
                {
                    dataId = GameCommon.grassItem,
                    count = growthStateData.productValue
                };
                await PackageManager.instance.SetItemInPackage(item, CharacterManager.instance.controllerCharacter.characterPackage, true);
            }
        }
        plantState = PlantState.死亡;
        return true;
    }

    public void Dispose()
    {
    }
}