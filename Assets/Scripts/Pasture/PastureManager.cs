using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class PastureManager : Singleton<PastureManager>
{
    private Dictionary<int, Pasture> pastures = new Dictionary<int, Pasture>();
    private Dictionary<int, Animal> animals = new Dictionary<int, Animal>();

    private Dictionary<int, int> pastureLinkItems = new Dictionary<int, int>(); 

    protected override void Clear()
    {
        base.Clear();
        pastures.Clear();
        animals.Clear();
    }

    public override void Init()
    {
        base.Init();
        pastures.Clear();
        animals.Clear();
        pastureLinkItems = new Dictionary<int, int>();

        GameActionManager.instance.AddListener<TryCreatPasture>(TryCreatPasture);
        GameActionManager.instance.AddListener<TryDeletePasture>(TryDeletePasture);
        GameActionManager.instance.AddListener<TryCreatAnimal>(TryCreatAnimal);
        GameActionManager.instance.AddListener<TryDeleteAnimal>(TryDeleteAnimal);
        GameActionManager.instance.AddListener<TrySetAnimalFoodToPasture>(TrySetAnimalFoodToPasture);
        GameActionManager.instance.AddListener<TryGetAnimalFoodFromPasture>(TryGetAnimalFoodFromPasture);
        GameActionManager.instance.AddListener<AnimalCostFood>(AnimalCostFood);
        GameActionManager.instance.AddListener<SampleCreatAnimal>(SampleCreatAnimal);
        GameActionManager.instance.AddListener<GetPastureLevel>(GetPastureLevel);
        GameActionManager.instance.AddListener<TryUpPastureLevel>(TryUpPastureLevel);
        GameActionManager.instance.AddListener<SetAnimalToPasture>(SetAnimalToPasture);
        GameActionManager.instance.AddListener<SetPastureIndex>(SetPastureIndex);
        GameActionManager.instance.AddListener<RefreshAnimalPos>(RefreshAnimalPos);
        GameActionManager.instance.AddListener<LinkPasturePackage>(LinkPasturePackage);
        GameActionManager.instance.AddListener<TrySetItemToPastureBox>(TrySetItemToPastureBox);

        GameActionManager.instance.AddListener<GiveGift>(GiveGift);

        GameActionManager.instance.AddListener<NewDay>(NewDay);
    }

    private void GiveGift(GiveGift giveGift)
    {
        if (animals.TryGetValue(giveGift.receiveCharacter, out var animal))
        {
            AnimalData animalData = animal.animalData;
            GameTimerController.instance.DelayAction(1000, () =>
            {
                ShowEmote showEmote = new ShowEmote
                {
                    emoteId = animalData.getFoodEmote,
                    entityType = EntityType.角色,
                    id = giveGift.receiveCharacter
                };
                GameActionManager.instance.QueueAction(showEmote);
            });
        }
    }

    private void NewDay(NewDay newDay)
    {
        foreach (Animal animal in animals.Values)
        {
            animal.Grow();
            GameDataSaveManager.instance.UserGameSaveData.SetAnimalData(animal);
        }
    }

    public bool GetPasture(int instanceId, out Pasture pasture)
    {
        return pastures.TryGetValue(instanceId, out pasture);
    }

    public List<Pasture> GetAllPasture()
    {
        List<Pasture> outResult = pastures.Values.ToList();
        return outResult;
    }

    public bool CheckAnimal(int id)
    {
        return animals.ContainsKey(id);
    }

    public bool TalkAnimal(int instanceId)
    {
        if(GetAnimal(instanceId,out var animal))
        {
            Talk talk = new Talk
            {
                characterId = instanceId,
                talkId = animal.growthStage < 1 ? animal.animalData.talkId.x : animal.animalData.talkId.y,
                displayFunction = true,

            };
            GameActionManager.instance.QueueAction(talk);
            return true;
        }
        return false;
    }
    public bool GetAnimal(int id, out Animal animal)
    {
        return animals.TryGetValue(id, out animal);
    }

    private async void TryUpPastureLevel(TryUpPastureLevel tryUpPastureLevel)
    {
        if (!pastureLinkItems.TryGetValue(tryUpPastureLevel.itemInstance, out var pastureId))
        {
            pastureId = tryUpPastureLevel.pastureId;
        }
        if (pastures.TryGetValue(pastureId, out var pasture))
        {
            var pastureData = pasture.pastureData;
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
               await UIManager.instance.ShowGamePanel<ItemCostSelectPanel, ItemCostEventData>(itemCostEventData);

                void UpPastureLevel(bool result)
                {
                    if (result)
                    {
                        pasture.level = nextlevel;
                        pasture.animalCase = pastureLevelData.animalCase;
                        pasture.linkRoom = tryUpPastureLevel.roomId;

                        SetItemAnimation setItemAnimation = new SetItemAnimation
                        {
                            id = tryUpPastureLevel.itemInstance,
                            keyX = pastureLevelData.animationKey.x,
                            keyY = pastureLevelData.animationKey.y,
                        };
                        GameActionManager.instance.QueueAction(setItemAnimation);

                        foreach (var animal in pasture.animals)
                        {
                            ChangeCharacterNewMap changeCharacterNewMap = new ChangeCharacterNewMap
                            {
                                characterInstance = animal,
                                newMap = tryUpPastureLevel.roomId
                            };
                            GameActionManager.instance.QueueAction(changeCharacterNewMap);
                        }

                        RefreshPasture refreshPasture = new RefreshPasture
                        {
                            instanceId = pasture.instanceId
                        };
                        GameActionManager.instance.QueueAction(refreshPasture);

                        tryUpPastureLevel.setValue(pasture.instanceId);
                        if (tryUpPastureLevel.setResult != null)
                        {
                            tryUpPastureLevel.setResult(true);
                        }
                        return;
                    }
                    else
                    {
                        tryUpPastureLevel.setValue(pasture.instanceId);
                        if (tryUpPastureLevel.setResult != null)
                        {
                            tryUpPastureLevel.setResult(false);
                        }
                        return;
                    }
                }

                return;
            }

            GameDataSaveManager.instance.UserGameSaveData.SetPastureData(pasture);
        }
        tryUpPastureLevel.setValue(pasture.instanceId);
        if (tryUpPastureLevel.setResult != null)
        {
            tryUpPastureLevel.setResult(false);
        }
    }

    private void GetPastureLevel(GetPastureLevel getPastureLevel)
    {
        if (pastures.TryGetValue(getPastureLevel.pastureId, out var pasture))
        {
            getPastureLevel.setValue(pasture.level);
            return;
        }
        getPastureLevel.setValue(0);
    }

    private void RefreshAnimalPos(RefreshAnimalPos RefreshAnimalPos)
    {
        if (animals.TryGetValue(RefreshAnimalPos.animalId, out var animal))
        {
            if (pastures.TryGetValue(animal.pasture, out var pasture))
            {
                int2 nextCoordinate = MapCellController.instance.GetRandomRoomCell(pasture.linkRoom);
                if (nextCoordinate.x != int.MinValue)
                {
                    SetCharacterCoordinate setCharacterCoordinate = new SetCharacterCoordinate
                    {
                        characterId = animal.instanceId,
                        coordinate = new int3(nextCoordinate.xy, pasture.linkRoom)
                    };
                    GameActionManager.instance.QueueAction(setCharacterCoordinate, true);
                }
            }
            else
            {
                TryDeleteAnimal(new TryDeleteAnimal { animalId = animal.instanceId });
            }
        }
    }

    private void SetAnimalToPasture(SetAnimalToPasture SetAnimalToPasture)
    {
        if (pastures.TryGetValue(SetAnimalToPasture.pastureId, out var pasture) &&
            animals.TryGetValue(SetAnimalToPasture.animalId, out Animal animal))
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
                            characterId = animal.instanceId,
                            coordinate = new int3(nextCoordinate.xy, pasture.linkRoom)
                        };
                        GameActionManager.instance.QueueAction(setCharacterCoordinate, true);
                    }
                }

                pasture.animals.Add(SetAnimalToPasture.animalId);
                animal.pasture = pasture.instanceId;
                RefreshPasture refreshPasture = new RefreshPasture
                {
                    instanceId = pasture.instanceId
                };
                GameActionManager.instance.QueueAction(refreshPasture);
                SetAnimalToPasture.setResult(true);

                InformationController.instance.AddInformation($"+{LanguageManage.SwitchStr(animal.name)}+{LanguageManage.SwitchStr("已经分配到对应牧场")}", PromptShow: true);
            }

            GameDataSaveManager.instance.UserGameSaveData.SetPastureData(pasture);
            GameDataSaveManager.instance.UserGameSaveData.SetAnimalData(animal);
            return;
        }
        SetAnimalToPasture.setResult(false);
    }

    private void TrySetItemToPastureBox(TrySetItemToPastureBox trySetItemToPastureBox)
    {
        if (pastures.TryGetValue(trySetItemToPastureBox.pastureId, out var pasture))
        {
            AddPackageItem addPackageItem = new AddPackageItem
            {
                itemDataId = trySetItemToPastureBox.itemId,
                itemCount = trySetItemToPastureBox.itemCount,
                packageId = pasture.productPackage
            };
            GameActionManager.instance.QueueAction(addPackageItem);
        }
    }

    private async void AnimalCostFood(AnimalCostFood animalCostFood)
    {
        if (pastures.TryGetValue(animalCostFood.pastureId, out var pasture))
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
        if (pastures.TryGetValue(tryGetAnimalFoodFromPasture.pastureId, out var pasture))
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
        if (pastures.TryGetValue(trySetAnimalFoodToPasture.pastureId, out var pasture))
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

    private void SetPastureIndex(SetPastureIndex setPastureIndex)
    {
        if (pastures.TryGetValue(setPastureIndex.pastureId, out var pasture))
        {
            pasture.index = setPastureIndex.index;

            GameDataSaveManager.instance.UserGameSaveData.SetPastureData(pasture);
        }
    }

    private void LinkPasturePackage(LinkPasturePackage linkPasturePackage)
    {
        if (pastures.TryGetValue(linkPasturePackage.pastureInstance, out var pasture))
        {
            ChangePackageInnstance changeFood = new ChangePackageInnstance
            {
                oldInstanceId = pasture.foodPackage,
                newInstanceId = linkPasturePackage.foodPackage
            };
            GameActionManager.instance.QueueAction(changeFood, true);

            ChangePackageInnstance changeProduct = new ChangePackageInnstance
            {
                oldInstanceId = pasture.productPackage,
                newInstanceId = linkPasturePackage.productPackage
            };
            GameActionManager.instance.QueueAction(changeProduct, true);
            // pasture.waterPackage = linkPasturePackage.waterPackage;
            pasture.foodPackage = linkPasturePackage.foodPackage;
            pasture.productPackage = linkPasturePackage.productPackage;

            GameDataSaveManager.instance.UserGameSaveData.SetPastureData(pasture);
        }
    }

    public async void CreatPasture(PastureSaveData pastureSaveData)
    {
        PastureData pastureData = await GameDataManager.instance.GetAsyncData<PastureData>(pastureSaveData.dataId);
        PastureLevelData pastureLevelData = pastureData.levelDatas[0];

        Pasture pasture = new Pasture
        {
            instanceId = pastureSaveData.instanceId,
            name = pastureSaveData.name,
            pastureState = pastureSaveData.pastureState,
            linkItem = pastureSaveData.linkItem,
            foodPackage = pastureSaveData.foodPackage,
            //waterPackage = waterPackageId,
            productPackage = pastureSaveData.productPackage,
            animals = new HashSet<int>(4),
            pastureData = pastureData,
            level = pastureLevelData.level,
            animalCase = pastureLevelData.animalCase,
            linkRoom = pastureSaveData.linkRoom
        };
        pastureLinkItems.Add(pasture.linkItem, pasture.instanceId);


        RefreshPasture refreshPasture = new RefreshPasture
        {
            instanceId = pasture.instanceId
        };
        GameActionManager.instance.QueueAction(refreshPasture,true);
        pastures.Add(pasture.instanceId, pasture);

        SetItemAnimation setItemAnimation = new SetItemAnimation
        {
            id = pastureSaveData.instanceId,
            keyX = pastureLevelData.animationKey.x,
            keyY = pastureLevelData.animationKey.y,
        };
        GameActionManager.instance.QueueAction(setItemAnimation);
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
           await UIManager.instance.ShowGamePanel<ItemCostSelectPanel, ItemCostEventData>(itemCostEventData);

            void CreatPasture(bool result)
            {
                if (result)
                {
                    SetItemAnimation setItemAnimation = new SetItemAnimation
                    {
                        id = tryCreatPasture.itemInstanceId,
                        keyX = pastureLevelData.animationKey.x,
                        keyY = pastureLevelData.animationKey.y,
                    };
                    GameActionManager.instance.QueueAction(setItemAnimation);
                    SetValue(tryCreatPasture.itemInstanceId);

                    WorldMapManager.instance.SaveMapItemInstance(tryCreatPasture.itemInstanceId);
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
                if (!pastureLinkItems.TryGetValue(instanceId, out var pastureInstance))
                {
                    if (pastures.TryGetValue(pastureInstance, out var pasture))
                    {
                        var TryDeleteRoom = new TryDeleteRoom
                        {
                            roomId = tryCreatPasture.roomId
                        };
                        GameActionManager.instance.QueueAction(TryDeleteRoom);

                        pasture.linkItem = instanceId;
                        pasture.level++;
                        pasture.animalCase = pastureLevelData.animalCase;


                        foreach (var animal in animals)
                        {
                            var character =
                                CharacterManager.instance.GetCharacter(animal.Value.instanceId);
                            if (character != null && character.mapInstance == pasture.linkRoom)
                                character.SetCoordinate(new int3(character.coordinate, tryCreatPasture.roomId));
                        }

                        pasture.linkRoom = tryCreatPasture.roomId;
                    }
                }
                else
                {
                    CreatPackage creatFoodPackage = new CreatPackage
                {
                    level = 0,
                    packageDataId = pastureData.foodPackage,
                    setValue = (int foodPackageId) =>
                    {
                        CreatPackage creatWaterPackage = new CreatPackage
                        {
                            level = 0,
                            packageDataId = pastureData.waterPackage,
                            setValue = (int waterPackageId) =>
                            {
                                CreatPackage creatProductPackage = new CreatPackage
                                {
                                    level = 0,
                                    packageDataId = pastureData.productPackage,
                                    setValue = SetPackageInstanceId
                                };
                                GameActionManager.instance.QueueAction(creatProductPackage, true);

                                void SetPackageInstanceId(int packageInstanceId)
                                {
                                    var pasture = new Pasture
                                    {
                                        instanceId = MyInstance.instance.Uid,
                                        name = string.IsNullOrEmpty(tryCreatPasture.pastureName)
                                            ? pastureData.pastureName
                                            : tryCreatPasture.pastureName,
                                        pastureState = PastureState.平常,
                                        linkItem = instanceId,
                                        foodPackage = foodPackageId,
                                        //waterPackage = waterPackageId,
                                        productPackage = packageInstanceId,
                                        animals = new HashSet<int>(4),
                                        pastureData = pastureData,
                                        level = 1,
                                        animalCase = pastureLevelData.animalCase,
                                        linkRoom = tryCreatPasture.roomId
                                    };
                                    pastureLinkItems.Add(instanceId, pasture.instanceId);

                                    var refreshPasture = new RefreshPasture
                                    {
                                        instanceId = pasture.instanceId
                                    };
                                    GameActionManager.instance.QueueAction(refreshPasture);

                                    var simpleTalk = new SimpleTalk
                                    {
                                        characterId = CharacterManager.instance.controllerCharacter.instanceId,
                                        talkId = pastureLevelData.successTalk
                                    };
                                    GameActionManager.instance.QueueAction(simpleTalk);

                                    if (tryCreatPasture.setValue != null) tryCreatPasture.setValue(pasture.instanceId);

                                    if (tryCreatPasture.setResult != null) tryCreatPasture.setResult(true);

                                    pastures.Add(pasture.instanceId, pasture);
                                    GameDataSaveManager.instance.UserGameSaveData.SetPastureData(pasture);
                                }
                            }
                        };
                        GameActionManager.instance.QueueAction(creatWaterPackage, true);
                    }
                };
                GameActionManager.instance.QueueAction(creatFoodPackage, true);
                } 
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
        if (pastures.TryGetValue(tryDeletePasture.instanceId, out var pasture))
        {
            int linkRoom = pasture.instanceId;

            TryDeleteRoom tryDeleteRoom = new TryDeleteRoom
            {
                roomId = linkRoom
            };
            GameActionManager.instance.QueueAction(tryDeleteRoom, true);

            pastures.Remove(pasture.instanceId);
            tryDeletePasture.setResult(true);

            GameDataSaveManager.instance.UserGameSaveData.DeletePasture(pasture.instanceId);
            return;
        }
        tryDeletePasture.setResult(false);
    }

    public async void CreatAnimal(AnimalSaveData animalSaveData)
    {
        AnimalData animalData = await GameDataManager.instance.GetAsyncData<AnimalData>(animalSaveData.dataId);
        Animal animal = new Animal(animalData,animalSaveData.instaceId, animalSaveData.name,animalSaveData.linkCharacterData)
        {  
            animalState = animalSaveData.animalState,
            animalData = animalData, 
            pasture= animalSaveData.pasture,
            growthStage = animalSaveData.growthStage,
            growthDay=animalSaveData.growthDay,
            setFood=animalSaveData.setFood,
            nowCD =animalSaveData.nowCD, 
        };
        if (pastures.TryGetValue(animal.pasture, out var pasture))
         {
            int2 nextCoordinate = MapCellController.instance.GetRandomRoomCell(pasture.linkRoom);
            if (nextCoordinate.x != int.MinValue)
            {
                pasture.animals.Add(animal.instanceId);
            }

            CreatCharacter creatCharacter = new CreatCharacter
            {
                characterId = animalData.linkCharacter,
                mapInstance = pasture.linkRoom,
                coordinateX = nextCoordinate.x,
                coordinateY = nextCoordinate.y,
                hideData = true,
                setResult= SetResult
            };
            GameActionManager.instance.QueueAction(creatCharacter,true);
            RefreshPasture refreshPasture = new RefreshPasture
            {
                instanceId = pasture.instanceId
            };
            GameActionManager.instance.QueueAction(refreshPasture);
        }
        else
        {
            pasture.animals.Add(animal.instanceId);
            Character character = CharacterManager.instance.controllerCharacter;
            CreatCharacter creatCharacter = new CreatCharacter
            {
                characterId = animalData.linkCharacter,
                mapInstance = character.mapInstance,
                coordinateX = character.coordinate.x,
                coordinateY = character.coordinate.y,
                hideData = true,
                setResult = SetResult
            };
            GameActionManager.instance.QueueAction(creatCharacter, true);

            JoinTeam joinTeam = new JoinTeam
            {
                characterId = animalData.linkCharacter,
                teamCharacterId = character.instanceId
            };
            GameActionManager.instance.QueueAction(joinTeam);
        }

        void SetResult(bool result)
        {
            animal.InitBehavior();
        }
       
    }
    private async void SampleCreatAnimal(SampleCreatAnimal sampleCreatAnimal)
    {
        AnimalData animalData = await GameDataManager.instance.GetAsyncData<AnimalData>(sampleCreatAnimal.dataId);
        Animal animal = new Animal(animalData,MyInstance.instance.Uid);

   
        animals.Add(animal.instanceId, animal);
        CreatCharacter creatCharacter = new CreatCharacter
        {
            characterId = animalData.linkCharacter,
            mapInstance = CharacterManager.instance.controllerCharacter.mapInstance,
            coordinateX = CharacterManager.instance.controllerCharacter.coordinate.x,
            coordinateY = CharacterManager.instance.controllerCharacter.coordinate.y,
            instanceId = animal.instanceId,
            setValue = SetAnimalInstanceId,
            hideData = true
        };
        void SetAnimalInstanceId(int value)
        {
            if (value != 0)
            {
                animal.InitBehavior();
                /*
                if (animalData.externalBehavior != null)
                {
                    CharacterBehaviorManager.instance.AddBehavior(animal.instanceId, animalData.externalBehavior);
                }
                */
                GameDataSaveManager.instance.UserGameSaveData.SetAnimalData(animal);
            }
            JoinTeam joinTeam = new JoinTeam
            {
                teamCharacterId = CharacterManager.instance.controllerCharacter.instanceId,
                characterId = value
            };
            GameActionManager.instance.QueueAction(joinTeam);
        }
        GameActionManager.instance.QueueAction(creatCharacter);
    }
    private async void TryCreatAnimal(TryCreatAnimal tryCreatAnimal)
    {
        AnimalData animalData = await GameDataManager.instance.GetAsyncData<AnimalData>(tryCreatAnimal.dataId);
        Animal animal = new Animal(animalData, MyInstance.instance.Uid);

        if (pastures.TryGetValue(tryCreatAnimal.roomId, out var pasture))
        {
            animal.pasture = pasture.instanceId;
        }
        animals.Add(animal.instanceId, animal);
        CreatCharacter creatCharacter = new CreatCharacter
        {
            characterId = animalData.linkCharacter,
            mapInstance = tryCreatAnimal.roomId,
            coordinateX = tryCreatAnimal.coordinate.x,
            coordinateY = tryCreatAnimal.coordinate.y,
            instanceId=animal.instanceId,
            setValue = SetAnimalInstanceId,
            hideData=true
        };
        void SetAnimalInstanceId(int value)
        {
            if (value != 0)
            { 
              
                if (pasture!=null&&pasture.pastureData != null)
                {
                    pasture.animals.Add(animal.instanceId);

                    RefreshPasture refreshPasture = new RefreshPasture
                    {
                        instanceId = pasture.instanceId
                    };
                    GameActionManager.instance.QueueAction(refreshPasture);
                }
                animal.InitBehavior();
                /*
                if (animalData.externalBehavior != null)
                {
                    CharacterBehaviorManager.instance.AddBehavior(animal.instanceId, animalData.externalBehavior);
                }
                */
                GameDataSaveManager.instance.UserGameSaveData.SetAnimalData(animal);
            }
            tryCreatAnimal.setValue(value);
        }
        GameActionManager.instance.QueueAction(creatCharacter);
        // tryCreatAnimal.setValue(-1);
    }

    private void TryDeleteAnimal(TryDeleteAnimal tryDeleteAnimal)
    {
        if (animals.TryGetValue(tryDeleteAnimal.animalId, out var animal))
        {
            if (pastures.TryGetValue(animal.pasture, out var pasture))
            {
                pasture.animals.Remove(animal.instanceId);

                RefreshPasture refreshPasture = new RefreshPasture
                {
                    instanceId = pasture.instanceId
                };
                GameActionManager.instance.QueueAction(refreshPasture);
            }

            InformationController.instance.AddInformation($"+{LanguageManage.SwitchStr(animal.name)}+{LanguageManage.SwitchStr("已经回归到大自然")}", true, true);

            DestoryCharacter destoryCharacter = new DestoryCharacter
            {
                characterId = animal.instanceId,
                isTemp = false
            };
            GameActionManager.instance.QueueAction(destoryCharacter);
            animal.Dispose();
            animals.Remove(animal.Key);

            GameDataSaveManager.instance.UserGameSaveData.DeleteAnimal(animal.Key);
        }
    }
}

[Serializable]
public enum AnimalState
{
    正常 = 0,
    饥饿 = 1,
    高兴 = 2,
    悲伤 = 3,
    死亡 = 4,
    衰老=5
}

[Serializable]
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

public class Pasture : IReferenceData
{
    public int instanceId;
    public int linkItem;
    public PastureState pastureState;
    public HashSet<int> animals;
    public PastureData pastureData;
    public string name;
    public int foodPackage;

    //public int waterPackage;
    public int productPackage;

    public int level;
    public int index;
    public int animalCase;
    public int linkRoom;
    public int Key => instanceId;

    public List<int2> GetPastureCells()
    {
      return  MapCellController.instance.GetItemTriggerCells(linkItem, linkRoom);
    }
}

public class Animal
{
    public int instanceId;
    public string name=>string.IsNullOrEmpty(animalName)?animalData.animalName:animalName;
    private string animalName;

    public int pasture;
    public AnimalData animalData;
    public int growthStage;
    public int growthDay;
    public bool setFood;
    public AnimalState animalState;
    public int nowCD;
    public int linkCharacterData { get; private set; }
    public int Key => instanceId;

    public int GetPastureRoom()
    {
        if(PastureManager.instance.GetPasture(instanceId,out var pasture))
        {
            return pasture.linkRoom;
        }
        return 0;
    }

    public Animal(AnimalData animalData,int instanceId,string animalName=null,int linkCharacterData=0)
    {
        this.animalName = animalName;
        this.animalData = animalData;
        this.instanceId = instanceId;
        this.linkCharacterData = animalData.linkCharacter;
        animalState = AnimalState.正常;
        if (linkCharacterData != 0)
        {
            this.linkCharacterData = linkCharacterData;
        }
        NPCTaskScheduleManager.instance.AddNPCBehavior(instanceId);
    }
    
    public void InitBehavior()
    {
        NPCTaskScheduleManager.instance.SetNPCTaskScheduleTimeList(instanceId, animalData.dailyTasks, animalData.externalBehavior);
    }
    public static Color GetStateColor(AnimalState animalState)
    {
        if (animalState == AnimalState.正常)
        {
            return new Color(0, 0.5f, 0, 1);
        }
        return new Color(0.5f, 0, 0, 1);
    }

    public void Grow()
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
                InformationController.instance.AddInformation($"+{LanguageManage.SwitchStr(name)}+{LanguageManage.SwitchStr("已死亡!")}");
                TryDeleteAnimal tryDeleteAnimal = new TryDeleteAnimal
                {
                    animalId = instanceId,
                };
                GameActionManager.instance.QueueAction(tryDeleteAnimal);
            }
            return;
        }
        var growthStateData = animalData.growthStages[growthStage];
        if (animalState != AnimalState.死亡)
        {
            nowCD++;
             Team team = TeamManager.instance.GetTeam(instanceId);
            if (animalState != AnimalState.饥饿&& nowCD >= animalData.productCD)
            { 
                if (team != null)
                {
                    if(growthStateData.productValue != 0) 
                    {
                        AddPackageItem addPackageItem = new AddPackageItem
                        {
                            packageId = team.leader.characterPackage,
                            itemDataId = growthStateData.productValue,
                            itemCount = animalData.productCount
                        };
                        GameActionManager.instance.QueueAction(addPackageItem);
                        nowCD=0;
                    }
                   
                }
                else if(growthStateData.productValue != 0)
                {
                    TrySetItemToPastureBox trySetItemToPastureBox = new TrySetItemToPastureBox
                    {
                        itemId = growthStateData.productValue,
                        itemCount = animalData.productCount,
                        pastureId = pasture,
                    };
                    GameActionManager.instance.QueueAction(trySetItemToPastureBox);
                    nowCD = 0;
                }

                ShowEmote showEmote = new ShowEmote
                {
                    emoteId = animalData.productEmote,
                    entityType = EntityType.角色,
                    id = instanceId
                };
                GameActionManager.instance.QueueAction(showEmote);
            }
            if (team != null)
            {
                CostFoodResult(false);
            }
            else
            {
                AnimalCostFood animalCostFood = new AnimalCostFood
                {
                    animalDataId = animalData.id,
                    pastureId = pasture,
                    setResult = CostFoodResult
                };
                GameActionManager.instance.QueueAction(animalCostFood, true);
            }
        }
        growthDay++; 
        if (growthDay >= growthStateData.growthHour)
        {
            int index = growthStage + 1;
            if (index < animalData.growthStages.Count - 1)
            {
                growthStage = index;
                growthDay = 0;

                int stageValue = animalData.growthStages[index].stageObj;
                if (stageValue != 0 && linkCharacterData != stageValue)
                {
                    ChangeCharacter changeCharacter = new ChangeCharacter
                    {
                        instanceId = instanceId,
                        newDataId = stageValue
                    };
                    GameActionManager.instance.QueueAction(changeCharacter);
                    linkCharacterData = stageValue;
                }
            }
            else
            {
                animalState = AnimalState.衰老;
                InformationController.instance.AddInformation($"+{LanguageManage.SwitchStr(name)}+{LanguageManage.SwitchStr("已衰老!")}");
                /*
                TryDeleteAnimal tryDeleteAnimal = new TryDeleteAnimal
                {
                    animalId = instaceId,
                };
                GameActionManager.instance.QueueAction(tryDeleteAnimal);*/
            }
        }
    }

    private void CostFoodResult(bool result)
    {
        this.setFood = result;
        if (result)
        {
            animalState = AnimalState.正常;
            TryRecycleCharacterEmote tryRecycleCharacterEmote = new TryRecycleCharacterEmote
            {
                id = linkCharacterData
            };
            GameActionManager.instance.QueueAction(tryRecycleCharacterEmote);
        }
        else
        {
            if (animalState == AnimalState.饥饿)
            {
                animalState = AnimalState.死亡;
                TryDeleteAnimal tryDeleteAnimal = new TryDeleteAnimal
                {
                    animalId = instanceId,
                };
                GameActionManager.instance.QueueAction(tryDeleteAnimal);
            }
            else
            {
                animalState = AnimalState.饥饿;
                var character = CharacterManager.instance.GetCharacter(linkCharacterData);
                if (character.mapInstance == WorldMapObjManager.instance.displayMap)
                {
                    TryUpDataCharacterEmote tryUpDataCharacterEmote = new TryUpDataCharacterEmote
                    {
                        emote = GameCommon.animalNeedFood,
                        id = linkCharacterData,
                        showTime = -1
                    };
                    GameActionManager.instance.QueueAction(tryUpDataCharacterEmote);
                }
            }
        }
    }

    public void Dispose()
    {
        NPCTaskScheduleManager.instance.RemoveBehavior(instanceId);
    }
}