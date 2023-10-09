using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public struct FormulaTypeData:IReferenceData
{
    public FormulaType formulaType;
}
public class FormulaTypeReference : UIObjReference<FormulaTypeData>
{
    Toggle toggle;
    TextMeshProUGUI typeName;

    private void Awake()
    {
        toggle.onValueChanged.AddListener((bool value) =>
        {
            if (SelectAction != null)
            {
                SelectAction(data);
            }
        });
        toggle.isOn = true;
    }
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        toggle = GetComponent<Toggle>();
        typeName = FindChildGameObject<TextMeshProUGUI>("TypeName");
    } 
    public override void InitData(FormulaTypeData t, SelectAction<FormulaTypeData> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        base.InitData(t, SelectAction, toggleGroup);
        typeName.text = data.formulaType.ToString();
    }
}