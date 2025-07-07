using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemReference : UIObjReference<ShopItemData>
{
    [SerializeField]
    private Image itemIcon;

    [SerializeField]
    private Toggle toggle;

    [SerializeField]
    private Image moneyIcon;

    [SerializeField]
    private TextMeshProUGUI moneyValue;

    private void Awake()
    {
        toggle.onValueChanged.AddListener((bool value) =>
        {
            if (value && SelectAction != null)
            {
                SelectAction(ShopItemData);
            }
        });
    }

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        itemIcon = FindChildGameObject<Image>("Icon");
        toggle = GetComponent<Toggle>();
        moneyIcon = FindChildGameObject<Image>("MoneyIcon");
        moneyValue = FindChildGameObject<TextMeshProUGUI>("MoneyValue");
    }

    private ShopItemData ShopItemData;

    public override async void InitData(ShopItemData t, SelectAction<ShopItemData> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        base.InitData(t, SelectAction, toggleGroup);
        ShopItemData = t;
        toggle.group = toggleGroup;
        this.SelectAction = SelectAction;
        ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(ShopItemData.item);
        itemIcon.sprite = itemData.icon;
        itemIcon.rectTransform.sizeDelta = GameCommon.SetImageSize(itemData.icon, new Vector2(32, 32));
        moneyValue.text = (itemData.shopPrice * ShopItemData.priceValue * 0.01f).ToString("0");
        moneyIcon.sprite = PayManager.instance.GetPayMoneySprite(ShopItemData.payType);
    }
}