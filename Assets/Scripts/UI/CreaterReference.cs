using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreaterReference : UIObjReference<MoneyCreatData>
{
    [SerializeField]
    private Toggle toggle;

    [SerializeField]
    private Image icon;

    [SerializeField]
    private TextMeshProUGUI value;

    [SerializeField]
    private TextMeshProUGUI cost;

    private MoneyCreatData moneyCreatData;

    private void Awake()
    {
        toggle.onValueChanged.AddListener((bool value) =>
        {
            if (value && SelectAction != null)
            {
                SelectAction(moneyCreatData);
            }
        });
    }

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        toggle = GetComponent<Toggle>();
        icon = FindChildGameObject<Image>("Icon");
        value = FindChildGameObject<TextMeshProUGUI>("Value");
        cost = FindChildGameObject<TextMeshProUGUI>("CostValue");
    }
     

    public override async Task InitData(MoneyCreatData t, SelectAction<MoneyCreatData> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        await  base.InitData(t, SelectAction);
        this.SelectAction = SelectAction;
        moneyCreatData = t;
        toggle.group = toggleGroup;
        icon.sprite = moneyCreatData.Icon.sprite;
        value.text = moneyCreatData.getValue.ToString();
        cost.text = moneyCreatData.costValue.ToString();
    }
}