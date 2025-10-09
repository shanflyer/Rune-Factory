using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopSelectReference : UIObjReference<Shop>
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
                SelectAction(data, index);
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

    public override async Task InitData(Shop t, SelectAction<Shop> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        await base.InitData(t, SelectAction, toggleGroup);
        toggle.group = toggleGroup;
        this.SelectAction = SelectAction;
        shopName0.SetSWText(data.shopName);
        shopName1.SetSWText(data.shopName);

        shopName0.enabled = !toggle.isOn;
        shopName1.enabled = toggle.isOn;
    }
}