using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Purchasing;

public class PlayerStoreManager : Singleton<PlayerStoreManager>
{
    public MyNativeData<RuntimeStoreCounter> RuntimeStoreCounters=>runtimeStoreCounters;
    private MyNativeData<RuntimeStoreCounter> runtimeStoreCounters;
    private Dictionary<int, RuntimeObj> nowRuntimeStoreCounterObjs = new Dictionary<int, RuntimeObj>();
    
    Dictionary<int, StoreCounterData> StoreCounterDataForMapItem = new Dictionary<int, StoreCounterData>();
    SellItem sellItem;
    protected override void Clear()
    {
        base.Clear();
    }
    public override async void Init()
    {
        base.Init(); 
        var storeShow= StoreShow.instance;

        var  playerStorePrefab = await GameSourceManager.instance.GetPrefab(DataPath.StoreCounterPrefab);
        sellItem= playerStorePrefab.GetComponent<SellItem>();
        runtimeStoreCounters.Init(32);
        StoreCounterDataForMapItem.Clear();
        var storeCounterDatas = await GameDataManager.instance.GetAllAsyncData<StoreCounterData>();
        for(int i = 0; i < storeCounterDatas.Count; i++)
        {
            var storeCounterData = storeCounterDatas[i];
            StoreCounterDataForMapItem[storeCounterData.linkItem] = storeCounterData;
        }

        GameActionManager.instance.AddListener<TryCreatStoreCounter>(TryCreatStoreCounter);
        GameActionManager.instance.AddListener<DisplayStoreCounter>(DisplayStoreCounter);
        GameActionManager.instance.AddListener<DeleteMapItem>(DeleteStoreCounter);
        GameActionManager.instance.AddListener<StoreCounterSetSelectItemAction>(StoreCounterSetSelectItemAction);
        GameActionManager.instance.AddListener<SetStoreCounterItem>(SetStoreCounterItem);
        GameActionManager.instance.AddListener<SetStoreCounter>(SetStoreCounter);
        GameActionManager.instance.AddListener<BuyPlayerGood>(BuyPlayerGood);
        GameActionManager.instance.AddListener<TryBuyPlayerGood>(TryBuyPlayerGood);
    }
    async void TryBuyPlayerGood(TryBuyPlayerGood buyPlayerGood)
    {
        if (runtimeStoreCounters.GetData(buyPlayerGood.storeCounterId, out var runtimeStoreCounter))
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
            ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(runtimeStoreCounter.itemId);
            PayManager.instance.AddGold(itemData.sellPrice);
            runtimeStoreCounter.count--;
            if (runtimeStoreCounter.count <= 0)
            {
                runtimeStoreCounter.itemId = 0;
                runtimeStoreCounter.count = 0; 
            }
            runtimeStoreCounters.SetData(runtimeStoreCounter);
            if (runtimeObj.obj != null)
            {
                (runtimeObj.obj as SellItem).SetItemCount(runtimeStoreCounter.count);
            }

            buyPlayerGood.setResult(true);
        }
    }
    async void BuyPlayerGood(BuyPlayerGood buyPlayerGood)
    {
        if (runtimeStoreCounters.GetData(buyPlayerGood.storeCounterId, out var runtimeStoreCounter))
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
                runtimeStoreCounters.SetData(runtimeStoreCounter);
                if(runtimeObj.obj != null)
                {
                    (runtimeObj.obj as SellItem).AddItemCount(-1);
                }

                ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(runtimeStoreCounter.itemId);
                PayManager.instance.AddGold(itemData.sellPrice);
            }
            else
            { 
                runtimeStoreCounter.itemId = 0;
                runtimeStoreCounter.count = 0;
                runtimeStoreCounters.SetData(runtimeStoreCounter);
                if (runtimeObj.obj!=null)
                {
                    GameRuntimeObjManager.instance.RecycleRuntimeObj(runtimeObj);
                }
            }

           
        }
    }
    async void SetStoreCounter(SetStoreCounter setStoreCounter)
    {
        int characterId = setStoreCounter.playerId;
        if(runtimeStoreCounters.GetData(setStoreCounter.storeCounterId,out var runtimeStoreCounter))
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
        }
    }

     //设置背包界面物体Action
    async void StoreCounterSetSelectItemAction(StoreCounterSetSelectItemAction storeCounterSetSelectItemAction)
    {
        if(runtimeStoreCounters.Contains(storeCounterSetSelectItemAction.targetObj))
        {
            WarehousePanel warehousePanel = await UIManager.instance.GetGamePanel<WarehousePanel>();
            warehousePanel.SetSelectItemAction((Item item, int packageId) =>
            {
                UIManager.instance.CloseGamePanel<WarehousePanel>();
                OpenSetItemPanel(storeCounterSetSelectItemAction.targetObj, item,packageId);
            }, "选择");
        }
    }
    void OpenStoreCounter(int storeId)
    {
        if (runtimeStoreCounters.GetData(storeId, out var runtimeStoreCounter))
        {
            SetStoreCounterItem setStoreCounterItem = new SetStoreCounterItem
            {
                storeCounterId = storeId,
                itemId = runtimeStoreCounter.itemId,
                count=runtimeStoreCounter.count
            };
            UIManager.instance.ShowGamePanel<StoreCounterSetPanel, SetStoreCounterItem>(setStoreCounterItem);
        }
    }
    void OpenSetItemPanel(int storeId,Item item,int packageId)
    {
        SetStoreCounterItem setStoreCounterItem = new SetStoreCounterItem
        {
            storeCounterId = storeId,
            itemId = item.dataId, 
        };
        if(runtimeStoreCounters.GetData(storeId,out var runtimeStoreCounter))
        {
            setStoreCounterItem.count = runtimeStoreCounter.count;
        }
        UIManager.instance.ShowGamePanel<StoreCounterSetPanel, SetStoreCounterItem>(setStoreCounterItem);
    }

    void SetStoreCounterItem(SetStoreCounterItem setStoreCounterItem)
    {
        if(runtimeStoreCounters.GetData(setStoreCounterItem.storeCounterId,out var runtimeStoreCounter))
        {
            runtimeStoreCounter.itemId = setStoreCounterItem.itemId;
            runtimeStoreCounter.count = setStoreCounterItem.count;
            if (runtimeStoreCounter.count == 0)
            {
                runtimeStoreCounter.itemId = 0;
            }
            runtimeStoreCounters.SetData(runtimeStoreCounter);

            if(nowRuntimeStoreCounterObjs.TryGetValue(setStoreCounterItem.storeCounterId,out var runtimeObj))
            {
                SellItem sellItem = runtimeObj.obj as SellItem;
                if (sellItem != null)
                {
                    sellItem.InitReferenceData(new Item
                    {
                        dataId = setStoreCounterItem.itemId,
                        count=setStoreCounterItem.count
                    });
                }
            }
        }
    }
    void TryCreatStoreCounter(TryCreatStoreCounter tryCreatStoreCounter)
    { 
        if (!runtimeStoreCounters.Contains(tryCreatStoreCounter.itemInstanceId) &&
            StoreCounterDataForMapItem.TryGetValue(tryCreatStoreCounter.itemDataId,out var storeCounterData))
        {
            RuntimeStoreCounter runtimeStoreCounter = new RuntimeStoreCounter
            {
                instanceId = tryCreatStoreCounter.itemInstanceId,
                dataId = tryCreatStoreCounter.itemDataId,
            };
            runtimeStoreCounters.AddData(runtimeStoreCounter);
        }
    }
    void DisplayStoreCounter(DisplayStoreCounter displayStoreCounter)
    {
        if (runtimeStoreCounters.GetData(displayStoreCounter.itemInstanceId,out var runtimeStoreCounter))
        {
            if (displayStoreCounter.display)
            {
                if (displayStoreCounter.transform == null)
                {
                    return;
                }
                var storeCounterData = StoreCounterDataForMapItem[runtimeStoreCounter.dataId]; 
                if (!nowRuntimeStoreCounterObjs.TryGetValue(displayStoreCounter.itemInstanceId,out var runtimeObj))
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
                        dataId = runtimeStoreCounter.itemId,
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
        }
    }
    void DeleteStoreCounter(DeleteMapItem deleteMapItem)
    {
        if (runtimeStoreCounters.RemoveData(deleteMapItem.mapItemInstanceId))
        {
            if(nowRuntimeStoreCounterObjs.TryGetValue(deleteMapItem.mapItemInstanceId,out var runtimeObj))
            {
                GameRuntimeObjManager.instance.RecycleRuntimeObj(runtimeObj);
                nowRuntimeStoreCounterObjs.Remove(deleteMapItem.mapItemInstanceId);
            }
        }
    }

    public bool GetRuntimeStoreCounter(int instanceId,out RuntimeStoreCounter runtimeStoreCounter)
    {
        return runtimeStoreCounters.GetData(instanceId, out runtimeStoreCounter);
    }
    
} 

public struct RuntimeStoreCounter
{
    public override int GetHashCode()
    {
        return instanceId;
    } 
    public int instanceId; 
    public int dataId;
    public int itemId;
    public int count;
}
 