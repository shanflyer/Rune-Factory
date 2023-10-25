using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class PastureManager:Singleton<PastureManager>
{
   
}
[System.Serializable]
public enum AnimalStatus
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
/*
public struct Pasture : INativeData
{
    public int instanceId;
    public FieldState fieldState;
    public bool isSetWater;
    public int plantId;

    public int Key => instanceId;
}

public struct Animal : INativeData
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
        }
    }
    public async Task<bool> GetPlantFruit()
    {
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
        }
        return false;
    }
}*/