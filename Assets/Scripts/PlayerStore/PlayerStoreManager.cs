using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class PlayerStoreManager : Singleton<PlayerStoreManager>
{
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
    }
     //设置背包界面物体Action
    async void StoreCounterSetSelectItemAction(StoreCounterSetSelectItemAction storeCounterSetSelectItemAction)
    {
        if(runtimeStoreCounters.Contains(storeCounterSetSelectItemAction.sourceObj))
        {
            WarehousePanel warehousePanel = await UIManager.instance.GetGamePanel<WarehousePanel>();
            warehousePanel.SetSelectItemAction((Item item, int packageId) =>
            {
                OpenSetItemPanel(storeCounterSetSelectItemAction.sourceObj,item,packageId);
            }, "选择");
        }
    }

    void OpenSetItemPanel(int storeId,Item item,int packageId)
    {
        SetStoreCounterItem setStoreCounterItem = new SetStoreCounterItem
        {
            storeCounterId = storeId,
            itemId = item.dataId,
            count = item.count
        };
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
                if (!nowRuntimeStoreCounterObjs.ContainsKey(displayStoreCounter.itemInstanceId))
                {
                    RuntimeObj runtimeObj = GameRuntimeObjManager.instance.CreatRuntimeObj(RuntimeObjType.STOREITEM.ToString(), "STOREITEM",
                        sellItem, displayStoreCounter.itemInstanceId);

                    var storeCounterData = StoreCounterDataForMapItem[runtimeStoreCounter.dataId];
                    SellItem nowSellItem = runtimeObj.obj as SellItem;

                    Transform transform = nowSellItem.transform;
                    transform.SetParent(displayStoreCounter.transform, false);
                    transform.localPosition = storeCounterData.offset;
                    nowRuntimeStoreCounterObjs.Add(displayStoreCounter.itemInstanceId, runtimeObj);
                }
            }
            else
            {
                if (nowRuntimeStoreCounterObjs.TryGetValue(displayStoreCounter.itemInstanceId, out var runtimeObj))
                {
                    GameRuntimeObjManager.instance.RecycleRuntimeObj(runtimeObj);
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
 