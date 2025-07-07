using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FormulaReference : UIObjReference<FormulaReferenceData>
{
    [SerializeField]
    private TextMeshProUGUI FormulaName;

    [SerializeField]
    private Image Icon;

    [SerializeField]
    private Toggle toggle;

    private void Awake()
    {
        toggle.onValueChanged.AddListener((bool isOn) =>
        {
            if (SelectAction != null)
            {
                SelectAction(data, isOn);
            }
        });
    }

    public override void SelectDefault()
    {
        base.SelectDefault();
        toggle.SetIsOnWithoutNotify(true);

        if (SelectAction != null)
        {
            SelectAction(data, true);
        }
    }

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        toggle = transform.GetComponentInChildren<Toggle>();
        FormulaName = FindChildGameObject<TextMeshProUGUI>("Name");
        Icon = FindChildGameObject<Image>("Icon");
    }

    public override async void InitData(FormulaReferenceData t, SelectAction<FormulaReferenceData> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        base.InitData(t, SelectAction, toggleGroup);
        toggle.group = toggleGroup;
        ItemData productData = data.formulaData.ProductItem;

        if (data.open)
        {
            FormulaName.SetSWText(data.formulaData.formulaName);
            Icon.sprite = productData.icon;
        }
        else
        {
            ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(1);
            Icon.sprite = itemData.icon;
            FormulaName.SetSWText("????");
        }
    }
}

public struct FormulaReferenceData : IReferenceData
{
    public FormulaData formulaData;
    public bool open;
}