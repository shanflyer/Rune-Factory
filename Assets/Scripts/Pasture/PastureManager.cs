using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine.Playables;

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

        GameActionManager.instance.AddListener<TryCreatPasture>(TryCreatPasture);
        GameActionManager.instance.AddListener<TryDeletePasture>(TryDeletePasture);
        GameActionManager.instance.AddListener<TryCreatAnimal>(TryCreatAnimal);
        GameActionManager.instance.AddListener<TryDeleteAnimal>(TryDeleteAnimal);
        GameActionManager.instance.AddListener<TrySetAnimalFoodToPasture>(TrySetAnimalFoodToPasture);
        GameActionManager.instance.AddListener<TryGetAnimalFoodFromPasture>(TryGetAnimalFoodFromPasture);
        GameActionManager.instance.AddListener<AnimalCostFood>(AnimalCostFood);

        GameActionManager.instance.AddListener<GetPastureLevel>(GetPastureLevel);
        GameActionManager.instance.AddListener<TryUpPastureLevel>(TryUpPastureLevel);
        GameActionManager.instance.AddListener<GetPastureNextLevelCost>(GetPastureNextLevelCost);
        GameActionManager.instance.AddListener<SetAnimalToPasture>(SetAnimalToPasture);

        GameActionManager.instance.AddListener<NewDay>(NewDay);
    }
    void NewDay(NewDay newDay)
    {
        foreach(Animal animal in animals)
        {
            animal.Grow();
        }
    }
    async void TryUpPastureLevel(TryUpPastureLevel tryUpPastureLevel)
    {
        if(pastures.GetData(tryUpPastureLevel.pastureId,out var pasture))
        {
            var pastureData = await GameDataManager.instance.GetAsyncData<PastureData>(pasture.dataId);
            int nextlevel = pasture.level + 1;
            if (pastureData.levelDatas.Count >= nextlevel)
            {
                int3 value = pastureData.levelDatas[nextlevel - 1];

                PayManager.instance.PayAction("牧场", "提升牧场等级", value.x, PayType.金币, UpPastureLevel);

                void UpPastureLevel(bool result)
                {
                    if (result)
                    {
                        pasture.level = nextlevel;
                        pasture.animalCase = value.y;
                        if (pasture.linkItem != value.z)
                        {
                            pasture.linkItem = value.z;
                        }
                        pastures.SetData(pasture);

                        tryUpPastureLevel.setValue(nextlevel);
                    }
                    else
                    {
                        tryUpPastureLevel.setValue(0);
                    } 
                }

                return;
            } 
        }
        tryUpPastureLevel.setValue(0);
    }
    void GetPastureLevel(GetPastureLevel getPastureLevel)
    {
        if(pastures.GetData(getPastureLevel.pastureId,out var pasture))
        {
            getPastureLevel.setValue(pasture.level);
            return;
        }
        getPastureLevel.setValue(0);
    }
    async void GetPastureNextLevelCost(GetPastureNextLevelCost getPastureNextLevelCost)
    {
        if(pastures.GetData(getPastureNextLevelCost.pastureId,out var pasture))
        {
            var pastureData = await GameDataManager.instance.GetAsyncData<PastureData>(pasture.dataId);
            int nextlevel = pasture.level + 1;
            if (pastureData.levelDatas.Count >= nextlevel)
            { 
                getPastureNextLevelCost.setValue(pastureData.levelDatas[nextlevel-1].x);
                return;
            }  
        }
        getPastureNextLevelCost.setValue(0);
    }

    void SetAnimalToPasture(SetAnimalToPasture SetAnimalToPasture)
    {
        if(pastures.GetData(SetAnimalToPasture.pastureId,out var pasture)&&
            animals.GetData(SetAnimalToPasture.animalId,out Animal animal))
        {
            if (pasture.animals.Count >= pasture.animalCase)
            {
                SetAnimalToPasture.setResult(false);
            }
            else
            {
                int2 nextCoordinate = MapCellController.instance.GetRandomRoomCell(pasture.instanceId);
                if (nextCoordinate.x == int.MinValue)
                {
                    SetAnimalToPasture.setResult(false);
                }
                else
                {
                    pasture.animals.Add(SetAnimalToPasture.animalId);
                    animal.pasture = pasture.instanceId;
                    animal.mapId = pasture.instanceId; 

                    SetCharacterCoordinate setCharacterCoordinate = new SetCharacterCoordinate
                    {
                        characterId = animal.instaceId,
                        coordinate = new int3(pasture.instanceId,
                        nextCoordinate.xy)
                    };
                    GameActionManager.instance.QueueAction(setCharacterCoordinate, true); 
                    SetAnimalToPasture.setResult(true);
                }
                
            }
            return;
        }
        SetAnimalToPasture.setResult(false);
    }
    async void AnimalCostFood(AnimalCostFood animalCostFood)
    {
        if (pastures.GetData(animalCostFood.pastureId, out var pasture))
        {
            var animalData = await GameDataManager.instance.GetAsyncData<AnimalData>(animalCostFood.animalDataId);
            for(int i = 0; i < animalData.foods.Count; i++)
            {
                bool result = PackageManager.instance.GetOutItenFromPackage(pasture.foodPackage, animalData.foods[i], 1);
                if (result)
                {
                    animalCostFood.setResult(result);
                    return;
                } 
            }  
        }
        animalCostFood.setResult(false);
    }
    void TryGetAnimalFoodFromPasture(TryGetAnimalFoodFromPasture tryGetAnimalFoodFromPasture)
    {
        if (pastures.GetData(tryGetAnimalFoodFromPasture.pastureId, out var pasture))
        {
            int foodPackage = pasture.foodPackage;
            bool result=PackageManager.instance.RemovePlayerPackageItem(tryGetAnimalFoodFromPasture.item.dataId,
                tryGetAnimalFoodFromPasture.item.count);
            tryGetAnimalFoodFromPasture.setResult(result);
        }
    }
    async void TrySetAnimalFoodToPasture(TrySetAnimalFoodToPasture trySetAnimalFoodToPasture)
    {
        if(pastures.GetData(trySetAnimalFoodToPasture.pastureId,out var pasture))
        {
            int foodPackage = pasture.foodPackage;

            int count = await PackageManager.instance.SetItemInPackage(trySetAnimalFoodToPasture.item,
                foodPackage);
            trySetAnimalFoodToPasture.setValue(count); 
        }
    }
    void TryCreatPasture(TryCreatPasture tryCreatPasture)
    {
        int2 key = new int2(tryCreatPasture.roomId, tryCreatPasture.itemInstanceId);
        if(PastureDatas.TryGetValue(key,out var pastureData))
        {
            if (pastureData.open)
            {
                //int instanceId = WorldMapManager.instance.GetInstanceFromEditorId(key);
                if (!pastures.Contains(tryCreatPasture.itemInstanceId))
                {
                    CreatPackage creatFoodPackage = new CreatPackage
                    {
                        level = 1,
                        packageDataId = pastureData.foodPackageId,
                        setValue = (int value) =>
                        {
                            CreatPackage creatPackage = new CreatPackage
                            {
                                level = 1,
                                packageDataId = pastureData.packageId,
                                setValue = SetPackageInstanceId
                            };
                            GameActionManager.instance.QueueAction(creatPackage, true);

                            void SetPackageInstanceId(int packageInstanceId)
                            {
                                Pasture pasture = new Pasture
                                {
                                    name = pastureData.name,
                                    pastureState = PastureState.平常,
                                    linkItem=tryCreatPasture.linkItemDataId,
                                    instanceId = tryCreatPasture.itemInstanceId,
                                    foodPackage = value,
                                    packageId = packageInstanceId,
                                    animals = new UnsafeHashSet<int>(4, Allocator.TempJob),
                                    dataId = pastureData.id,
                                    level = 1,
                                    animalCase = pastureData.levelDatas[0].y
                                };

                                TryCreatRoom tryCreatRoom = new TryCreatRoom
                                {
                                    roomName = tryCreatPasture.pastureName,
                                    roomId = pastureData.linkRoom,
                                    eventId = pastureData.eventId,
                                    setValue = (int instanceId) =>
                                    {
                                        // pasture.roomInstanceId = instanceId;
                                        pastures.SetData(pasture);
                                        tryCreatPasture.setResult(true);
                                    }
                                };
                            }
                        }
                    };
                    GameActionManager.instance.QueueAction(creatFoodPackage, true); 
                } 
            }
        }
    }
    void TryDeletePasture(TryDeletePasture tryDeletePasture)
    {
        if(pastures.GetData(tryDeletePasture.instanceId,out var pasture))
        {
            int linkRoom = pasture.instanceId;
             
            TryDeleteRoom tryDeleteRoom = new TryDeleteRoom
            {
                roomId = linkRoom
            };
            GameActionManager.instance.QueueAction(tryDeleteRoom, true);

            pastures.RemoveData(pasture.instanceId);
            tryDeletePasture.setResult(true);
            return;
        }
        tryDeletePasture.setResult(false);
    }
    
    async void TryCreatAnimal(TryCreatAnimal tryCreatAnimal)
    {
        AnimalData animalData = await GameDataManager.instance.GetAsyncData<AnimalData>(tryCreatAnimal.dataId);
        Animal animal = new Animal
        {
            animalState = AnimalState.正常,
            dataId = tryCreatAnimal.dataId,
            mapId = tryCreatAnimal.roomId,
            linkCharacterData=animalData.linkCharacter
        };

        if (pastures.GetData(tryCreatAnimal.roomId,out var pasture))
        {
            animal.pasture = pasture.instanceId;  
        }

        CreatCharacter creatCharacter = new CreatCharacter
        {
            characterId = animalData.linkCharacter,
            mapInstance = tryCreatAnimal.roomId,
            coordinateX = tryCreatAnimal.coordinate.x,
            coordinateY = tryCreatAnimal.coordinate.y,
            setValue= SetAnimalInstanceId
        };
        void SetAnimalInstanceId(int value)
        {
            if (value != 0)
            { 
                animal.instaceId = value;
                animals.AddData(animal);
                pasture.animals.Add(animal.instaceId);
                pastures.SetData(pasture);
            }
            tryCreatAnimal.setValue(value);
        }
        GameActionManager.instance.QueueAction(creatCharacter);
       // tryCreatAnimal.setValue(-1);
    }
    void TryDeleteAnimal(TryDeleteAnimal tryDeleteAnimal)
    {
        if(animals.GetData(tryDeleteAnimal.animalId,out var animal))
        {
            if(pastures.GetData(animal.pasture,out var pasture))
            {
                pasture.animals.Remove(animal.instaceId);
                pastures.SetData(pasture);
            }

            DestoryCharacter destoryCharacter = new DestoryCharacter
            {
                characterId = animal.instaceId,
                isTemp = false
            };
            GameActionManager.instance.QueueAction(destoryCharacter);
            animals.RemoveData(animal.Key);
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
    public int linkItem;
    public PastureState pastureState; 
    public UnsafeHashSet<int> animals;
    public int dataId;
    public FixedString64Bytes name;
    public int packageId;
    public int foodPackage;
    public int level;
    public int animalCase;
    
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
    public bool setFood;
    public AnimalState animalState;
    public int nowCycle;

    public int linkCharacterData;
    public int Key => instaceId;

    public async void Grow()
    {
        if (animalState == AnimalState.死亡)
        {
            return;
        }

        if (pasture == 0)
        {
            if (animalState == AnimalState.饥饿)
            {
                animalState = AnimalState.死亡;
                TryDeleteAnimal tryDeleteAnimal = new TryDeleteAnimal
                {
                    animalId = instaceId,
                };
                GameActionManager.instance.QueueAction(tryDeleteAnimal);
            }
            return;
        }

        growthDay++;
        AnimalData animalData = await GameDataManager.instance.GetAsyncData<AnimalData>(dataId);
        var growthStateData = animalData.growthStages[growthStage];
        if (growthDay >= growthStateData.growthDay)
        {
            int index = growthStage + 1;
            if (index < animalData.growthStages.Count - 1)
            {
                growthStage = index;
                growthDay = 0;

                int stageValue = animalData.growthStages[index].objAnimationStage;
                if (stageValue!=0&&linkCharacterData != stageValue)
                {
                    ChangeCharacter changeCharacter = new ChangeCharacter
                    {
                        instanceId = instaceId,
                        newDataId = stageValue
                    };
                    GameActionManager.instance.QueueAction(changeCharacter);
                }
            }
            else
            {
                animalState = AnimalState.死亡;
                TryDeleteAnimal tryDeleteAnimal = new TryDeleteAnimal
                {
                    animalId = instaceId,
                };
                GameActionManager.instance.QueueAction(tryDeleteAnimal);                
            }
        }
        if (animalState != AnimalState.死亡)
        {
            if (!setFood)
            {
                animalState = AnimalState.饥饿;
            }
            if (animalState != AnimalState.饥饿)
            {
                Item item = new Item
                {
                    dataId = animalData.product,
                    count = 1
                };
                TrySetItemToPastureBox trySetItemToPastureBox = new TrySetItemToPastureBox
                {
                    item = item,
                    pastureId = pasture,
                    setResult= SetResult
                };
                GameActionManager.instance.QueueAction(trySetItemToPastureBox);
                void SetResult(bool result)
                {

                }                
            }

            AnimalCostFood animalCostFood = new AnimalCostFood
            {
                animalDataId = dataId,
                pastureId = pasture,
                setResult = CostFoodResult
            };
            GameActionManager.instance.QueueAction(animalCostFood, true); 
        }
    }

    void CostFoodResult(bool result)
    {
        this.setFood = result;
        if (result)
        {
            animalState = AnimalState.正常;
        }
        else
        {
            if (animalState == AnimalState.饥饿)
            {
                animalState = AnimalState.死亡;
                TryDeleteAnimal tryDeleteAnimal = new TryDeleteAnimal
                {
                    animalId = instaceId,
                };
                GameActionManager.instance.QueueAction(tryDeleteAnimal);
            }
            else
            {
                animalState = AnimalState.饥饿;
            }
        }
    }
}