using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NPCFunctionReference:UIObjReference<NPCFunctionData>
{
    [SerializeField]
    Image icon;
    [SerializeField]
    TextMeshProUGUI Name;
    [SerializeField]
    Button button;
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        icon = FindChildGameObject<Image>("Icon");
        Name = FindChildGameObject<TextMeshProUGUI>("Name");
        button = GetComponent<Button>();
    }
    private void Awake()
    {
        button.onClick.AddListener(ClickAction);
    }
    public override void InitData(NPCFunctionData t, SelectAction<NPCFunctionData> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        base.InitData(t, SelectAction, toggleGroup);
        icon.sprite = data.icon;
        Name.text = data.npcFunctionName;
    }
}