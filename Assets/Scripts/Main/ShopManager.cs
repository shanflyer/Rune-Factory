using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
public class ShopManager : Singleton<ShopManager>
{
    Dictionary<string, ShopList> shopListDic = new Dictionary<string, ShopList>();
    Dictionary<int, Shop> shopDic = new Dictionary<int, Shop>();
    public override void Init()
    {
        base.Init();
        InitShop();
        GameActionManager.instance.AddListener<TryVisitShop>(TryVisitShop);
        GameActionManager.instance.AddListener<OpenShopItem>(OpenShopItem);
    }
    void OpenShopItem(OpenShopItem openShopItem) 
    { 
        if(shopDic.TryGetValue(openShopItem.shopId,out var shop))
        {
            shop.OpenShopItem(openShopItem.itemId);  
        }
    }
    public int2 GetShopMapItem(int shopId)
    {
        if(shopDic.TryGetValue(shopId,out var shop))
        {
            if(shopListDic.TryGetValue(shop.listName,out var shopList))
            {
                return new int2(shopList.mapInstance, shopList.mapItemInstance);
            }
        }
        return int2.zero;
    }
    public void InitShopList(ShopListSaveData shopListSaveData)
    {
        if(shopListDic.TryGetValue(shopListSaveData.name,out var shopList))
        {
            shopList.bindCharacters.Clear();
            shopList.bindCharacters.AddRange(shopListSaveData.binders);
        }
    }
    public void InitShop(ShopSaveData shopSaveData)
    {
        if (shopDic.TryGetValue(shopSaveData.shopId, out var shop))
        {
           for(int i = 0; i < shopSaveData.openItems.Count; i++)
            {
                shop.OpenShopItem(shopSaveData.openItems[i], false);
            }
        }
    }
    async void InitShop()
    {
        shopListDic.Clear();
        shopDic.Clear();
        var shopGroupDatas = await GameDataManager.instance.GetAllAsyncData<ShopGroup>();
        for(int i = 0; i < shopGroupDatas.Count; i++)
        {
            var shopGroupData = shopGroupDatas[i];
            ShopList shopList = new ShopList
            {
                groupName = shopGroupData.name,
                mapInstance=shopGroupData.mapInstance,
                mapItemInstance=shopGroupData.mapItem
            };
            shopList.bindCharacters.AddRange(shopGroupData.bindCharacters);

            for(int j = 0; j < shopGroupData.shopDatas.Count; j++)
            {
                Shop shop = new Shop(shopGroupData.shopDatas[j]); 
                shop.listName=shopGroupData.name;
                shopList.shops.Add(shop.shopId,shop);
                shopDic.Add(shop.shopId, shop);
            }
            shopListDic.Add(shopGroupData.name, shopList);
        }
    }
    async void TryVisitShop(TryVisitShop tryVisitShop)
    {
        string shopName = tryVisitShop.ShopName;
        if (string.IsNullOrEmpty(shopName))
        {
            if(NPCManager.instance.GetNPCFormInstance(tryVisitShop.CharacterId,out var NPC))
            {
                shopName = NPC.shopName;
            } 
        } 
        if(shopListDic.TryGetValue(shopName, out var shopList))
        {
          await UIManager.instance.ShowGamePanel<ShopPanel, ShopList>(shopList);
        } 
    }
}
public class ShopList : IReferenceData
{
    public string groupName;
    public int mapInstance;
    public int mapItemInstance;
    public MyDic<int,Shop> shops=new MyDic<int, Shop>();
    public List<int> bindCharacters = new List<int>();
   
}
public class Shop:IReferenceData
{ 
    public string shopName;
    public string listName;
    public int shopId;
    private MyDic<int,ShopItemData> shopItemDatas=new MyDic<int, ShopItemData>();
    private MyDic<int, ShopItemData> openShopItems = new MyDic<int, ShopItemData>();

    public bool OpenShopItem(int itemId,bool saveData=true)
    {
        if (shopItemDatas.TryGetValue(itemId,out var shopItemData))
        {
            openShopItems.Add(itemId, shopItemData);
            shopItemDatas.Remove(itemId);
            GameDataSaveManager.instance.UserGameSaveDataList.nowSaveData.SetShopSaveData(this);
            return true;
        }
        return false;
    }
    public Shop(ShopData shopData)
    {
        shopItemDatas.Clear();
        shopName = shopData.shopName;
        shopId = shopData.shopId;

        for(int i = 0; i < shopData.shopItem.Count; i++)
        {
           
            if (shopData.shopItem[i].open)
            {
                openShopItems.Add(shopData.shopItem[i].item, shopData.shopItem[i]);
            }
            else
            {
                shopItemDatas.Add(shopData.shopItem[i].item, shopData.shopItem[i]);
            }
        }
    }
    public List<ShopItemData> GetOpenShopItem()
    {
        return openShopItems.GetValueList();
    }
}