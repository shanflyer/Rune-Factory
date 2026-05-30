using System;
using System.Collections.Generic;
using System.Threading.Tasks;

[Serializable]
public struct Item : IReferenceData
{
    [NonSerialized] public int instanceId; // ≤999999
    [NonSerialized] public int packageId; // ≤999999
    [NonSerialized] public int saveId;
    [NonSerialized] public int packageSaveId;
    [NonSerialized] public int dataId; // ≤9999
    [NonSerialized] public int count; // ≤100
    [NonSerialized] public int value; // ≤100
    [NonSerialized] public bool isFresh;
    [NonSerialized] public ItemType itemType; // ≤20
    [NonSerialized] public bool locked;


    public (ulong, ulong) Pack()
    {
        int packedSaveId = saveId != 0 ? saveId : instanceId;
        int packedPackageSaveId = packageSaveId != 0 ? packageSaveId : packageId;
        ValidatePackRange(packedSaveId, packedPackageSaveId);

        ulong d1, d2;
        d1 = d2 = 0;

        // d1
        d1 |= (ulong)(packedSaveId & 0xFFFFF) << 0; // 20
        d1 |= (ulong)(packedPackageSaveId & 0xFFFFF) << 20; // 20
        d1 |= (ulong)(dataId & 0x3FFF) << 40; // 14
        d1 |= (ulong)(count & 0x7F) << 54; // 7

        // d2
        d2 |= (ulong)(value & 0x7F) << 0; // 7
        d2 |= (isFresh ? 1UL : 0UL) << 7; // 1
        d2 |= (ulong)((int)itemType & 0x1F) << 8; // 5
        d2 |= (locked ? 1UL : 0UL) << 13; // 1

        return (d1, d2);
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    private void ValidatePackRange(int packedSaveId, int packedPackageSaveId)
    {
        ValidateUnsignedPackField(nameof(saveId), packedSaveId, 0xFFFFF);
        ValidateUnsignedPackField(nameof(packageSaveId), packedPackageSaveId, 0xFFFFF);
        ValidateUnsignedPackField(nameof(dataId), dataId, 0x3FFF);
        ValidateUnsignedPackField(nameof(count), count, 0x7F);
        ValidateUnsignedPackField(nameof(value), value, 0x7F);

        int itemTypeValue = (int)itemType;
        if (itemTypeValue < -1 || itemTypeValue > 0x1F)
        {
            UnityEngine.Debug.LogError($"Item pack field {nameof(itemType)} is out of range: {itemTypeValue}. Allowed range is -1..31.");
        }
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    private static void ValidateUnsignedPackField(string fieldName, int fieldValue, int maxValue)
    {
        if (fieldValue < 0 || fieldValue > maxValue)
        {
            UnityEngine.Debug.LogError($"Item pack field {fieldName} is out of range: {fieldValue}. Allowed range is 0..{maxValue}.");
        }
    }


    public Item(ulong d1, ulong d2)
    {
        saveId = (int)((d1 >> 0) & 0xFFFFF);
        packageSaveId = (int)((d1 >> 20) & 0xFFFFF);
        instanceId = 0;
        packageId = 0;
        dataId = (int)((d1 >> 40) & 0x3FFF);
        count = (int)((d1 >> 54) & 0x7F);

        // d2
        value = (int)((d2 >> 0) & 0x7F);
        isFresh = ((d2 >> 7) & 0x1) != 0;
        itemType = (ItemType)((d2 >> 8) & 0x1F);
        locked = ((d2 >> 13) & 0x1) != 0;
    }
    public Item(int dataId, int count,  int packageId = 0)
    {
        this.dataId = dataId;
        this.count = count;
        instanceId = 0;
        this.packageId = packageId;
        saveId = 0;
        packageSaveId = 0;
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
    private Task initializationTask = Task.CompletedTask;
    public override Task InitializationTask => initializationTask;

    public override void Init()
    {
        base.Init();
        initializationTask = InitAsync();
    }

    private async Task InitAsync()
    {
        allItemDatas.Clear();
        var data = await GameDataManager.instance.GetAllAsyncData<ItemData>();
        for(int i = 0; i < data.Count; i++)
        {
            allItemDatas.Add(data[i].id, data[i]);
        }
    }

    protected override void Clear()
    {
        initializationTask = Task.CompletedTask;
        allItemDatas.Clear();
        base.Clear();
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

                InformationController.instance.AddInformation(
                    $"{LanguageManage.SwitchStr("成功购买")}{buyCount}{LanguageManage.SwitchStr("个+")} {LanguageManage.SwitchStr(itemData.itemName)} +");
                if (selectShopItemData.buyAction != 0)
                {
                    var gameActionAsset = await GameDataManager.instance.GetAsyncData<GameActionAsset>(selectShopItemData.buyAction);
                    gameActionAsset.Action();
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
