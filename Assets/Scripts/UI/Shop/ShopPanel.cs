using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class ShopPanel : GamePanel<ShopList>
{
    //public override bool changeInputModel => false;
    [SerializeField]
    private TextMeshProUGUI Title;

    [SerializeField]
    private Button CloseButton;

    [SerializeField]
    private Transform ShopItemParent, ShopSelectParent;

    [SerializeField]
    private ToggleGroup ItemGroup, ShopGroup;

    [SerializeField]
    private Image selectItemIcon;

    [SerializeField]
    private TextMeshProUGUI selectItemName;

    [SerializeField]
    private Image selectMoneyIcon;

    [SerializeField]
    private TextMeshProUGUI selectMoneyValue;

    [SerializeField]
    private TextMeshProUGUI selectItemInfo;

    [SerializeField]
    private TextMeshProUGUI selectItemProperty;

    [SerializeField]
    private TMP_InputField buyCountValue;

    [SerializeField]
    private Button addButton, reduceButton;

    [SerializeField]
    private Button buyButton;

    [SerializeField]
    private Transform SelectInformation;

    [SerializeField]
    private ShopItemReference ShopItemReference;

    [SerializeField]
    private ShopSelectReference ShopSelectReference;

    private DisplayList<ShopSelectReference, Shop> shops;
    private DisplayList<ShopItemReference, ShopItemData> shopItems;

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        Title = FindChildGameObject<TextMeshProUGUI>("Title");
        CloseButton = FindChildGameObject<Button>("CloseButton");

        ShopSelectReference = FindChildGameObject<ShopSelectReference>("ShopSelect");
        ShopItemReference = FindChildGameObject<ShopItemReference>("ShopItem");
        ShopItemParent = FindChildGameObject("ShopItemList");
        ShopSelectParent = FindChildGameObject("ShopSelectList");
        ItemGroup = ShopItemParent.GetComponent<ToggleGroup>();
        ShopGroup = ShopSelectParent.GetComponent<ToggleGroup>();

        selectItemName = FindChildGameObject<TextMeshProUGUI>("SelectItemName");
        selectItemIcon = FindChildGameObject<Image>("SelectIcon");
        selectItemInfo = FindChildGameObject<TextMeshProUGUI>("Info");
        selectMoneyIcon = FindChildGameObject<Image>("SelectMoneyIcon");
        selectMoneyValue = FindChildGameObject<TextMeshProUGUI>("SelectMoneyValue");
        SelectInformation = FindChildGameObject("InformationObj");

        buyCountValue = FindChildGameObject<TMP_InputField>("BuyCountValue");
        addButton = FindChildGameObject<Button>("AddButton");
        reduceButton = FindChildGameObject<Button>("ReduceButton");
        buyButton = FindChildGameObject<Button>("BuyButton");

        selectItemProperty = FindChildGameObject<TextMeshProUGUI>("Property");
    }

    private ShopItemData selectShopItemData;
    private int buyCount = 1;

    public override void Close()
    {
        base.Close();
        ShopGroup.enabled = false;
        ItemGroup.enabled = false;
        shopItems.ClearSelect();
        shops.ClearSelect();
    }

    protected override void Awake()
    {
        base.Awake();
        SelectInformation.transform.localScale = Vector3.zero;

        CloseButton.onClick.AddListener(Close);
        shopItems = new DisplayList<ShopItemReference, ShopItemData>(ShopItemReference, ShopItemParent);
        shops = new DisplayList<ShopSelectReference, Shop>(ShopSelectReference, ShopSelectParent);

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
        buyButton.onClick.AddListener(BuyAction);
    }

    private void RefreshBuyCount()
    {
        buyCount = math.clamp(buyCount, 1, 999);
        buyCountValue.SetTextWithoutNotify(buyCount.ToString());
    }

    private async void BuyAction()
    {
        switch (selectShopItemData.type)
        {
            case ShopItemType.道具:
               await ItemManager.instance.BuyActionAsync(selectShopItemData, buyCount);
                break;

            case ShopItemType.动物:
                BuyAnimal();
                break;

            case ShopItemType.家具:
                HomeEquipManager.instance.BuyAction(selectShopItemData);
                break;
        }
    }

    private async void BuyAnimal()
    {
        if (TeamManager.instance.playerTeam.TeamCharacters.Count > 4)
        {
            TwoSelectData twoSelectData = new TwoSelectData
            {
                notice = "队伍人数超过4，不能购买动物"
            };

            UIManager.instance.ShowGamePanel<TwoSelectPanel, TwoSelectData>(twoSelectData);
            
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

                var controllerCharacter = CharacterManager.instance.controllerCharacter;
                TryCreatAnimal tryCreatAnimal = new TryCreatAnimal
                {
                    dataId = itemData.typeValue,
                    coordinate = controllerCharacter.coordinate,
                    roomId = controllerCharacter.mapInstance,
                    setValue = CreatAnimalEnd
                };
                GameActionManager.instance.QueueAction(tryCreatAnimal, true);

                void CreatAnimalEnd(int animalInstanceId)
                {
                    JoinTeam joinTeam = new JoinTeam
                    {
                        teamCharacterId = controllerCharacter.instanceId,
                        characterId = animalInstanceId
                    };
                    GameActionManager.instance.QueueAction(joinTeam);
                }

                InformationController.instance.AddInformation(string.Format(LanguageManage.SwitchStr("成功购买{0}个+ {1} +"),buyCount, LanguageManage.SwitchStr(itemData.itemName)));
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

    private async void SeletShopItem(ShopItemData shopItemData, bool selected = true)
    {
        selectShopItemData = shopItemData;
        ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(shopItemData.item);
        if (itemData != null)
        {
            selectItemName.SetADDText("+ ",itemData.itemName," +");
            selectItemInfo.SetSWText(itemData.GetInfo());
            selectItemProperty.SetSWText(itemData.GetProperty());
            selectItemIcon.sprite = itemData.icon;
            selectItemIcon.rectTransform.sizeDelta = GameCommon.SetImageSize(itemData.icon, new Vector2(32, 32));
            selectMoneyValue.SetSWText((itemData.shopPrice * shopItemData.priceValue * 0.01f).ToString("0"));
            selectMoneyIcon.sprite = PayManager.instance.GetPayMoneySprite(shopItemData.payType);
        }
        buyCountValue.interactable = addButton.interactable = reduceButton.interactable = !shopItemData.buyLimitOne;
        buyCount = 1;
        RefreshBuyCount();
        SelectInformation.transform.localScale = Vector3.one;
    }

    private void SelecShopData(Shop shop, bool selected)
    {
        if (selected)
        {
            SelectInformation.transform.localScale = Vector3.zero;
            List<ShopItemData> shopItemDatas = shop.GetOpenShopItem();
            shopItems.InitListData(shopItemDatas, SeletShopItem, ItemGroup);
        }
    }

    public override void InitReferenceData(ShopList v)
    {
        base.InitReferenceData(v);
        SelectInformation.transform.localScale = Vector3.zero;
        Title.SetSWText(v.groupName);
        ShopGroup.enabled = true;
        ItemGroup.enabled = true;
        shops.InitListData(v.shops.GetValueList(), SelecShopData, ShopGroup);
        shops.SelectDefault();
        SelecShopData(v.shops[0], true);
        buyCount = 1;
        RefreshBuyCount();
    }
}