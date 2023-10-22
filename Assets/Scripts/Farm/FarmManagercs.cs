
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
                    int id = fieldArea.fields[i];
                    if (!fields.Contains(id))
                    {
                        Field field = new Field
                        {
                            instanceId = id,
                            fieldState = FieldState.待平整
                        };
                        fields.SetData(field);
                    }
                }
            }
            
        }
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
            }
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
public struct Field
{
    public int instanceId;
    public FieldState fieldState;
    public bool isSetWater;
    public int plantId;
    public override int GetHashCode()
    {
        return instanceId;
    }
}
public enum PlantState 
{
    正常,干旱,枯死,死亡,成熟
}
public struct Plant
{
    public int instaceId;
    public int field;
    public int dataId;
    public int growthStage;
    public int growthDay;
    public bool setWater;
    public PlantState plantState;
    public int nowCycle;

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
  
    public override int GetHashCode()
    {
        return instaceId;
    }
}