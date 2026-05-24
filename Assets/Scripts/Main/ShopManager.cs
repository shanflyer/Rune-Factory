using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Mathematics;

public class ShopManager : Singleton<ShopManager>
{
    Dictionary<int2, ShopList> _shopListDic = new Dictionary<int2, ShopList>();
    Dictionary<string, ShopList> shopListDic = new Dictionary<string, ShopList>();
    Dictionary<int, Shop> shopDic = new Dictionary<int, Shop>();
    private Task initializationTask = Task.CompletedTask;
    public override Task InitializationTask => initializationTask;
    public override System.Collections.Generic.IReadOnlyList<System.Type> InitializationDependencies => new[] { typeof(GameDataManager), typeof(GameActionManager) };

    public override void Init()
    {
        base.Init();
        // 商店数据属于启动链路，必须暴露 InitializationTask 让自动初始化等待完成。
        initializationTask = InitShopAsync();
        GameActionManager.instance.AddAsyncListener<TryVisitShop>(TryVisitShopAsync, nameof(TryVisitShop));
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

    async Task InitShopAsync()
    {
        shopListDic.Clear();
        _shopListDic.Clear();
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
            _shopListDic.Add(new int2(shopGroupData.mapInstance, shopGroupData.mapItem), shopList);
            shopListDic.Add(shopGroupData.name, shopList);
        }
    }

    protected override void Clear()
    {
        initializationTask = Task.CompletedTask;
        shopListDic.Clear();
        _shopListDic.Clear();
        shopDic.Clear();
        base.Clear();
    }

    async System.Threading.Tasks.Task TryVisitShopAsync(TryVisitShop tryVisitShop)
    {
        string shopName = tryVisitShop.ShopName;
        if (string.IsNullOrEmpty(shopName))
        {
            if (tryVisitShop.CharacterId == 0)
            {
                if(WorldMapManager.instance.GetRuntimeMapItem(tryVisitShop.ShopObjId, out var runtimeMapItem))
                {
                    if(_shopListDic.TryGetValue(runtimeMapItem.editorKey,out var shopList1))
                    {
                        await UIManager.instance.ShowGamePanel<ShopPanel, ShopList>(shopList1);
                    }
                }

            }
            else
            {
                if (NPCManager.instance.GetNPCFormInstance(tryVisitShop.CharacterId, out var NPC))
                {
                    shopName = NPC.shopName;
                }
                else if (NPCManager.instance.GetNPC(tryVisitShop.CharacterId, out NPC))
                {
                    shopName = NPC.shopName;
                }
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


    int friendLevel = 1;
    public void RefreshOpenItem(bool show = false)
    {
        AsyncTaskRunner.Run(() => RefreshOpenItemAsync(show), nameof(RefreshOpenItem));
    }

    public async System.Threading.Tasks.Task RefreshOpenItemAsync(bool show = false)
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
                    if (itemData.type == ItemType.种子 || itemData.type == ItemType.农作物)
                    {
                        GameDataSaveManager.instance.SetPlantFruitCount(itemData.typeValue, 0);
                    }
                    ItemResultInfo itemResultInfo = new ItemResultInfo
                    {
                        icon = itemData.icon,
                        info0 = itemData.itemName,
                        info1 = $"{LanguageManage.SwitchStr(shopData.shopName)}{LanguageManage.SwitchStr("已经开始售卖!")}"
                    };
                    GameNotificationManager.instance.ShowItemResultInfo(itemResultInfo);
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
