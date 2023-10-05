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
    public int priceValue;
    public bool open;
    public bool buyLimitOne;
    public int buyAction;
}
[CreateAssetMenu(menuName = "Data/商店数据")]
public class ShopDataList : ScriptableObject, IGameData,IDataArray<ShopGroup>
{
    public override string ToString()
    {
        return "ShopDataList";
    }
#if UNITY_EDITOR
    public EditorShopItemData[] shopItemDatas;
    public void SetReferenceData()
    {
        shopGroups.Clear();
        int groupIndex = -2;
        int shopIndex = -2;
        ShopGroup shopGroup=default(ShopGroup);
        ShopData shopData = default(ShopData);
        for (int i = 0; i < shopItemDatas.Length; i++)
        {
            EditorShopItemData shopItemData = shopItemDatas[i];
            int _groupIndex = shopGroups.FindIndex(s => s.name == shopItemData.shopGroup);
            if (_groupIndex != groupIndex)
            {
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
                        shopDatas = new List<ShopData>()
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
                    priceValue=shopItemData.priceValue,
                    open=shopItemData.open
                }
                );

            if (i == shopItemDatas.Length - 1)
            {
               // shopData.shopItem.Add(new int3(shopItemData.item, shopItemData.priceValue, shopItemData.open ? 1 : 0));
                shopGroup.shopDatas[shopIndex] = shopData;
                shopGroups[groupIndex] = shopGroup;
            }
        }
    }

    public string GetKey()
    {
       return "";
    }
#endif
    [SerializeField]
    private List<ShopGroup> shopGroups=new List<ShopGroup>();

    public List<ShopGroup> DataList => shopGroups;
}
[Serializable]
public struct ShopGroup : IReferenceData, IGameData
{ 
    public string name;
    public List<ShopData> shopDatas;

    public string GetKey()
    {
        return name;
    }

    public void SetReferenceData()
    { 
    } 
}
[Serializable]
public struct ShopData:IReferenceData
{
    public string shopName;
    public int shopId;
    public List<ShopItemData> shopItem;
}
[Serializable]
public struct ShopItemData: IReferenceData
{
    public int item;
    public PayType payType;
    public int priceValue;
    public bool open;
    public bool buyLimitOne;
    public int buyAction;
}
