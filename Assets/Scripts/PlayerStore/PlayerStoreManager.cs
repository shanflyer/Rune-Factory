using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PlayerStoreManager : Singleton<PlayerStoreManager>
{
    public Dictionary<int,RuntimeStoreCounter> RuntimeStoreCounters => runtimeStoreCounters;
    private Dictionary<int, RuntimeStoreCounter> runtimeStoreCounters;
    private Dictionary<int, RuntimeObj> nowRuntimeStoreCounterObjs = new Dictionary<int, RuntimeObj>();
    private System.Threading.Tasks.Task initializationTask = System.Threading.Tasks.Task.CompletedTask;
    public override System.Threading.Tasks.Task InitializationTask => initializationTask;

    private SellItem sellItem;
    AnimationCurve timeCurve,weatherCurve;

    public bool playerStoreOpen
    {
        get => GameDataSaveManager.instance.UserGameSaveData.otherSaveData.playerStoreOpen;
        private set => GameDataSaveManager.instance.UserGameSaveData.otherSaveData.playerStoreOpen = value;
    }
    protected override void Clear()
    {
        initializationTask = System.Threading.Tasks.Task.CompletedTask;
        base.Clear();
    }

    public override void Init()
    {
        base.Init();
        initializationTask = InitAsync();
    }

    private async System.Threading.Tasks.Task InitAsync()
    {
        var storeShow = StoreShow.instance;
        await storeShow.WaitForInitialization();
        var timeCurveData = await GameDataManager.instance.GetAsyncData<GrowModelData>(GameCommon.timeStoreCurveData);
        timeCurve = timeCurveData.curve;
        var weatherCurveData = await GameDataManager.instance.GetAsyncData<GrowModelData>(GameCommon.weatherStoreCurveData);
        weatherCurve = weatherCurveData.curve;

        var playerStorePrefab = await GameSourceManager.instance.GetPrefab(DataPath.StoreCounterPrefab);
        if (playerStorePrefab == null)
        {
            Debug.LogError($"PlayerStoreManager init failed: missing prefab '{DataPath.StoreCounterPrefab}'.");
            return;
        }
        sellItem = playerStorePrefab.GetComponent<SellItem>();
        if (sellItem == null)
        {
            Debug.LogError($"PlayerStoreManager init failed: prefab '{DataPath.StoreCounterPrefab}' missing SellItem.");
            return;
        }
        runtimeStoreCounters=new Dictionary<int, RuntimeStoreCounter>();
        GameActionManager.instance.AddAsyncListener<CreatStoreCounter>(CreatStoreCounterAsync, nameof(CreatStoreCounter));
        GameActionManager.instance.AddAsyncListener<DisplayStoreCounter>(DisplayStoreCounterAsync, nameof(DisplayStoreCounter));
        GameActionManager.instance.AddListener<DeleteMapItem>(DeleteStoreCounter);
        GameActionManager.instance.AddAsyncListener<StoreCounterSetSelectItemAction>(StoreCounterSetSelectItemActionAsync, nameof(StoreCounterSetSelectItemAction));
        GameActionManager.instance.AddAsyncListener<SetStoreCounterItem>(SetStoreCounterItemAsync, nameof(SetStoreCounterItem));
        GameActionManager.instance.AddAsyncListener<SetStoreCounter>(SetStoreCounterAsync, nameof(SetStoreCounter));
        GameActionManager.instance.AddListener<BuyPlayerGood>(BuyPlayerGood);
        GameActionManager.instance.AddListener<TryBuyPlayerGood>(TryBuyPlayerGood);
        GameActionManager.instance.AddListener<SetPlayerStoreOpen>(SetPlayerStoreOpen);
        GameActionManager.instance.AddListener<SwitchAutoStore>(SwitchAutoStore);
        GameActionManager.instance.AddListener<CheckPlayerStoreOpen>(CheckPlayerStoreOpen);
    }
    void CheckPlayerStoreOpen(CheckPlayerStoreOpen checkPlayerStoreOpen)
    {
        if (checkPlayerStoreOpen.setResult != null)
        {
            checkPlayerStoreOpen.setResult(playerStoreOpen);
        }
    }
    void SwitchAutoStore(SwitchAutoStore SwitchAutoStore)
    {
        if (nowAutoTask.IsValid)
        {
            GameObjectCurveController.instance.Cancel(nowAutoTask);
            nowAutoTask = default;
        }
        if (SwitchAutoStore.isAuto&& playerStoreOpen)
        {
            StartAutoCustomerTask();
        }
    }

    public float GetCustomerCD()
    {
        if (WorldMapObjManager.instance.displayMap == GameCommon.MyPlayerStore)
        {
            int storeCount = 0;
            using (var e = RuntimeStoreCounters.Values.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    if (e.Current.count > 0)
                    {
                        storeCount++;
                    }
                }
            }
            float cdValue = storeCount / 5.0f;
            cdValue = math.clamp(cdValue, 1, 3);

            float nowCd = GameRandom.RandomFloat(GameCommon.autoCustomerCD);
            nowCd += timeCurve.Evaluate(GameTimeManager.instance.timeValue);
            nowCd += weatherCurve.Evaluate(EnvironmentManger.instance.nowWaterFall);
            nowCd = nowCd / cdValue;
            return nowCd;
        }
        return 0;
    }

    GameObjectCurveController.FrameTaskHandle nowAutoTask;
    void StartAutoCustomerTask()
    {
        float timeValue = 0;
        float nowCd = GameRandom.RandomFloat(GameCommon.autoCustomerCD);
        List<RuntimeStoreCounter> nowRuntimeStoreCounters = new List<RuntimeStoreCounter>();
        nowAutoTask = GameObjectCurveController.instance.StartFrameTask((float deltaTime) =>
        {
            timeValue += deltaTime;
            if (timeValue > nowCd)
            {
                nowRuntimeStoreCounters.Clear();
                using (var e= RuntimeStoreCounters.Values.GetEnumerator())
                {
                    while (e.MoveNext())
                    {
                        if (e.Current.count > 0)
                        {
                            nowRuntimeStoreCounters.Add(e.Current);
                        }
                    }
                }
                if (nowRuntimeStoreCounters.Count > 0)
                {
                    int index = GameRandom.RandomInt(0, nowRuntimeStoreCounters.Count);
                    RuntimeStoreCounter runtimeStoreCounter = nowRuntimeStoreCounters[index];
                    TryBuyPlayerGood buyPlayerGood = new TryBuyPlayerGood
                    {
                        storeCounterId = runtimeStoreCounter.instanceId,
                    };
                    TryBuyPlayerGood(buyPlayerGood);
                }
                float cdValue = nowRuntimeStoreCounters.Count / 5.0f;
                cdValue = math.clamp(cdValue, 1, 3);

                nowCd = GameRandom.RandomFloat(GameCommon.autoCustomerCD);
                nowCd += timeCurve.Evaluate(GameTimeManager.instance.timeValue);
                nowCd += weatherCurve.Evaluate(EnvironmentManger.instance.nowWaterFall);
                nowCd = nowCd / cdValue;
            }
            return true;
        }, () => nowAutoTask = default);
    }
    void SetPlayerStoreOpen(SetPlayerStoreOpen setPlayerStoreOpen)
    {
        if (playerStoreOpen != setPlayerStoreOpen.open)
        {
            playerStoreOpen = setPlayerStoreOpen.open;
            if (playerStoreOpen)
            {
                StartCreatTempCharacter startCreatTempCharacter = new StartCreatTempCharacter
                {
                    creatDataId = 1,
                    overrideMaxCount= runtimeStoreCounters.Count==0?2:runtimeStoreCounters.Count*2,
                };
                GameActionManager.instance.QueueAction(startCreatTempCharacter,true);
            }
            else
            {
                StopTempCharacterCreat stopTempCharacterCreat = new StopTempCharacterCreat(); 
                GameActionManager.instance.QueueAction(stopTempCharacterCreat, true);
            }
            
        }  
    }
    private  void TryBuyPlayerGood(TryBuyPlayerGood buyPlayerGood)
    {
        if (runtimeStoreCounters.TryGetValue(buyPlayerGood.storeCounterId, out var runtimeStoreCounter))
        {
            if (runtimeStoreCounter.count <= 0)
            {
                if (buyPlayerGood.setResult != null)
                    buyPlayerGood.setResult(false);
                return;
            }
            if (nowRuntimeStoreCounterObjs.TryGetValue(buyPlayerGood.storeCounterId, out var runtimeObj))
            {
                ShowCoin showCoin = new ShowCoin
                {
                    pos = (runtimeObj.obj as SellItem).transform.position,
                };
                GameActionManager.instance.QueueAction(showCoin);
            }
            ItemData itemData = runtimeStoreCounter.itemData;
            PayManager.instance.AddGold(itemData.sellPrice);
            runtimeStoreCounter.count--;
            if (runtimeStoreCounter.count <= 0)
            {
                runtimeStoreCounter.itemData=null;
                runtimeStoreCounter.count = 0;
            } 
            if (runtimeObj!=null&&runtimeObj.obj != null)
            {
                (runtimeObj.obj as SellItem).SetItemCount(runtimeStoreCounter.count);
            }
 
            if (buyPlayerGood.setResult != null)
                buyPlayerGood.setResult(true);
        }
    }

    private void BuyPlayerGood(BuyPlayerGood buyPlayerGood)
    {
        if (runtimeStoreCounters.TryGetValue(buyPlayerGood.storeCounterId, out var runtimeStoreCounter))
        {
            if (runtimeStoreCounter.count <= 0)
            {
                return;
            }
            if (nowRuntimeStoreCounterObjs.TryGetValue(buyPlayerGood.storeCounterId, out var runtimeObj))
            {
                ShowCoin showCoin = new ShowCoin
                {
                    pos = (runtimeObj.obj as SellItem).transform.position,
                };
                GameActionManager.instance.QueueAction(showCoin);
            }

            runtimeStoreCounter.count--;
            if (runtimeStoreCounter.count > 0)
            { 
                if (runtimeObj.obj != null)
                {
                    (runtimeObj.obj as SellItem).AddItemCount(-1);
                }

                ItemData itemData = runtimeStoreCounter.itemData;
                PayManager.instance.AddGold(itemData.sellPrice);
            }
            else
            {
                runtimeStoreCounter.itemData = null;
                runtimeStoreCounter.count = 0; 
                if (runtimeObj.obj != null)
                {
                    GameRuntimeObjManager.instance.RecycleRuntimeObj(runtimeObj);
                }
            } 
        }
    }

    private async System.Threading.Tasks.Task SetStoreCounterAsync(SetStoreCounter setStoreCounter)
    {
        int characterId = setStoreCounter.playerId;
        if (runtimeStoreCounters.TryGetValue(setStoreCounter.storeCounterId, out var runtimeStoreCounter))
        {
            if (runtimeStoreCounter.count == 0)
            {
                var gameActionData = await GameDataManager.instance.GetAsyncData<GameActionAsset>(setStoreCounter.nullAction);
                gameActionData.Action(characterId, setStoreCounter.storeCounterId);
            }
            else
            {
                OpenStoreCounter(setStoreCounter.storeCounterId);
            } 
        }
    }

    //设置背包界面物体Action
    private async System.Threading.Tasks.Task StoreCounterSetSelectItemActionAsync(StoreCounterSetSelectItemAction storeCounterSetSelectItemAction)
    {
        if (runtimeStoreCounters.TryGetValue(storeCounterSetSelectItemAction.targetObj,out var runtimeStoreCounter))
        { 
            var storeData = runtimeStoreCounter.storeCounterData;
            var itemMatchData = new ItemMatchData
            {
                itemMatchType = ItemMatchType.ItemType,
                matchValues = new HashSet<int>()
            };
            for(int i = 0; i < storeData.itemTypes.Count; i++)
            {
                itemMatchData.matchValues.Add((int)storeData.itemTypes[i]);
            }
            PackageList packageList = new PackageList
            {
                canSetShortcut = false,
                packageDatas = new List<PackageData>
                {
                     PackageManager.instance.GetPackageData(CharacterManager.instance.controllerCharacter.characterPackage)
                 },
                itemMatchData = itemMatchData
            };
             
            WarehousePanel warehousePanel = await UIManager.instance.ShowGamePanel<WarehousePanel,PackageList>(packageList);
            warehousePanel.SetSelectItemAction((Item item, int index, bool select) =>
            {
                UIManager.instance.CloseGamePanel<WarehousePanel>();
                OpenSetItemPanel(storeCounterSetSelectItemAction.targetObj, item);
            }, "选择");
        }
    }

    private void OpenStoreCounter(int storeId)
    {
        AsyncTaskRunner.Run(() => OpenStoreCounterAsync(storeId), nameof(OpenStoreCounter));
    }

    private async System.Threading.Tasks.Task OpenStoreCounterAsync(int storeId)
    {
        if (runtimeStoreCounters.TryGetValue(storeId, out var runtimeStoreCounter))
        {
            int itemDataId= runtimeStoreCounter.itemData==null?0: runtimeStoreCounter.itemData.id;
            SetStoreCounterItem setStoreCounterItem = new SetStoreCounterItem
            {
                storeCounterId = storeId,
                itemId = itemDataId,
                count = runtimeStoreCounter.count
            };
           await UIManager.instance.ShowGamePanel<StoreCounterSetPanel, SetStoreCounterItem>(setStoreCounterItem);
        }
    }

    private void OpenSetItemPanel(int storeId, Item item)
    {
        AsyncTaskRunner.Run(() => OpenSetItemPanelAsync(storeId, item), nameof(OpenSetItemPanel));
    }

    private async System.Threading.Tasks.Task OpenSetItemPanelAsync(int storeId, Item item)
    {
        SetStoreCounterItem setStoreCounterItem = new SetStoreCounterItem
        {
            storeCounterId = storeId,
            itemId = item.dataId,
        };
        if (runtimeStoreCounters.TryGetValue(storeId, out var runtimeStoreCounter))
        {
            setStoreCounterItem.count = runtimeStoreCounter.count; 
        }
       await UIManager.instance.ShowGamePanel<StoreCounterSetPanel, SetStoreCounterItem>(setStoreCounterItem);
    }

    private async System.Threading.Tasks.Task SetStoreCounterItemAsync(SetStoreCounterItem setStoreCounterItem)
    {
        if (runtimeStoreCounters.TryGetValue(setStoreCounterItem.storeCounterId, out var runtimeStoreCounter))
        {
            runtimeStoreCounter.itemData =await GameDataManager.instance.GetAsyncData<ItemData>(setStoreCounterItem.itemId);
            runtimeStoreCounter.count = setStoreCounterItem.count;
            if (runtimeStoreCounter.count == 0)
            {
                runtimeStoreCounter.itemData=null;
            } 

            if (nowRuntimeStoreCounterObjs.TryGetValue(setStoreCounterItem.storeCounterId, out var runtimeObj))
            {
                SellItem sellItem = runtimeObj.obj as SellItem;
                if (sellItem != null)
                {
                    sellItem.InitReferenceData(new Item
                    {
                        dataId = setStoreCounterItem.itemId,
                        count = setStoreCounterItem.count
                    });
                }
            }
 
        }
    }
    public void CreatStoreCounter(StoreCounterSaveData storeCounterSaveData)
    {
        AsyncTaskRunner.Run(() => CreatStoreCounterAsync(storeCounterSaveData), nameof(CreatStoreCounter));
    }

    public async System.Threading.Tasks.Task CreatStoreCounterAsync(StoreCounterSaveData storeCounterSaveData)
    {
        var storeData = await GameDataManager.instance.GetAsyncData<StoreCounterData>(storeCounterSaveData.dataId);
        if (storeData != null)
        {
            RuntimeStoreCounter runtimeStoreCounter = new RuntimeStoreCounter
            {
                instanceId = storeCounterSaveData.instanceId,
                storeCounterData = storeData,

            };
            runtimeStoreCounters.Add(storeCounterSaveData.instanceId, runtimeStoreCounter);

            ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(storeCounterSaveData.itemDataId);
            runtimeStoreCounter.itemData = itemData;
            runtimeStoreCounter.count = storeCounterSaveData.count;
        }
    }

    private async System.Threading.Tasks.Task CreatStoreCounterAsync(CreatStoreCounter creatStoreCounter)
    {
        if (!runtimeStoreCounters.ContainsKey(creatStoreCounter.itemInstanceId))
        {
            var storeData = await GameDataManager.instance.GetAsyncData<StoreCounterData>(creatStoreCounter.storeDataId);

            if (storeData!=null)
            {
                RuntimeStoreCounter runtimeStoreCounter = new RuntimeStoreCounter
                {
                    instanceId = creatStoreCounter.itemInstanceId,
                    storeCounterData = storeData,
                     
                };
                runtimeStoreCounters.Add(creatStoreCounter.itemInstanceId,runtimeStoreCounter);
 
            }
        }
    }

    private async System.Threading.Tasks.Task DisplayStoreCounterAsync(DisplayStoreCounter displayStoreCounter)
    { 
        if (runtimeStoreCounters.TryGetValue(displayStoreCounter.itemInstanceId, out var runtimeStoreCounter))
        {
            if (displayStoreCounter.display)
            {
                if (displayStoreCounter.transform == null)
                {
                    return;
                }

                var storeCounterData = runtimeStoreCounter.storeCounterData;
                if (!nowRuntimeStoreCounterObjs.TryGetValue(displayStoreCounter.itemInstanceId, out var runtimeObj))
                {
                    runtimeObj =await GameRuntimeObjManager.instance.CreatRuntimeObj(RuntimeObjType.STOREITEM.ToString(), "STOREITEM",
                        sellItem, displayStoreCounter.itemInstanceId); 
                    nowRuntimeStoreCounterObjs.Add(displayStoreCounter.itemInstanceId, runtimeObj);
                }
                SellItem nowSellItem = runtimeObj.obj as SellItem;
                if (nowSellItem != null)
                {
                    int itemDataId = runtimeStoreCounter.itemData!=null? runtimeStoreCounter.itemData.id:0;
                    nowSellItem.SetDefaultOffset(storeCounterData.offset);
                    nowSellItem.InitReferenceData(new Item
                    {
                        dataId = itemDataId,
                        count = runtimeStoreCounter.count
                    });
                }

                nowSellItem.enabled = true;
                Transform transform = nowSellItem.transform;
                transform.gameObject.SetActive(true);
                transform.SetParent(displayStoreCounter.transform, false);
                // transform.localPosition = storeCounterData.offset;
                // 
            }
            else
            {
                if (nowRuntimeStoreCounterObjs.TryGetValue(displayStoreCounter.itemInstanceId, out var runtimeObj))
                {
                    GameRuntimeObjManager.instance.RecycleRuntimeObj(runtimeObj);
                    nowRuntimeStoreCounterObjs.Remove(displayStoreCounter.itemInstanceId);
                }
            }
 
        }
    }

    private void DeleteStoreCounter(DeleteMapItem deleteMapItem)
    {
        if (runtimeStoreCounters.Remove(deleteMapItem.mapItemInstanceId))
        {
            if (nowRuntimeStoreCounterObjs.TryGetValue(deleteMapItem.mapItemInstanceId, out var runtimeObj))
            {
                GameRuntimeObjManager.instance.RecycleRuntimeObj(runtimeObj);
                nowRuntimeStoreCounterObjs.Remove(deleteMapItem.mapItemInstanceId);
            } 
        }
    }

    public bool GetRuntimeStoreCounter(int instanceId, out RuntimeStoreCounter runtimeStoreCounter)
    {
        return runtimeStoreCounters.TryGetValue(instanceId, out runtimeStoreCounter);
    }
}

public class RuntimeStoreCounter  
{
    public int instanceId;
    public StoreCounterData storeCounterData;
    public ItemData itemData;
    public int count; 
}
