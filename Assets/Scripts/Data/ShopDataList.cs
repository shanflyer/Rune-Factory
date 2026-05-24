using System.Collections.Generic;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using System;


public struct EditorShopItemData
{
    public int shopId;
    public string shopName;
    public string shopGroup;
    public int item;
    public int openFriendLevel;
    public ShopItemType shopItemType;
    public List<int> bindCharacters;
    public PayType payType;
    public int priceValue;
    public bool buyLimitOne;
    public int buyAction;
    public int mapInstance;
    public int mapItem;
}
[CreateAssetMenu(menuName = "Data/商店数据")]
public class ShopDataList : ScriptableObject, IGameData,IDataArray<ShopGroup>
{
    public override string ToString()
    {
        return "ShopDataList";
    }
    public string GetKey()
    {
        return "";
    }
#if UNITY_EDITOR
    public EditorShopItemData[] shopItemDatas;
    public void SetReferenceData()
    {
        List<ShopGroup> shopGroups = new List<ShopGroup>();
        int groupIndex = -2;
        int shopIndex = -2;
        ShopGroup shopGroup=new ShopGroup();
        ShopData shopData = new ShopData();
        for (int i = 0; i < shopItemDatas.Length; i++)
        {
            EditorShopItemData shopItemData = shopItemDatas[i];
            int _groupIndex = shopGroups.FindIndex(s => s.name == shopItemData.shopGroup);
            if (_groupIndex != groupIndex)
            {
                shopIndex = -2;
                if (groupIndex >= 0)
                {
                    shopGroups[groupIndex] = shopGroup;
                }
                groupIndex = _groupIndex;
                if (groupIndex >= 0)
                {
                    shopGroup = shopGroups[groupIndex];
                }
                else
                {
                    shopGroup = new ShopGroup
                    {
                        name = shopItemData.shopGroup,
                        shopDatas = new List<ShopData>(),
                        mapInstance=shopItemData.mapInstance,
                        mapItem=shopItemData.mapItem,
                        bindCharacters=shopItemData.bindCharacters,
                    };
                    shopGroups.Add(shopGroup);
                    groupIndex = shopGroups.Count - 1;
                }
            }

            int _shopIndex = shopGroup.shopDatas.FindIndex(s => s.shopId == shopItemData.shopId);
            if (shopIndex != _shopIndex)
            {
                if (shopIndex >= 0)
                {
                    shopGroup.shopDatas[shopIndex] = shopData;
                }
                shopIndex = _shopIndex;
                if (shopIndex >= 0)
                {
                    shopData = shopGroup.shopDatas[shopIndex];
                }
                else
                {
                    shopData = new ShopData
                    {
                        shopName = shopItemData.shopName,
                        shopId = shopItemData.shopId,
                        shopItem = new List<ShopItemData>()
                    };
                    shopGroup.shopDatas.Add(shopData);
                    shopIndex = shopGroup.shopDatas.Count - 1;
                }

            }
            shopData.shopItem.Add(
                new ShopItemData
                {
                    item=shopItemData.item,
                    type=shopItemData.shopItemType,
                    payType=shopItemData.payType,
                    priceValue=shopItemData.priceValue,
                    openFriendLevel=shopItemData.openFriendLevel,
                    buyLimitOne=shopItemData.buyLimitOne
                }
                );

            if (i == shopItemDatas.Length - 1)
            {
               // shopData.shopItem.Add(new int3(shopItemData.item, shopItemData.priceValue, shopItemData.open ? 1 : 0));
                shopGroup.shopDatas[shopIndex] = shopData;
                shopGroups[groupIndex] = shopGroup;
            }
        }

        this.shopGroups = shopGroups.ToArray();
    }


#endif
    [SerializeField]
    private ShopGroup[] shopGroups;

    public ShopGroup[] DataList => shopGroups;

}
[Serializable]
public class ShopGroup : IReferenceData, IGameData
{
    public string name;
    public int mapInstance;
    public int mapItem;
    public List<int> bindCharacters;
    public List<ShopData> shopDatas=new List<ShopData>();

    public string GetKey()
    {
        return name;
    }

    public void SetReferenceData()
    {
    }
}
[Serializable]
public class ShopData :IReferenceData
{
    public string shopName;

    public int shopId;
    public List<ShopItemData> shopItem=new List<ShopItemData>();
}
[Serializable]
public class ShopItemData : IReferenceData
{
    public int item;
    public ShopItemType type;
    public PayType payType;
    public int priceValue;
    public int openFriendLevel;
    public bool buyLimitOne;
    public int buyAction;
}
