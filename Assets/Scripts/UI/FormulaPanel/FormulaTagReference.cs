using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FormulaTagReference : UIObjReference<FormulaType>
{
    [SerializeField]
    private Toggle toggle;

    [SerializeField]
    private TextMeshProUGUI tagName;

    private void Awake()
    {
        toggle.onValueChanged.AddListener((bool value) =>
        {
            if (SelectAction != null)
            {
                SelectAction(data,index, value);
            }
        });
    }
    public override void SelectDefault()
    {
        base.SelectDefault();
        toggle.SetIsOnWithoutNotify(true);
        if (SelectAction != null)
        {
            SelectAction(data,index, true);
        }
    }
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        toggle = GetComponentInChildren<Toggle>();
        tagName = FindChildGameObject<TextMeshProUGUI>("Name");
    }

    public override Task InitData(FormulaType t, SelectAction<FormulaType> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        tagName.SetSWText(t.ToString());
        return base.InitData(t, SelectAction, toggleGroup);
    }
}