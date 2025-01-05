using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine.TextCore.Text;

public class FishingManager : Singleton<FishingManager>
{
    private Dictionary<int2, FishPondData> fishPondDatas = new Dictionary<int2, FishPondData>(); 

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
        GameActionManager.instance.AddListener<CheckPlayFishingAction>(CheckPlayFishingAction);
    }

    protected override void Clear()
    {
        base.Clear(); 
        if (!SingletonType.Cleared)
        {
            GameActionManager.instance.RemoveListener<FishingIsSuccess>(FishingIsSuccess);
            GameActionManager.instance.RemoveListener<StopFishing>(StopFishing);
            GameActionManager.instance.RemoveListener<StartFishing>(StartFishing);
            GameActionManager.instance.RemoveListener<DisplayMap>(DisplayMap);
            GameActionManager.instance.RemoveListener<CheckPlayFishingAction>(CheckPlayFishingAction);
        }
    }

    private async void FishingIsSuccess(FishingIsSuccess fishingIsSuccess)
    {
        bool isController = CharacterManager.instance.controllerCharacter.instanceId == fishingIsSuccess.characterId;
        if (isController)
        {
           int itemInstance= WorldMapManager.instance.GetInstanceFromEditorId(fishingIsSuccess.pondData.linkMapItem);

            RemoveMapItemOperate removeOperateData = new RemoveMapItemOperate
            {
                mapItemId = itemInstance,
                removeOperateId = GameCommon.GetFish
            };
            GameActionManager.instance.QueueAction(removeOperateData);
            AddMapItemOperate addOperateData = new AddMapItemOperate
            {
                mapItemId = itemInstance,
                addeOperateId = GameCommon.StartFish
            };
            GameActionManager.instance.QueueAction(addOperateData);

            ShowMapObjTips ShowMapObjTips = new ShowMapObjTips
            {
                id = itemInstance,
            };
            GameActionManager.instance.QueueAction(ShowMapObjTips);

            UIManager.instance.ShowGamePanel<ScreenControllerPanel>();
        }

        if(waitFishers.TryGetValue(fishingIsSuccess.characterId,out var action))
        {
            GameTimerController.instance.RemoveWaiter(action);
            waitFishers.Remove(fishingIsSuccess.characterId);
        }
        else if (fishWaitActions.TryGetValue(fishingIsSuccess.characterId, out var @delegate))
        {
            GameTimerController.instance.RemoveWaiter(@delegate);
            fishWaitActions.Remove(fishingIsSuccess.characterId);
        }

        if (fishingIsSuccess.isSuccess)
        {
            var fishPondData = fishingIsSuccess.pondData;
            Season season = GameTimeManager.instance.Season;
            if (!fishPondData.seasonRandomValue.TryGetValue(season, out var randomId))
            {
                fishPondData.seasonRandomValue.TryGetValue(Season.Default, out randomId);
            }
            var randomResults = GameRandom.instance.GetRandomValue(randomId, countValue: fishingIsSuccess.fishValue);
            if (randomResults.Count > 0)
            {
                var randomResult = randomResults[0];
                int fishDataId = randomResult.x;

                FishData fishData = await GameDataManager.instance.GetAsyncData<FishData>(fishDataId);
                Item item = new Item
                {
                    dataId = fishData.itemId,
                    count = 1, 
                };
                item = await Item.SetValue(item, randomResult.y);
                Character character = CharacterManager.instance.GetCharacter(fishingIsSuccess.characterId);
                int count = await PackageManager.instance.SetItemInPackage(item, character.characterPackage);
                if (count <= 0)
                {
                    if (isController)
                    {
                        ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(fishData.itemId);
                        bool newRecord = GameDataSaveManager.instance.SetFishSaveData(fishData.id, randomResult.y, character.mapInstance);
                        ItemResultInfo itemResultInfo = new ItemResultInfo
                        {
                            icon = itemData.icon,
                            info0 = $"获得了一条  <color=green>{randomResult.y}</color>cm<color=#02B8E3> {itemData.itemName} </color>!",
                            info1 = newRecord ? $"<color=red> 新记录！ </color>" : ""
                        };
                      await  UIManager.instance.ShowGamePanel<ItemResultPanel, ItemResultInfo>(itemResultInfo);
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
                            info1 = "背包空间不足，鱼已放生"
                        };
                      await  UIManager.instance.ShowGamePanel<ItemResultPanel, ItemResultInfo>(itemResultInfo);
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
               await UIManager.instance.ShowGamePanel<ItemResultPanel, ItemResultInfo>(itemResultInfo);
            }
            NPCFishingResult nPCFishingResult = new NPCFishingResult
            {
                characterId = fishingIsSuccess.characterId,
                success = false
            };
            GameActionManager.instance.QueueAction(nPCFishingResult);
        }
    }

    private Dictionary<int, Action> waitFishers = new Dictionary<int, Action>();
    private Dictionary<int, Action> fishWaitActions = new Dictionary<int, Action>();
    private HashSet<int> fishers = new HashSet<int>();

    private void CheckPlayFishingAction(CheckPlayFishingAction checkPlayFishingAction)
    {
        bool playerIsFisher = waitFishers.ContainsKey(CharacterManager.instance.controllerCharacter.instanceId);
        if (checkPlayFishingAction.setResult != null)
        {
            checkPlayFishingAction.setResult(playerIsFisher == checkPlayFishingAction.isFishing);
        }
    }
    private void DisplayMap(DisplayMap displayMap)
    {
        foreach (var fisher in fishers)
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
        if (stopFishing.characterId == CharacterManager.instance.controllerCharacter.instanceId)
        {
            ShowMapObjTips showMapObjTips = new ShowMapObjTips
            {
                id = CharacterManager.instance.controllerCharacter.OperateItem
            };
            GameActionManager.instance.QueueAction(showMapObjTips);
        }
       
        UIManager.instance.ShowGamePanel<ScreenControllerPanel>();
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
        RemovePackageItem removePackageItem = new RemovePackageItem
        {
            itemCount = 1,
            itemDataId = 12,
            packageId = character.characterPackage
        };
        GameActionManager.instance.QueueAction(removePackageItem);
         
        UIManager.instance.CloseGamePanel<ScreenControllerPanel>();
        if(WorldMapManager.instance.GetRuntimeMapItem(startFishing.mapItemId,out var runtimeMapItem))
        {
            key = new int2(mapId, runtimeMapItem.editorInstanceId);
            /*
            RemoveMapItemOperate removeOperateData = new RemoveMapItemOperate
            {
                mapItemId = startFishing.mapItemId,
                removeOperateId = GameCommon.StartFish
            };
            GameActionManager.instance.QueueAction(removeOperateData);
            AddMapItemOperate addOperateData = new AddMapItemOperate
            {
                mapItemId = startFishing.mapItemId,
                addeOperateId = GameCommon.GetFish
            };
            GameActionManager.instance.QueueAction(addOperateData);*/
            ShowMapObjTips showMapObjTips = new ShowMapObjTips
            {
                id = startFishing.mapItemId
            };
            GameActionManager.instance.QueueAction(showMapObjTips);
        }

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

            Action action = FishingAction;

            GameTimerController.instance.DelayAction(waitFishingTime, action);
            if (waitFishers.TryGetValue(characterId, out var @delegate))
            {
                GameTimerController.instance.RemoveWaiter(@delegate);
            }
            AddWaitFisher(characterId, action);

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
                            pondData=fishPondData,
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
                                pondData = fishPondData,
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

            if (mapId == WorldMapObjManager.instance.displayMap)
            {
                CreatFisher creatFisher = new CreatFisher
                {
                    characterInstance = characterId,
                    pondData = fishPondData,
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