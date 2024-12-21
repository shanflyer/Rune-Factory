using System;
using System.Collections.Generic;
using System.Threading.Tasks;

[System.Serializable]
public struct Item : IReferenceData
{
    public int instanceId;
    public int packageId;
    public int dataId;
    public int count;
    public int value { get; private set; }
    public bool isFresh;
    public ItemType itemType;
    public bool locked;
    public bool singleItem;
    public Item(int dataId, int count,  int packageId = 0)
    {
        this.dataId = dataId;
        this.count = count;
        instanceId = 0;
        this.packageId = packageId;
        itemType = ItemType.Default;
        isFresh = false;
        locked = false;
        value = 1;
        maxValue = 1;
        singleItem = false;
        InitValue();
    }
    int maxValue;
    
    public void SetValue(int value)
    {
        this.value = value;
        if (this.value > maxValue)
        {
            this.value = maxValue;
        }
    }
    public void ChangeValue(int value)
    {
        this.value = this.value+value;
        if (this.value > maxValue)
        {
            this.value = maxValue;
        }
    }
    async void InitValue()
    {
        ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(dataId);
        if (itemData != null)
        {
            singleItem = itemData.groupCount <= 1;
            if(itemData.itemValue)
                maxValue = value = itemData.Property.Other;
        }
    }
    public float GetValue()
    {
        return value /(float)maxValue;
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
    public override void Init()
    {
        base.Init(); 
    }
    public Item CreatItem(ItemData data, int count)
    {
        Item item = new Item
        {
            dataId = data.id,
            count = count,
            instanceId = MyInstance.instance.uid
        };
        return item;
    }

    public Item CreatItem(int dataId, int count)
    {
        Item item = new Item
        {
            dataId = dataId,
            count = count,
            instanceId = MyInstance.instance.uid
        };
        return item;
    }

   

    public async Task BuyActionAsync(ShopItemData selectShopItemData, int buyCount)
    {
        if (!await PackageManager.instance.CheckPackageTryItemIn(CharacterManager.instance.controllerCharacter.characterPackage, selectShopItemData.item, buyCount))
        {
            GameNotificationManager.instance.DisplayTips($"空间不足", "背包无法放下这么多东西");
            return;
        }

        ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(selectShopItemData.item);
        if (itemData != null)
        {
            int trueCost = (int)(itemData.shopPrice * selectShopItemData.priceValue * 0.01f) * buyCount;
            PayManager.instance.PayAction("购买", $"购买{buyCount}个+ {itemData.itemName} +", trueCost, selectShopItemData.payType, async (bool result) =>
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

                InformationController.instance.AddInformation($"成功购买{buyCount}个+ {itemData.itemName} +");
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