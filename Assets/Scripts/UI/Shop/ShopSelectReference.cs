using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopSelectReference : UIObjReference<ShopData>
{
    [SerializeField]
    private TextMeshProUGUI shopName0, shopName1;

    [SerializeField]
    private Toggle toggle;

    private void Awake()
    {
        toggle.onValueChanged.AddListener((bool value) =>
        {
            shopName0.enabled = !value;
            shopName1.enabled = value;
            if (value && SelectAction != null)
            {
                SelectAction(data);
            }
        });
    }
    public override void ClearSelect()
    {
        base.ClearSelect();
        toggle.SetIsOnWithoutNotify(false);
    }
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        toggle = GetComponent<Toggle>();
        shopName0 = FindChildGameObject<TextMeshProUGUI>("ShopName0");
        shopName1 = FindChildGameObject<TextMeshProUGUI>("ShopName1");
    }

    public override void InitData(ShopData t, SelectAction<ShopData> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        base.InitData(t, SelectAction, toggleGroup);
        toggle.group = toggleGroup;
        this.SelectAction = SelectAction;
        shopName0.text = data.shopName;
        shopName1.text = data.shopName;

        shopName0.enabled = !toggle.isOn;
        shopName1.enabled = toggle.isOn;
    }
}