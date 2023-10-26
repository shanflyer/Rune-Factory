using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

public class PastureManager:Singleton<PastureManager>
{
    MyNativeData<Pasture> pastures = new MyNativeData<Pasture>();
    MyNativeData<Animal> animals = new MyNativeData<Animal>();


    Dictionary<int2, PastureData> PastureDatas = new Dictionary<int2, PastureData>();
    protected override void Clear()
    {
        base.Clear();
        foreach(Pasture pasture in pastures)
        {
            pasture.Dispose();  
        }
        pastures.Dispose();
        animals.Dispose();
    }
    public override async void Init()
    {
        base.Init();
        pastures.Init(16);
        animals.Init(16);

        var allPastureDatas = await GameDataManager.instance.GetAllAsyncData<PastureData>();
        for (int i = 0; i < allPastureDatas.Count; i++)
        {
            var pastureData = allPastureDatas[i];
            PastureDatas.Add(new int2(pastureData.mapId, pastureData.linkItem), pastureData);
        }
    }
    void TryCreatPasture(TryCreatPasture tryCreatPasture)
    {
        int2 key = new int2(tryCreatPasture.roomId, tryCreatPasture.itemInstanceId);
        if(PastureDatas.TryGetValue(key,out var pastureData))
        {
            if (pastureData.open)
            {
                int instanceId = WorldMapManager.instance.GetInstanceFromEditorId(key);
                if (!pastures.Contains(instanceId))
                {
                    Pasture pasture = new Pasture
                    {
                        name = pastureData.name,
                        pastureState=PastureState.平常,
                        instanceId = instanceId
                    };
                    pastures.SetData(pasture);
                } 
            }
        }
    }
}
[System.Serializable]
public enum AnimalState
{
    正常 = 0,
    饥饿 = 1,
    高兴 = 2,
    悲伤 = 3,
    死亡 = 4
}

[System.Serializable]
public enum AgeStatus
{
    幼年 = 0,
    成年 = 1,
    老年 = 2
}

public enum PastureState
{
    平常,损毁,
}
public struct Pasture : INativeData
{
    public int instanceId;
    public PastureState pastureState;
    public UnsafeList<int> animals;
    public int dataId;
    public FixedString64Bytes name;

    public int Key => instanceId;

    public void Dispose()
    {
        animals.Dispose();
    }
}

public struct Animal : INativeData
{
    public int mapId;
    public int instaceId;
    public int pasture;
    public int dataId;
    public int growthStage;
    public int growthDay;
    public bool setWater;
    public AnimalState animalState;
    public int nowCycle;

    public int Key => instaceId;

    public async void Grow()
    {
        /*
        if (plantState == PlantState.死亡 || plantState == PlantState.枯死)
        {
            return;
        }
        growthDay++;
        PlantData plantData = await GameDataManager.instance.GetAsyncData<PlantData>(dataId);
        var growthStateData = plantData.growthStages[growthStage];
        if (growthDay >= growthStateData.growthDay)
        {
            int index = growthStage + 1;
            if (index < plantData.growthStages.Count - 1)
            {
                growthStage = index;
                growthDay = 0;
            }
            if (index == plantData.growthStages.Count - 1)
            {
                plantState = PlantState.成熟;
            }
        }*/
    }
    public async Task<bool> GetPlantFruit()
    {
        /*
        if (plantState == PlantState.成熟)
        {
            PlantData plantData = await GameDataManager.instance.GetAsyncData<PlantData>(dataId);

            if (await PackageManager.instance.CheckPackageTryItemIn(CharacterManager.instance.controllerCharacter.characterPackage,
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
        }*/
        return false;
    }
}