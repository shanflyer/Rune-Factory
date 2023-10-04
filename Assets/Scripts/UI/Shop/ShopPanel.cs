using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using OldName;
using Unity.Mathematics;

public class ShopPanel : GamePanel<ShopGroup>
{
    [SerializeField]
    TextMeshProUGUI Title;
    [SerializeField]
    Button CloseButton;
    [SerializeField]
    Transform ShopItemParent, ShopSelectParent;
    [SerializeField]
    ToggleGroup ItemGroup, ShopGroup;
    [SerializeField]
    Image selectItemIcon;
    [SerializeField]
    TextMeshProUGUI selectItemName;
    [SerializeField]
    Image selectMoneyIcon;
    [SerializeField]
    TextMeshProUGUI selectMoneyValue;
    [SerializeField]
    TextMeshProUGUI selectItemInfo;
    [SerializeField]
    TextMeshProUGUI selectItemProperty;
    [SerializeField]
    TMP_InputField buyCountValue;
    [SerializeField]
    Button addButton, reduceButton;
    [SerializeField]
    Button buyButton;
    [SerializeField]
    Transform SelectInformation;
    [SerializeField]
    ShopItemReference ShopItemReference;
    [SerializeField]
    ShopSelectReference ShopSelectReference;
    DisplayList<ShopSelectReference,ShopData> shops;
    DisplayList<ShopItemReference, ShopItemData> shopItems;

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        Title = FindChildGameObject<TextMeshProUGUI>("Title");
        CloseButton = FindChildGameObject<Button>("CloseButton");
        CloseButton.onClick.AddListener(Close);

        ShopSelectReference = FindChildGameObject<ShopSelectReference>("ShopSelect");
        ShopItemReference = FindChildGameObject<ShopItemReference>("ShopItem");
        ShopItemParent = FindChildGameObject("ShopItemList");
        ShopSelectParent = FindChildGameObject("ShopSelectList");
        ItemGroup = ShopItemParent.GetComponent<ToggleGroup>();
        ShopGroup = ShopItemParent.GetComponent<ToggleGroup>();

        shopItems = new DisplayList<ShopItemReference, ShopItemData>(ShopItemReference, ShopItemParent);
        shops = new DisplayList<ShopSelectReference, ShopData>(ShopSelectReference, ShopSelectParent);

        selectItemName = FindChildGameObject<TextMeshProUGUI>("SelectItemName");
        selectItemIcon = FindChildGameObject<Image>("SelectIcon");
        selectItemInfo = FindChildGameObject<TextMeshProUGUI>("Info");
        selectMoneyIcon = FindChildGameObject<Image>("SelectMoneyIcon");
        selectMoneyValue = FindChildGameObject<TextMeshProUGUI>("SelectMoneyValue");
        SelectInformation = FindChildGameObject("InformationObj");

        buyCountValue = FindChildGameObject<TMP_InputField>("BuyCountValue");
        addButton = FindChildGameObject<Button>("AddButton");
        reduceButton = FindChildGameObject<Button>("ReduceButton");
        buyCountValue.onValueChanged.AddListener((string value) =>
        {
            buyCount = int.Parse(value);
            RefreshBuyCount();
        });
        addButton.onClick.AddListener(() =>
        {
            buyCount++;
            RefreshBuyCount();
        });
        reduceButton.onClick.AddListener(() =>
        {
            buyCount--;
            RefreshBuyCount();
        });


        buyButton = FindChildGameObject<Button>("BuyButton");
        buyButton.onClick.AddListener(BuyAction);
    }

    ShopItemData selectShopItemData;
    int buyCount=1;

    protected override void Awake()
    {
        base.Awake();
        SelectInformation.transform.localScale = Vector3.zero; 
    }
    void RefreshBuyCount()
    {
        buyCount = math.clamp(buyCount, 1, 999);
        buyCountValue.SetTextWithoutNotify(buyCount.ToString());
    }
    async void BuyAction()
    {
        if(!await PackageManager.instance.CheckPackageTryItemIn(CharacterManager.instance.player.characterPackage, selectShopItemData.item, buyCount))
        {
            GameNotificationManager.instance.DisplayTips($"空间不足", "背包无法放下这么多东西");
            return;
        }

        ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(selectShopItemData.item);
        if (itemData != null)
        {
            int trueCost = (int)(itemData.shopPrice * selectShopItemData.priceValue * 0.01f)*buyCount;
            PayManager.instance.PayAction("购买", $"购买{buyCount}个+ {itemData.name} +", trueCost, selectShopItemData.payType, async () =>
            {
                await PackageManager.instance.SetItemInPackage(new Item 
                { 
                    dataId = selectShopItemData.item,
                    count=buyCount
                },CharacterManager.instance.player.characterPackage);

                InformationController.instance.AddInformation($"成功购买{buyCount}个+ {itemData.name} +");
                if (selectShopItemData.buyAction != 0)
                {
                    var GameActionData = await GameDataManager.instance.GetAsyncData<GameActionData>(selectShopItemData.buyAction);
                    GameActionData.Action();
                }
            }); 
        }
    }
    async void SeletShopItem(ShopItemData shopItemData)
    {
        selectShopItemData = shopItemData;
        ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(shopItemData.item);
        if (itemData == null)
        {
            selectItemName.text = itemData.name;
            selectItemInfo.text = itemData.text1;
            selectItemProperty.text = itemData.property.ToString();
            selectItemIcon.sprite = itemData.icon;
            selectItemIcon.SetNativeSize();
            selectMoneyValue.text = (itemData.shopPrice * shopItemData.priceValue / 100.0f).ToString("0");
            selectMoneyIcon.sprite = PayManager.instance.GetPayMoneySprite(shopItemData.payType);
        }
        buyCountValue.interactable= addButton.interactable = reduceButton.interactable = !shopItemData.buyLimitOne;
        buyCount = 1;
        RefreshBuyCount();
        SelectInformation.transform.localScale = Vector3.one;
    } 
    public override void InitReferenceData(ShopGroup v)
    {
        base.InitReferenceData(v);
        Title.text = v.name;
        shops.InitListData(v.shopDatas,
            (ShopData shopData) =>
            {
                List<ShopItemData> shopItemDatas = shopData.shopItem.FindAll(s=>s.open);  
                shopItems.InitListData(shopItemDatas, SeletShopItem,ItemGroup);
            },ShopGroup);
        buyCount = 1;
        RefreshBuyCount();
    }
}
