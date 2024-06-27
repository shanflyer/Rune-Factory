using System;
using System.Collections.Generic;
using Unity.Mathematics; 

public class FishingManager : Singleton<FishingManager>
{
    private Dictionary<int2, FishPondData> fishPondDatas = new Dictionary<int2, FishPondData>();
    private MyInstance myInstance;

    public override async void Init()
    {
        base.Init();

        fishPondDatas.Clear();
        var allData = await GameDataManager.instance.GetAllAsyncData<FishPondData>();
        for (int i = 0; i < allData.Count; i++)
        {
            var data = allData[i];
            fishPondDatas[data.linkMapItem] = data;
        }
        GameActionManager.instance.AddListener<FishingIsSuccess>(FishingIsSuccess);
        GameActionManager.instance.AddListener<StopFishing>(StopFishing);
        GameActionManager.instance.AddListener<StartFishing>(StartFishing);
        GameActionManager.instance.AddListener<DisplayMap>(DisplayMap);
    }

    protected override void Clear()
    {
        base.Clear();
        if (myInstance != null)
        {
            myInstance.Clear();
        }
        if (!SingletonType.Cleared)
        {
            GameActionManager.instance.RemoveListener<FishingIsSuccess>(FishingIsSuccess);
            GameActionManager.instance.RemoveListener<StopFishing>(StopFishing);
            GameActionManager.instance.RemoveListener<StartFishing>(StartFishing);
            GameActionManager.instance.RemoveListener<DisplayMap>(DisplayMap);
        } 
    }

    private async void FishingIsSuccess(FishingIsSuccess fishingIsSuccess)
    {
        bool isController = CharacterManager.instance.controllerCharacter.instanceId == fishingIsSuccess.characterId;
        if (fishingIsSuccess.isSuccess)
        {
            var fishPondData = fishingIsSuccess.pondData;
            Season season = GameTimeManager.instance.Season;
            if (!fishPondData.seasonRandomValue.TryGetValue(season, out var randomId))
            {
                fishPondData.seasonRandomValue.TryGetValue(Season.Default, out randomId);
            }
            var randomResults = GameRandom.instance.GetRandomValue(randomId,countValue:fishingIsSuccess.fishValue);
            if (randomResults.Count > 0)
            {
                var randomResult = randomResults[0];
                int fishDataId = int.Parse(randomResult.result);

                FishData fishData = await GameDataManager.instance.GetAsyncData<FishData>(fishDataId);
                Item item = new Item
                {
                    dataId = fishData.itemId,
                    count = 1,
                    value = randomResult.count
                };
                Character character = CharacterManager.instance.GetCharacter(fishingIsSuccess.characterId);
                int count = await PackageManager.instance.SetItemInPackage(item, character.characterPackage);
                if (count <= 0)
                { 
                    if (isController)
                    { 
                        ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(fishData.itemId);
                        bool newRecord = GameDataSaveManager.instance.SetFishSaveData(fishData.id, randomResult.count, character.mapInstance);
                        ItemResultInfo itemResultInfo = new ItemResultInfo
                        {
                            icon = itemData.icon,
                            info0 = $"获得了一条  <color=green>{item.value}</color>cm<color=#02B8E3> {itemData.itemName} </color>!",
                            info1 = newRecord ? $"<color=red> 新记录！ </color>" : ""
                        };
                        UIManager.instance.ShowGamePanel<ItemResultPanel, ItemResultInfo>(itemResultInfo);
                    }
                    NPCFishingResult nPCFishingResult = new NPCFishingResult
                    {
                        characterId = fishingIsSuccess.characterId,
                        success = true
                    };
                    GameActionManager.instance.QueueAction(nPCFishingResult);
                }
                else
                {
                    if (isController)
                    {
                        ItemResultInfo itemResultInfo = new ItemResultInfo
                        {
                            icon = null,
                            info0 = "",
                            info1 = "$背包空间不足，鱼已放生"
                        };
                        UIManager.instance.ShowGamePanel<ItemResultPanel, ItemResultInfo>(itemResultInfo);
                    }
                    NPCFishingResult nPCFishingResult = new NPCFishingResult
                    {
                        characterId = fishingIsSuccess.characterId,
                        success = false
                    };
                    GameActionManager.instance.QueueAction(nPCFishingResult);
                }
            }
        }
        else
        {
            if (isController)
            {
                ItemResultInfo itemResultInfo = new ItemResultInfo
                {
                    icon = null,
                    info0 = "",
                    info1 = "本次垂钓一无所获"
                };
                UIManager.instance.ShowGamePanel<ItemResultPanel, ItemResultInfo>(itemResultInfo);
            }
            NPCFishingResult nPCFishingResult = new NPCFishingResult
            {
                characterId = fishingIsSuccess.characterId,
                success = false
            };
            GameActionManager.instance.QueueAction(nPCFishingResult);
        }
    }

    private Dictionary<int, Delegate> waitFishers = new Dictionary<int, Delegate>();
    private Dictionary<int, Delegate> fishWaitActions = new Dictionary<int, Delegate>();
    private HashSet<int> fishers = new HashSet<int>();

    void DisplayMap(DisplayMap displayMap)
    {
        foreach(var fisher in fishers)
        {
            Character character = CharacterManager.instance.GetCharacter(fisher);
            if (character.mapInstance != displayMap.displayMap)
            {
                RecycleFisher recycleFisher = new RecycleFisher
                {
                    characterInstance = fisher
                };
                GameActionManager.instance.QueueAction(recycleFisher);
            }
            else
            {
                CreatFisher creatFisher = new CreatFisher
                {
                    characterInstance = fisher
                };
                GameActionManager.instance.QueueAction(creatFisher);
            }
        }
    }
    private void StopFishing(StopFishing stopFishing)
    {
        int characterId = stopFishing.characterId;
        if (waitFishers.TryGetValue(characterId, out var @delegate))
        {
            GameTimerController.instance.RemoveWaiter(@delegate);
            waitFishers.Remove(characterId);
        }
        else if (fishWaitActions.TryGetValue(characterId, out @delegate))
        {
            GameTimerController.instance.RemoveWaiter(@delegate);
            fishWaitActions.Remove(characterId);
        }
    }

    private void StartFishing(StartFishing startFishing)
    {
        int mapItemId = startFishing.mapItemId;
        int characterId = startFishing.characterId;
        Character character = CharacterManager.instance.GetCharacter(characterId);
        int mapId = character.mapInstance;
        int2 key = new int2(mapId, mapItemId);
        if (fishPondDatas.TryGetValue(key, out var fishPondData))
        {
            SetCharacterAnimator setCharacterAnimator = new SetCharacterAnimator
            {
                characterId = characterId,
                parameterType = ParameterType.BOOL,
                parameter = "Fish",
                boolValue = true
            };
            GameActionManager.instance.QueueAction(setCharacterAnimator);

            int waitFishingTime = GameRandom.RandomInt(fishPondData.waitFishingCd.x, fishPondData.waitFishingCd.y);
            GameTimerController.instance.DelayAction(waitFishingTime, FishingAction);
            if (waitFishers.TryGetValue(characterId, out var @delegate))
            {
                GameTimerController.instance.RemoveWaiter(@delegate);
            }
            AddWaitFisher(characterId, FishingAction);

            void FishingAction()
            {
                StartFishingGame startFishingGame = new StartFishingGame
                {
                    characterId = characterId, 
                };
                GameActionManager.instance.QueueAction(startFishingGame);
                waitFishers.Remove(characterId);
                fishers.Add(characterId);

                if (fishWaitActions.TryGetValue(characterId, out var @delegate))
                {
                    GameTimerController.instance.RemoveWaiter(@delegate);
                }
                GameTimerController.instance.DelayAction(GameCommon.fishingGameTime, WaitFishingGame);
                AddFishingGame(characterId, WaitFishingGame);
                void WaitFishingGame()
                {
                    if (characterId == CharacterManager.instance.controllerCharacter.instanceId)
                    {
                        FishingIsSuccess fishingIsSuccess = new FishingIsSuccess
                        {
                            characterId = characterId,
                            isSuccess = false
                        };
                        GameActionManager.instance.QueueAction(fishingIsSuccess);
                    }
                    else
                    {
                        int randomValue = GameRandom.RandomInt(0, 100);
                        if (randomValue < 50)
                        {
                            FishingIsSuccess fishingIsSuccess = new FishingIsSuccess
                            {
                                characterId = characterId,
                                isSuccess = false
                            };
                            GameActionManager.instance.QueueAction(fishingIsSuccess);
                        }
                        else
                        {
                            FishingIsSuccess fishingIsSuccess = new FishingIsSuccess
                            {
                                characterId = characterId,
                                pondData = fishPondData,
                                isSuccess = true
                            };
                            GameActionManager.instance.QueueAction(fishingIsSuccess);
                        }
                    }
                    fishers.Remove(characterId);
                    fishWaitActions.Remove(characterId);
                }
            }

            if(mapId== WorldMapObjManager.instance.displayMap)
            {
                CreatFisher creatFisher = new CreatFisher
                {
                    characterInstance = characterId,
                    pondData= fishPondData,
                };
                GameActionManager.instance.QueueAction(creatFisher);
            }
        }
    }

    private void AddWaitFisher(int characterId, Action action)
    {
        waitFishers[characterId] = action;
    }

    private void AddFishingGame(int characterId, Action action)
    {
        fishWaitActions[characterId] = action;
    }
}