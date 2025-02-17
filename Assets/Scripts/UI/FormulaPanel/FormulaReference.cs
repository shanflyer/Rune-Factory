using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing.MiniJSON;
using UnityEngine.UI;

public class FormulaReference : UIObjReference<FormulaReferenceData>
{
    [SerializeField]
    TextMeshProUGUI FormulaName;
    [SerializeField]
    Image Icon;
    [SerializeField]
    Toggle toggle;
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
    public override async Task InitData(FormulaReferenceData t, SelectAction<FormulaReferenceData> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        await base.InitData(t, SelectAction, toggleGroup);
        toggle.group = toggleGroup;
         
        FormulaName.SetSWText(data.formulaData.formulaName);
        ItemData productData = await GameDataManager.instance.GetAsyncData<ItemData>(data.formulaData.Product);
        Icon.sprite = productData.icon;
    }

}
public struct FormulaReferenceData : IReferenceData
{
    public FormulaData formulaData;
    public bool open;
}