using System.Collections.Generic;
using UnityEngine;

public class PlayerStoreManager : Singleton<PlayerStoreManager>
{
    public Dictionary<int,RuntimeStoreCounter> RuntimeStoreCounters => runtimeStoreCounters;
    private Dictionary<int, RuntimeStoreCounter> runtimeStoreCounters;
    private Dictionary<int, RuntimeObj> nowRuntimeStoreCounterObjs = new Dictionary<int, RuntimeObj>();

    private SellItem sellItem;

    protected override void Clear()
    {
        base.Clear();
    }

    public override async void Init()
    {
        base.Init();
        var storeShow = StoreShow.instance;

        var playerStorePrefab = await GameSourceManager.instance.GetPrefab(DataPath.StoreCounterPrefab);
        sellItem = playerStorePrefab.GetComponent<SellItem>();
        runtimeStoreCounters=new Dictionary<int, RuntimeStoreCounter>();
        GameActionManager.instance.AddListener<CreatStoreCounter>(CreatStoreCounter);
        GameActionManager.instance.AddListener<DisplayStoreCounter>(DisplayStoreCounter);
        GameActionManager.instance.AddListener<DeleteMapItem>(DeleteStoreCounter);
        GameActionManager.instance.AddListener<StoreCounterSetSelectItemAction>(StoreCounterSetSelectItemAction);
        GameActionManager.instance.AddListener<SetStoreCounterItem>(SetStoreCounterItem);
        GameActionManager.instance.AddListener<SetStoreCounter>(SetStoreCounter);
        GameActionManager.instance.AddListener<BuyPlayerGood>(BuyPlayerGood);
        GameActionManager.instance.AddListener<TryBuyPlayerGood>(TryBuyPlayerGood);
    }

    private async void TryBuyPlayerGood(TryBuyPlayerGood buyPlayerGood)
    {
        if (runtimeStoreCounters.TryGetValue(buyPlayerGood.storeCounterId, out var runtimeStoreCounter))
        {
            if (runtimeStoreCounter.count <= 0)
            {
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
            if (runtimeObj.obj != null)
            {
                (runtimeObj.obj as SellItem).SetItemCount(runtimeStoreCounter.count);
            }

            GameDataSaveManager.instance.UserGameSaveData.SetStoreCounterSaveData(runtimeStoreCounter);
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
            GameDataSaveManager.instance.UserGameSaveData.SetStoreCounterSaveData(runtimeStoreCounter);
        }
    }

    private async void SetStoreCounter(SetStoreCounter setStoreCounter)
    {
        int characterId = setStoreCounter.playerId;
        if (runtimeStoreCounters.TryGetValue(setStoreCounter.storeCounterId, out var runtimeStoreCounter))
        {
            if (runtimeStoreCounter.count == 0)
            {
                var gameActionData = await GameDataManager.instance.GetAsyncData<GameActionData>(setStoreCounter.nullAction);
                gameActionData.Action(characterId, setStoreCounter.storeCounterId);
            }
            else
            {
                OpenStoreCounter(setStoreCounter.storeCounterId);
            }
            GameDataSaveManager.instance.UserGameSaveData.SetStoreCounterSaveData(runtimeStoreCounter);
        }
    }

    //设置背包界面物体Action
    private async void StoreCounterSetSelectItemAction(StoreCounterSetSelectItemAction storeCounterSetSelectItemAction)
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
            warehousePanel.SetSelectItemAction((Item item, bool select) =>
            {
                UIManager.instance.CloseGamePanel<WarehousePanel>();
                OpenSetItemPanel(storeCounterSetSelectItemAction.targetObj, item);
            }, "选择");
        }
    }

    private void OpenStoreCounter(int storeId)
    {
        if (runtimeStoreCounters.TryGetValue(storeId, out var runtimeStoreCounter))
        {
            SetStoreCounterItem setStoreCounterItem = new SetStoreCounterItem
            {
                storeCounterId = storeId,
                itemId = runtimeStoreCounter.itemData.id,
                count = runtimeStoreCounter.count
            };
            UIManager.instance.ShowGamePanel<StoreCounterSetPanel, SetStoreCounterItem>(setStoreCounterItem);
        }
    }

    private void OpenSetItemPanel(int storeId, Item item)
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
        UIManager.instance.ShowGamePanel<StoreCounterSetPanel, SetStoreCounterItem>(setStoreCounterItem);
    }

    private async void SetStoreCounterItem(SetStoreCounterItem setStoreCounterItem)
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

            GameDataSaveManager.instance.UserGameSaveData.SetStoreCounterSaveData(runtimeStoreCounter);
        }
    }

    private async void CreatStoreCounter(CreatStoreCounter creatStoreCounter)
    {
        if (!runtimeStoreCounters.ContainsKey(creatStoreCounter.itemInstanceId))
        {
            var storeData = await GameDataManager.instance.GetAsyncData<StoreCounterData>(creatStoreCounter.storeDataId);

            if (storeData.id == creatStoreCounter.storeDataId)
            {
                RuntimeStoreCounter runtimeStoreCounter = new RuntimeStoreCounter
                {
                    instanceId = creatStoreCounter.itemInstanceId,
                    storeCounterData = storeData,
                     
                };
                runtimeStoreCounters.Add(creatStoreCounter.itemInstanceId,runtimeStoreCounter);

                GameDataSaveManager.instance.UserGameSaveData.SetStoreCounterSaveData(runtimeStoreCounter);
            }
        }
    }

    private void DisplayStoreCounter(DisplayStoreCounter displayStoreCounter)
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
                    runtimeObj = GameRuntimeObjManager.instance.CreatRuntimeObj(RuntimeObjType.STOREITEM.ToString(), "STOREITEM",
                        sellItem, displayStoreCounter.itemInstanceId);
                    nowRuntimeStoreCounterObjs.Add(displayStoreCounter.itemInstanceId, runtimeObj);
                }
                SellItem nowSellItem = runtimeObj.obj as SellItem;
                if (nowSellItem != null)
                {
                    nowSellItem.InitReferenceData(new Item
                    {
                        dataId = runtimeStoreCounter.itemData.id,
                        count = runtimeStoreCounter.count
                    });
                }

                nowSellItem.enabled = true;
                Transform transform = nowSellItem.transform;
                transform.gameObject.SetActive(true);
                transform.SetParent(displayStoreCounter.transform, false);
                transform.localPosition = storeCounterData.offset;
            }
            else
            {
                if (nowRuntimeStoreCounterObjs.TryGetValue(displayStoreCounter.itemInstanceId, out var runtimeObj))
                {
                    GameRuntimeObjManager.instance.RecycleRuntimeObj(runtimeObj);
                    nowRuntimeStoreCounterObjs.Remove(displayStoreCounter.itemInstanceId);
                }
            }

            GameDataSaveManager.instance.UserGameSaveData.SetStoreCounterSaveData(runtimeStoreCounter);
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
            GameDataSaveManager.instance.UserGameSaveData.DeleteStoreCounter(deleteMapItem.mapItemInstanceId) ;
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