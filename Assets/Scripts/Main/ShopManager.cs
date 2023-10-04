using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : Singleton<ShopManager>
{
    Dictionary<string, ShopGroup> initShopGroups = new Dictionary<string, ShopGroup>();
    Dictionary<string, List<int>> ShopNpcs = new Dictionary<string, List<int>>();
    public override async void Init()
    {
        base.Init();
        initShopGroups.Clear();
        var shopDataList = await GameDataManager.instance.GetAsyncData<ShopDataList>();
        for(int i = 0; i < shopDataList.shopGroups.Count; i++)
        {
            initShopGroups[shopDataList.shopGroups[i].name] = shopDataList.shopGroups[i];
        }
    }

}