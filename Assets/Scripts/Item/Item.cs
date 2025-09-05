using System;
using System.Collections.Generic;
using System.Threading.Tasks;

[Serializable]
public struct Item : IReferenceData
{
    public int instanceId;
    public int packageId;
    public int dataId;
    public int count;
    public int value;
    public bool isFresh;
    public ItemType itemType;
    public bool locked; 
    public Item(int dataId, int count,  int packageId = 0)
    {
        this.dataId = dataId;
        this.count = count;
        instanceId = 0;
        this.packageId = packageId;
        itemType = ItemType.Default;
        isFresh = false;
        locked = false;
        value = 100;  
    } 
    
   public async Task<bool> IsSingleItem()
    {
        ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(dataId);
        return itemData.groupCount == 1;
    }
    public async Task<float> GetValue()
    {
        ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(dataId);
        int maxValue = itemData.Property.Other;
        return value /(float)maxValue;
    }
    public static async Task<Item> SetValue(Item item,int value)
    {
        ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(item.dataId);
        item.value = value;
        if (item.value > itemData.Property.Other)
        {
            item.value = itemData.Property.Other;
        }
        return item;
    }
    public static async Task<Item> ChangeValue(Item item, int value)
    {
        ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(item.dataId);
        item.value += value;
        if (item.value > itemData.Property.Other)
        {
            item.value = itemData.Property.Other;
        }
        if (item.value < 0)
        {
            item.value = 0;
        }
        return item;
    }
}

public struct Equipment : IReferenceData
{
    public int characterId;
    public int dataId;
    public float itemValue;
    public ItemType ItemType;
    public bool hide;
}

public class ItemManager:Singleton<ItemManager>
{
    MyDic<int, ItemData> allItemDatas = new MyDic<int, ItemData>();
    public override async void Init()
    {
        base.Init(); 
        allItemDatas.Clear();
        var data = await GameDataManager.instance.GetAllAsyncData<ItemData>();
        for(int i = 0; i < data.Count; i++)
        {
            allItemDatas.Add(data[i].id, data[i]);
        }
    }
    public HashSet<int> GetItemsForTag(int tag)
    {
        HashSet<int> results = new HashSet<int>();
        for(int i = 0; i < allItemDatas.length; i++)
        {
            if (allItemDatas[i].tag.Contains(tag))
            {
                results.Add(allItemDatas[i].id);
            }
        }
        return results;
    }
    public Item CreatItem(ItemData data, int count)
    {
        Item item = new Item
        {
            dataId = data.id,
            count = count,
            instanceId = MyInstance.instance.Uid
        };
        return item;
    }

    public Item CreatItem(int dataId, int count)
    {
        Item item = new Item
        {
            dataId = dataId,
            count = count,
            instanceId = MyInstance.instance.Uid
        };
        return item;
    }

   

    public async Task BuyActionAsync(ShopItemData selectShopItemData, int buyCount)
    {
        if (!PackageManager.instance.CheckPackageTryItemIn(
                CharacterManager.instance.controllerCharacter.characterPackage, selectShopItemData.item, buyCount))
        {
            GameNotificationManager.instance.DisplayTips($"空间不足", "背包无法放下这么多东西");
            return;
        }

        ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(selectShopItemData.item);
        if (itemData != null)
        {
            int trueCost = (int)(itemData.shopPrice * selectShopItemData.priceValue * 0.01f) * buyCount;
            PayManager.instance.PayAction("购买", $"{string.Format(LanguageManage.SwitchStr("购买{0}个"), buyCount)}+ {LanguageManage.SwitchStr(itemData.itemName)} +", trueCost, selectShopItemData.payType, async (bool result) =>
            {
                if (!result)
                {
                    return;
                }
                await PackageManager.instance.SetItemInPackage(new Item
                {
                    dataId = selectShopItemData.item,
                    count = buyCount
                }, CharacterManager.instance.controllerCharacter.characterPackage);

                InformationController.instance.AddInformation($"{LanguageManage.SwitchStr("成功购买")}{buyCount}{LanguageManage.SwitchStr("个+")} {LanguageManage.SwitchStr(itemData.name)} +");
                if (selectShopItemData.buyAction != 0)
                {
                    var GameActionData = await GameDataManager.instance.GetAsyncData<GameActionData>(selectShopItemData.buyAction);
                    GameActionData.Action();
                }
                ShopBuySuccess shopBuySuccess = new ShopBuySuccess
                {
                    buyCount = buyCount
                };
                GameActionManager.instance.QueueAction(shopBuySuccess);
            });
        }
    }
}