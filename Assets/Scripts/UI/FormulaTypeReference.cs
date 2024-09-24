using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public struct FormulaTypeData : IReferenceData
{
    public FormulaType formulaType;
}

public class FormulaTypeReference : UIObjReference<FormulaTypeData>
{
    [SerializeField]
    private Toggle toggle;

    [SerializeField]
    private TextMeshProUGUI typeName;

    private void Awake()
    {
        toggle.onValueChanged.AddListener((bool value) =>
        {
            if (SelectAction != null)
            {
                SelectAction(data, value);
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

    public override async Task InitData(FormulaTypeData t, SelectAction<FormulaTypeData> SelectAction = null, ToggleGroup toggleGroup = null)
    {
       await base.InitData(t, SelectAction, toggleGroup);
        typeName.text = data.formulaType.ToString();
    }

    public override void ClearSelect()
    {
        base.ClearSelect();
        toggle.SetIsOnWithoutNotify(true);
    }
}