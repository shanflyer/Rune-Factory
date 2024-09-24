using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public struct FunctionButtonList : IReferenceData
{
    public List<FunctionButton> buttons;
}

public struct FunctionButton : IReferenceData
{
    public Sprite sprite;
    public string name;
    public Action action;
}

public class OtherFunctionButtonReference : UIObjReference<FunctionButton>
{
    [SerializeField]
    private Button button;

    [SerializeField]
    private Image Icon;

    [SerializeField]
    private TextMeshProUGUI Name;

    public override async Task InitData(FunctionButton t, SelectAction<FunctionButton> SelectAction = null, ToggleGroup toggleGroup = null)
    {
       await base.InitData(t, SelectAction, toggleGroup);
        Name.text = data.name;
        Icon.sprite = data.sprite;
        Icon.rectTransform.sizeDelta = GameCommon.SetImageSize(data.sprite, new Vector2(32, 32));
        button.onClick.AddListener(() => { data.action(); });
    }

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        button = GetComponent<Button>();
        Name = FindChildGameObject<TextMeshProUGUI>("Name");
        Icon = FindChildGameObject<Image>("Icon");
    }
}