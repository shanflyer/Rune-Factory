using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OperateButtonReference : UIObjReference<OperateData>
{
    [SerializeField]
    TextMeshProUGUI nameText;
    [SerializeField]
    Button button;
    OperateData operateData;
    SelectAction<OperateData> SelectAction;
    private void Awake()
    {
        button.onClick.AddListener(() =>
        {
            if (SelectAction != null)
            {
                SelectAction(operateData);
            }
        });
    }
    public override void InitData(OperateData t, SelectAction<OperateData> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        base.InitData(t, SelectAction, toggleGroup);
        operateData = t;
        nameText.text = operateData.operateName;
        this.SelectAction = SelectAction;
    }
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        nameText = FindChildGameObject<TextMeshProUGUI>("Name");
        button = GetComponent<Button>();
    }
}
