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
        GameActionManager.instance.AddListener<RefreshShopLevel>(RefreshShopLevel);
    }
    void RefreshShopLevel(RefreshShopLevel refreshShopLevel)
    {
        if (shopListDic.TryGetValue(refreshShopLevel.shopName, out var shopList))
        {
           for(int i = 0; i < shopList.shops.length; i++)
            {
                var shop = shopList.shops[i];
                shop.RefreshOpenItem(true);
            }
        }
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
                Shop shop = new Shop(shopGroupData.shopDatas[j],shopGroupData.bindCharacters); 
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
            else if (NPCManager.instance.GetNPC(tryVisitShop.CharacterId, out NPC))
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
    public string shopName=>shopData.shopName;
    public string listName;
    public int shopId=> shopData.shopId;
    private MyDic<int,ShopItemData> shopItemDatas=new MyDic<int, ShopItemData>();
    private MyDic<int, ShopItemData> openShopItems = new MyDic<int, ShopItemData>();
    private List<int> bindCharacters = new List<int>();

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
    int friendLevel = 0;
    public async void RefreshOpenItem(bool show = false)
    {
        if (this.bindCharacters != null)
        {
            for (int i = 0; i < this.bindCharacters.Count; i++)
            {
                int friendLevel = FriendManager.instance.GetFriendShipLevel(this.bindCharacters[i]);
                if (friendLevel > this.friendLevel)
                {
                    this.friendLevel = friendLevel;
                }
            }
        }
        for (int i = shopItemDatas.length-1; i >=0; i--)
        {  
            if (shopItemDatas[i].openFriendLevel <= friendLevel)
            {
                int item = shopItemDatas[i].item;
                openShopItems.Add(item, shopItemDatas[i]);
                shopItemDatas.Remove(item);

                if (show)
                {
                    ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(item);
                    InformationController.instance.AddInformation($"{itemData.itemName}已经开始售卖!");
                } 
            } 
        }
    }
    ShopData shopData;
    public Shop(ShopData shopData, List<int> bindCharacters)
    {
        this.shopData=shopData;
        shopItemDatas.Clear();
        this.bindCharacters = bindCharacters;
        for (int i = 0; i < shopData.shopItem.Count; i++)
        {

            if (shopData.shopItem[i].openFriendLevel <= friendLevel)
            {
                openShopItems.Add(shopData.shopItem[i].item, shopData.shopItem[i]);
            }
            else
            {
                shopItemDatas.Add(shopData.shopItem[i].item, shopData.shopItem[i]);
            }
        }

        RefreshOpenItem(false); 
    }
    public List<ShopItemData> GetOpenShopItem()
    {
        return openShopItems.GetValueList();
    }
}