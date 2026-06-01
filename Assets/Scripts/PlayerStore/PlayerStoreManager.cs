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
            return CalculateCustomerCd(CountStockedCounters());
        }
        return 0;
    }

    private int CountStockedCounters()
    {
        int storeCount = 0;
        using (var e = RuntimeStoreCounters.Values.GetEnumerator())
        {
            while (e.MoveNext())
            {
                if (e.Current.count > 0 && e.Current.itemData != null)
                {
                    storeCount++;
                }
            }
        }
        return storeCount;
    }

    private float CalculateCustomerCd(int storeCount)
    {
        float cdValue = storeCount / 5.0f;
        cdValue = math.clamp(cdValue, 1, 3);

        float nowCd = GameRandom.RandomFloat(GameCommon.autoCustomerCD);
        if (timeCurve != null)
        {
            nowCd += timeCurve.Evaluate(GameTimeManager.instance.timeValue);
        }
        if (weatherCurve != null)
        {
            nowCd += weatherCurve.Evaluate(EnvironmentManger.instance.nowWaterFall);
        }
        return nowCd / cdValue;
    }

    GameObjectCurveController.FrameTaskHandle nowAutoTask;
    void StartAutoCustomerTask()
    {
        float timeValue = 0;
        float nowCd = CalculateCustomerCd(CountStockedCounters());
        List<RuntimeStoreCounter> nowRuntimeStoreCounters = new List<RuntimeStoreCounter>();
        nowAutoTask = GameObjectCurveController.instance.StartFrameTask((float deltaTime) =>
        {
            timeValue += deltaTime;
            if (timeValue > nowCd)
            {
                timeValue = 0;
                nowRuntimeStoreCounters.Clear();
                using (var e= RuntimeStoreCounters.Values.GetEnumerator())
                {
                    while (e.MoveNext())
                    {
                        if (e.Current.count > 0 && e.Current.itemData != null)
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
                nowCd = CalculateCustomerCd(nowRuntimeStoreCounters.Count);
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
        bool result = SellStoreCounterItem(buyPlayerGood.storeCounterId, recycleWhenEmpty: false);
        if (buyPlayerGood.setResult != null)
        {
            buyPlayerGood.setResult(result);
        }
    }

    private void BuyPlayerGood(BuyPlayerGood buyPlayerGood)
    {
        bool result = SellStoreCounterItem(buyPlayerGood.storeCounterId, recycleWhenEmpty: true);
        if (buyPlayerGood.setResult != null)
        {
            buyPlayerGood.setResult(result);
        }
    }

    private bool SellStoreCounterItem(int storeCounterId, bool recycleWhenEmpty)
    {
        if (!runtimeStoreCounters.TryGetValue(storeCounterId, out var runtimeStoreCounter))
        {
            return false;
        }
        if (runtimeStoreCounter.count <= 0 || runtimeStoreCounter.itemData == null)
        {
            runtimeStoreCounter.count = math.max(runtimeStoreCounter.count, 0);
            return false;
        }

        RuntimeObj runtimeObj = null;
        SellItem sellItem = null;
        if (nowRuntimeStoreCounterObjs.TryGetValue(storeCounterId, out runtimeObj))
        {
            sellItem = runtimeObj.obj as SellItem;
            if (sellItem != null)
            {
                ShowCoin showCoin = new ShowCoin
                {
                    pos = sellItem.transform.position,
                };
                GameActionManager.instance.QueueAction(showCoin);
            }
        }

        PayManager.instance.AddGold(runtimeStoreCounter.itemData.sellPrice);
        runtimeStoreCounter.count--;

        if (runtimeStoreCounter.count <= 0)
        {
            runtimeStoreCounter.itemData = null;
            runtimeStoreCounter.count = 0;
            if (runtimeObj != null && runtimeObj.obj != null)
            {
                if (recycleWhenEmpty)
                {
                    GameRuntimeObjManager.instance.RecycleRuntimeObj(runtimeObj);
                    nowRuntimeStoreCounterObjs.Remove(storeCounterId);
                }
                else if (sellItem != null)
                {
                    sellItem.SetItemCount(0);
                }
            }
        }
        else if (sellItem != null)
        {
            sellItem.SetItemCount(runtimeStoreCounter.count);
        }

        return true;
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
            runtimeStoreCounter.count = math.max(setStoreCounterItem.count, 0);
            runtimeStoreCounter.itemData = null;
            if (runtimeStoreCounter.count > 0 && setStoreCounterItem.itemId > 0)
            {
                runtimeStoreCounter.itemData = await GameDataManager.instance.GetAsyncData<ItemData>(setStoreCounterItem.itemId);
                if (runtimeStoreCounter.itemData == null)
                {
                    runtimeStoreCounter.count = 0;
                }
            }

            if (nowRuntimeStoreCounterObjs.TryGetValue(setStoreCounterItem.storeCounterId, out var runtimeObj))
            {
                SellItem sellItem = runtimeObj.obj as SellItem;
                if (sellItem != null)
                {
                    sellItem.InitReferenceData(new Item
                    {
                        dataId = runtimeStoreCounter.itemData != null ? runtimeStoreCounter.itemData.id : 0,
                        count = runtimeStoreCounter.count
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
            int saveId = SaveRuntimeResolver.instance.EnsureSaveId(SaveEntityKind.StoreCounter, storeCounterSaveData.saveId);
            int instanceId = SaveRuntimeResolver.instance.Resolve(SaveEntityKind.StoreCounter, saveId);
            if (instanceId == 0)
            {
                instanceId = SaveRuntimeResolver.instance.Resolve(SaveEntityKind.HomeEquip, saveId);
            }
            if (instanceId == 0)
            {
                instanceId = MyInstance.instance.Uid;
            }
            SaveRuntimeResolver.instance.Bind(SaveEntityKind.StoreCounter, saveId, instanceId);
            RuntimeStoreCounter runtimeStoreCounter = new RuntimeStoreCounter
            {
                instanceId = instanceId,
                saveId = saveId,
                storeCounterData = storeData,

            };
            runtimeStoreCounters[instanceId] = runtimeStoreCounter;

            ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(storeCounterSaveData.itemDataId);
            runtimeStoreCounter.itemData = itemData;
            runtimeStoreCounter.count = storeCounterSaveData.count;
        }
    }

    private async System.Threading.Tasks.Task CreatStoreCounterAsync(CreatStoreCounter creatStoreCounter)
    {
        int instanceId = creatStoreCounter.itemInstanceId;
        int saveId = creatStoreCounter.saveId;
        if (saveId == 0 && instanceId != 0)
        {
            saveId = SaveRuntimeResolver.instance.GetSaveId(SaveEntityKind.StoreCounter, instanceId);
        }
        if (saveId == 0 && instanceId != 0)
        {
            saveId = SaveRuntimeResolver.instance.GetSaveId(SaveEntityKind.HomeEquip, instanceId);
        }
        saveId = SaveRuntimeResolver.instance.EnsureSaveId(SaveEntityKind.StoreCounter, saveId);
        if (instanceId == 0)
        {
            instanceId = SaveRuntimeResolver.instance.Resolve(SaveEntityKind.StoreCounter, saveId);
        }
        if (instanceId == 0)
        {
            instanceId = SaveRuntimeResolver.instance.Resolve(SaveEntityKind.HomeEquip, saveId);
        }
        if (instanceId == 0)
        {
            instanceId = MyInstance.instance.Uid;
        }
        SaveRuntimeResolver.instance.Bind(SaveEntityKind.StoreCounter, saveId, instanceId);

        if (!runtimeStoreCounters.ContainsKey(instanceId))
        {
            var storeData = await GameDataManager.instance.GetAsyncData<StoreCounterData>(creatStoreCounter.storeDataId);

            if (storeData!=null)
            {
                RuntimeStoreCounter runtimeStoreCounter = new RuntimeStoreCounter
                {
                    instanceId = instanceId,
                    saveId = saveId,
                    storeCounterData = storeData,

                };
                runtimeStoreCounters.Add(instanceId,runtimeStoreCounter);

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
    public int saveId;
    public StoreCounterData storeCounterData;
    public ItemData itemData;
    public int count;
}
