using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine.TextCore.Text;

public class PastureManager : Singleton<PastureManager>
{
    private MyNativeData<Pasture> pastures = new MyNativeData<Pasture>();
    private MyNativeData<Animal> animals = new MyNativeData<Animal>();
    
    private Dictionary<int, int> pastureLinkItems = new Dictionary<int, int>();
    MyInstance myInstance = new MyInstance();
    protected override void Clear()
    {
        base.Clear(); 
        pastures.Dispose();
        animals.Dispose();
    }

    public override void Init()
    {
        base.Init();
        pastures.Init(16);
        animals.Init(16);
        pastureLinkItems = new Dictionary<int, int>();

        GameActionManager.instance.AddListener<TryCreatPasture>(TryCreatPasture);
        GameActionManager.instance.AddListener<TryDeletePasture>(TryDeletePasture);
        GameActionManager.instance.AddListener<TryCreatAnimal>(TryCreatAnimal);
        GameActionManager.instance.AddListener<TryDeleteAnimal>(TryDeleteAnimal);
        GameActionManager.instance.AddListener<TrySetAnimalFoodToPasture>(TrySetAnimalFoodToPasture);
        GameActionManager.instance.AddListener<TryGetAnimalFoodFromPasture>(TryGetAnimalFoodFromPasture);
        GameActionManager.instance.AddListener<AnimalCostFood>(AnimalCostFood);

        GameActionManager.instance.AddListener<GetPastureLevel>(GetPastureLevel);
        GameActionManager.instance.AddListener<TryUpPastureLevel>(TryUpPastureLevel);
        GameActionManager.instance.AddListener<SetAnimalToPasture>(SetAnimalToPasture);
        GameActionManager.instance.AddListener<SetPastureIndex>(SetPastureIndex);
        GameActionManager.instance.AddListener<RefreshAnimalPos>(RefreshAnimalPos);

        GameActionManager.instance.AddListener<NewDay>(NewDay);
    }

    private void NewDay(NewDay newDay)
    {
        foreach (Animal animal in animals)
        {
            animal.Grow();
        }
    }
    public bool GetPasture(int instanceId,out Pasture pasture)
    {
        return pastures.GetData(instanceId, out pasture);
    }
    public List<Pasture> GetAllPasture()
    {
        List<Pasture> outResult = new List<Pasture>();
        foreach (Pasture pasture in pastures)
        {
            outResult.Add(pasture);
        }
        return outResult;
    }
    public bool CheckAnimal(int id)
    {
        return animals.Contains(id);
    }
    public bool GetAnimal(int id,out Animal animal)
    {
        return animals.GetData(id, out animal);
    }

    private async void TryUpPastureLevel(TryUpPastureLevel tryUpPastureLevel)
    {
        if (pastures.GetData(tryUpPastureLevel.pastureId, out var pasture))
        {
            var pastureData = await GameDataManager.instance.GetAsyncData<PastureData>(pasture.dataId);
            int nextlevel = pasture.level + 1;
            if (pastureData.levelDatas.Count >= nextlevel)
            {
                var pastureLevelData = pastureData.levelDatas[nextlevel - 1];
                List<MyInt3> items = new List<MyInt3>();
                for (int i = 0; i < pastureLevelData.creatItems.Count; i++)
                {
                    int2 costItem = pastureLevelData.creatItems[i];
                    int totalCount = PackageManager.instance.GetPlayerItemCount(costItem.x);
                    items.Add(new MyInt3
                    {
                        value = new int3(costItem.xy, totalCount)
                    });
                }

                ItemCostEventData itemCostEventData = new ItemCostEventData
                {
                    title = "牧场",
                    notice = "提升牧场等级",
                    costValue = pastureLevelData.creatMoney,
                    payType = PayType.金币,
                    items = items,
                    afterAction = UpPastureLevel,
                };
                UIManager.instance.ShowGamePanel<ItemCostSelectPanel, ItemCostEventData>(itemCostEventData);

                void UpPastureLevel(bool result)
                {
                    if (result)
                    {
                        pasture.level = nextlevel;
                        pasture.animalCase = pastureLevelData.animalCase;

                        if (WorldMapManager.instance.GetMapItemPos(pasture.instanceId, out var objCoordinate))
                        {
                            AddMapItem addMapItem = new AddMapItem
                            {
                                coordinate = objCoordinate.xy,
                                mapId = objCoordinate.z,
                                dataId = pastureLevelData.linkItem,
                                setValue = (int instanceId) =>
                                {
                                    pastureLinkItems.Remove(pasture.linkItem);
                                    pasture.linkItem = instanceId;
                                    pastures.SetData(pasture);
                                    pastureLinkItems[pasture.linkItem] = pasture.instanceId;
                                }
                            };
                            GameActionManager.instance.QueueAction(addMapItem);
                        }
                        DeleteMapItem deleteMapItem = new DeleteMapItem
                        {
                            mapItemInstanceId = pasture.linkItem,
                        };
                        GameActionManager.instance.QueueAction(deleteMapItem, true);

                        pastures.SetData(pasture);

                        RefreshPasture refreshPasture = new RefreshPasture
                        {
                            instanceId = pasture.instanceId
                        };
                        GameActionManager.instance.QueueAction(refreshPasture);

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

    private void GetPastureLevel(GetPastureLevel getPastureLevel)
    {
        if (pastures.GetData(getPastureLevel.pastureId, out var pasture))
        {
            getPastureLevel.setValue(pasture.level);
            return;
        }
        getPastureLevel.setValue(0);
    }

    private void RefreshAnimalPos(RefreshAnimalPos RefreshAnimalPos)
    {
        if(animals.GetData(RefreshAnimalPos.animalId,out var animal))
        {
            if(pastures.GetData(animal.pasture, out var pasture))
            {
                int2 nextCoordinate = MapCellController.instance.GetRandomRoomCell(pasture.linkRoom);
                if (nextCoordinate.x != int.MinValue)
                {
                    SetCharacterCoordinate setCharacterCoordinate = new SetCharacterCoordinate
                    {
                        characterId = animal.instaceId,
                        coordinate = new int3(nextCoordinate.xy, pasture.linkRoom)
                    };
                    GameActionManager.instance.QueueAction(setCharacterCoordinate, true);
                }
            }
            else
            {
                TryDeleteAnimal(new TryDeleteAnimal { animalId=animal.instaceId});
            } 
        }
    }

    private void SetAnimalToPasture(SetAnimalToPasture SetAnimalToPasture)
    {
        if (pastures.GetData(SetAnimalToPasture.pastureId, out var pasture) &&
            animals.GetData(SetAnimalToPasture.animalId, out Animal animal))
        {
            if (pasture.animals.Count >= pasture.animalCase)
            {
                SetAnimalToPasture.setResult(false);
            }
            else
            {
                if (SetAnimalToPasture.refreshPos)
                {
                    int2 nextCoordinate = MapCellController.instance.GetRandomRoomCell(pasture.linkRoom);
                    if (nextCoordinate.x != int.MinValue)
                    {
                        SetCharacterCoordinate setCharacterCoordinate = new SetCharacterCoordinate
                        {
                            characterId = animal.instaceId,
                            coordinate = new int3(nextCoordinate.xy, pasture.linkRoom)
                        };
                        GameActionManager.instance.QueueAction(setCharacterCoordinate, true);
                    }
                }

                pasture.animals.Add(SetAnimalToPasture.animalId);
                animal.pasture = pasture.instanceId;
                animals.SetData(animal);
                pastures.SetData(pasture);
                RefreshPasture refreshPasture = new RefreshPasture
                {
                    instanceId = pasture.instanceId
                };
                GameActionManager.instance.QueueAction(refreshPasture);
                SetAnimalToPasture.setResult(true);
            }
            return;
        }
        SetAnimalToPasture.setResult(false);
    }

    private async void AnimalCostFood(AnimalCostFood animalCostFood)
    { 
        if (pastures.GetData(animalCostFood.pastureId, out var pasture))
        {
            RefreshPasture refreshPasture = new RefreshPasture
            {
                instanceId = pasture.instanceId
            };
            var animalData = await GameDataManager.instance.GetAsyncData<AnimalData>(animalCostFood.animalDataId);
            for (int i = 0; i < animalData.foods.Count; i++)
            {
                bool result = PackageManager.instance.GetOutItenFromPackage(pasture.foodPackage, animalData.foods[i], 1);
                if (!result)
                {
                    
                    GameActionManager.instance.QueueAction(refreshPasture);
                    animalCostFood.setResult(false);
                    return;
                }
            }

            GameActionManager.instance.QueueAction(refreshPasture);
            animalCostFood.setResult(true);
        } 
    }

    private void TryGetAnimalFoodFromPasture(TryGetAnimalFoodFromPasture tryGetAnimalFoodFromPasture)
    {
        if (pastures.GetData(tryGetAnimalFoodFromPasture.pastureId, out var pasture))
        {
            int foodPackage = pasture.foodPackage;
            bool result = PackageManager.instance.RemovePlayerPackageItem(tryGetAnimalFoodFromPasture.item.dataId,
                tryGetAnimalFoodFromPasture.item.count);
            tryGetAnimalFoodFromPasture.setResult(result);

            RefreshPasture refreshPasture = new RefreshPasture
            {
                instanceId = pasture.instanceId
            };
            GameActionManager.instance.QueueAction(refreshPasture);
        }
    }

    private async void TrySetAnimalFoodToPasture(TrySetAnimalFoodToPasture trySetAnimalFoodToPasture)
    {
        if (pastures.GetData(trySetAnimalFoodToPasture.pastureId, out var pasture))
        {
            int foodPackage = pasture.foodPackage;

            int count = await PackageManager.instance.SetItemInPackage(trySetAnimalFoodToPasture.item,
                foodPackage);
            trySetAnimalFoodToPasture.setValue(count);

            RefreshPasture refreshPasture = new RefreshPasture
            {
                instanceId = pasture.instanceId
            };
            GameActionManager.instance.QueueAction(refreshPasture);
        }
    }
    void SetPastureIndex(SetPastureIndex setPastureIndex)
    {
        if(pastures.GetData(setPastureIndex.pastureId,out var pasture))
        {
            pasture.index = setPastureIndex.index;
            pastures.SetData(pasture);
        }
    }
    private async void TryCreatPasture(TryCreatPasture tryCreatPasture)
    {

        if (WorldMapManager.instance.GetMapItemPos(tryCreatPasture.itemInstanceId, out var objCoordinate))
        { 
            PastureData pastureData = await GameDataManager.instance.GetAsyncData<PastureData>(tryCreatPasture.dataId); 
            PastureLevelData pastureLevelData = pastureData.levelDatas[0];

            List<MyInt3> items = new List<MyInt3>();
            for (int i = 0; i < pastureLevelData.creatItems.Count; i++)
            {
                int2 costItem = pastureLevelData.creatItems[i];
                int totalCount = PackageManager.instance.GetPlayerItemCount(costItem.x);
                items.Add(new MyInt3
                {
                    value = new int3(costItem.xy, totalCount)
                });
            }

            ItemCostEventData itemCostEventData = new ItemCostEventData
            {
                title = "牧场",
                notice = "建造一座牧场",
                costValue = pastureLevelData.creatMoney,
                payType = PayType.金币,
                items = items,
                afterAction = CreatPasture,
            };
            UIManager.instance.ShowGamePanel<ItemCostSelectPanel, ItemCostEventData>(itemCostEventData);

            void CreatPasture(bool result)
            {
                if (result)
                {
                    DeleteMapItem deleteMapItem = new DeleteMapItem
                    {
                        mapItemInstanceId = tryCreatPasture.itemInstanceId,
                        triggerClear = true
                    };
                    GameActionManager.instance.QueueAction(deleteMapItem, true);

                    AddMapItem addMapItem = new AddMapItem
                    {
                        dataId = pastureLevelData.linkItem,
                        coordinate = objCoordinate.xy,
                        mapId = objCoordinate.z,
                        setValue = SetValue
                    };
                    GameActionManager.instance.QueueAction(addMapItem);
                }
                else
                {
                    if (tryCreatPasture.setResult != null)
                    {
                        tryCreatPasture.setResult(false);
                    }

                }
            }
            void SetValue(int instanceId)
            {

                CreatPackage creatFoodPackage = new CreatPackage
                {
                    level = 1,
                    packageDataId = pastureData.foodPackage,
                    setValue = (int foodPackageId) =>
                    {
                        CreatPackage creatWaterPackage = new CreatPackage
                        {
                            level = 1,
                            packageDataId = pastureData.waterPackage,
                            setValue = (int waterPackageId) =>
                            {
                                CreatPackage creatProductPackage = new CreatPackage
                                {
                                    level = 1,
                                    packageDataId = pastureData.productPackage,
                                    setValue = SetPackageInstanceId
                                };
                                GameActionManager.instance.QueueAction(creatProductPackage, true);


                                void SetPackageInstanceId(int packageInstanceId)
                                {

                                    Pasture pasture = new Pasture
                                    {
                                        instanceId = myInstance.CreatInstanceId(),
                                        name = string.IsNullOrEmpty(tryCreatPasture.pastureName) ? pastureData.pastureName : tryCreatPasture.pastureName,
                                        pastureState = PastureState.平常,
                                        linkItem = instanceId,
                                        foodPackage = foodPackageId,
                                        waterPackage = waterPackageId,
                                        productPackage = packageInstanceId,
                                        animals = new UnsafeHashSet<int>(4, Allocator.TempJob),
                                        dataId = pastureData.id,
                                        level = 1,
                                        animalCase = pastureLevelData.animalCase,
                                        linkRoom = tryCreatPasture.roomId
                                    };
                                    pastures.SetData(pasture);
                                    pastureLinkItems.Add(instanceId, pasture.instanceId);
                                    RefreshPasture refreshPasture = new RefreshPasture
                                    {
                                        instanceId = pasture.instanceId
                                    };
                                    GameActionManager.instance.QueueAction(refreshPasture);

                                    SimpleTalk simpleTalk = new SimpleTalk
                                    {
                                        characterId = CharacterManager.instance.controllerCharacter.instanceId,
                                        talkId = pastureLevelData.successTalk,
                                    };
                                    GameActionManager.instance.QueueAction(simpleTalk);

                                    if (tryCreatPasture.setValue != null)
                                    {
                                        tryCreatPasture.setValue(pasture.instanceId);
                                    }

                                    if (tryCreatPasture.setResult != null)
                                    {
                                        tryCreatPasture.setResult(true);
                                    }
                                }
                            }
                        };
                        GameActionManager.instance.QueueAction(creatWaterPackage, true);


                    }
                };
                GameActionManager.instance.QueueAction(creatFoodPackage, true);

                
            }
        }
        else
        {
            if (tryCreatPasture.setResult != null)
            {
                tryCreatPasture.setResult(false);
            }
        }

    }

    private void TryDeletePasture(TryDeletePasture tryDeletePasture)
    {
        if (pastures.GetData(tryDeletePasture.instanceId, out var pasture))
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

    private async void TryCreatAnimal(TryCreatAnimal tryCreatAnimal)
    {
        AnimalData animalData = await GameDataManager.instance.GetAsyncData<AnimalData>(tryCreatAnimal.dataId);
        Animal animal = new Animal
        {
            animalState = AnimalState.正常,
            dataId = tryCreatAnimal.dataId, 
            linkCharacterData = animalData.linkCharacter
        };

        if (pastures.GetData(tryCreatAnimal.roomId, out var pasture))
        {
            animal.pasture = pasture.instanceId; 
        }

        CreatCharacter creatCharacter = new CreatCharacter
        {
            characterId = animalData.linkCharacter,
            mapInstance = tryCreatAnimal.roomId,
            coordinateX = tryCreatAnimal.coordinate.x,
            coordinateY = tryCreatAnimal.coordinate.y,
            setValue = SetAnimalInstanceId
        };
        void SetAnimalInstanceId(int value)
        {
            if (value != 0)
            {
                animal.instaceId = value;
                animals.AddData(animal);
                if (pasture.dataId != 0)
                {
                    pasture.animals.Add(animal.instaceId);
                    pastures.SetData(pasture);

                    RefreshPasture refreshPasture = new RefreshPasture
                    {
                        instanceId = pasture.instanceId
                    };
                    GameActionManager.instance.QueueAction(refreshPasture);
                }
            }
            tryCreatAnimal.setValue(value);
        }
        GameActionManager.instance.QueueAction(creatCharacter);
        // tryCreatAnimal.setValue(-1);
    }

    private void TryDeleteAnimal(TryDeleteAnimal tryDeleteAnimal)
    {
        if (animals.GetData(tryDeleteAnimal.animalId, out var animal))
        {
            if (pastures.GetData(animal.pasture, out var pasture))
            {
                pasture.animals.Remove(animal.instaceId);
                pastures.SetData(pasture);

                RefreshPasture refreshPasture = new RefreshPasture
                {
                    instanceId = pasture.instanceId
                };
                GameActionManager.instance.QueueAction(refreshPasture);
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
    平常, 损毁,
}

public struct Pasture : INativeData,IReferenceData
{
    public int instanceId;
    public int linkItem;
    public PastureState pastureState;
    public UnsafeHashSet<int> animals;
    public int dataId;
    public FixedString64Bytes name; 
    public int foodPackage;
    public int waterPackage;
    public int productPackage;
    public int level;
    public int index;
    public int animalCase;
    public int linkRoom;
    public int Key => instanceId;

    public void Dispose()
    {
        animals.Dispose();
    }
}

public struct Animal : INativeData
{ 
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
                if (stageValue != 0 && linkCharacterData != stageValue)
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
                    setResult = SetResult
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

    private void CostFoodResult(bool result)
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

    public void Dispose()
    {
    }
}