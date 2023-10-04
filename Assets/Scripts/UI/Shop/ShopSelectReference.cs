using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopSelectReference : UIObjReference<ShopData>
{
    [SerializeField]
    TextMeshProUGUI shopName0,shopName1;
    [SerializeField]
    Toggle toggle;
    private void Awake()
    {
        toggle.onValueChanged.AddListener((bool value) =>
        {
            shopName0.enabled = !value;
            shopName1.enabled = value;
            if (value&& SelectAction!=null)
            {
                SelectAction(shopData);
            }
        });
    }
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        toggle = GetComponent<Toggle>();
        shopName0 = FindChildGameObject<TextMeshProUGUI>("ShopName0");
        shopName1 = FindChildGameObject<TextMeshProUGUI>("ShopName1");
    }
    ShopData shopData;
    SelectAction<ShopData> SelectAction;
    public override void InitData(ShopData t, SelectAction<ShopData> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        base.InitData(t, SelectAction, toggleGroup);
        toggle.group = toggleGroup;
        shopData = t;
        this.SelectAction = SelectAction;
        shopName0.text = shopData.shopName;
        shopName1.text = shopData.shopName;

        shopName0.enabled = !toggle.isOn;
        shopName1.enabled = toggle.isOn;
    }
}