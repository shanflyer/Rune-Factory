using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreaterReference : UIObjReference<MoneyCreatData>
{
    [SerializeField]
    Toggle toggle;
    [SerializeField]
    Image icon;
    [SerializeField]
    Text value;
    [SerializeField]
    Text cost;

    MoneyCreatData moneyCreatData;
    private void Awake()
    {
        toggle.onValueChanged.AddListener((bool value) =>
        {
            if (value&& SelectAction!=null)
            {
                SelectAction(moneyCreatData);
            }
        });
    }
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        toggle=GetComponent<Toggle>();
        icon = FindChildGameObject<Image>("Icon");
        value=FindChildGameObject<Text>("Value");
        cost = FindChildGameObject<Text>("CostValue");
    }
    SelectAction<MoneyCreatData> SelectAction;
    public override void InitData(MoneyCreatData t, SelectAction<MoneyCreatData> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        base.InitData(t, SelectAction);
        this.SelectAction = SelectAction;
        moneyCreatData = t;
        toggle.group=toggleGroup;
        icon.sprite = moneyCreatData.Icon;
        value.text = moneyCreatData.getValue.ToString();
        cost.text= moneyCreatData.costValue.ToString();
    }
}
