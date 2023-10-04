using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Analytics;
using UnityEngine;
using UnityEngine.UI;

public class ItemReference : UIObjReference<Item>
{
    [SerializeField]
    Toggle toggle;
    [SerializeField]
    Image icon;
    [SerializeField]
    Text text;

    public override void SetPanelUISerializeObj()
    {
        toggle = GetComponentInChildren<Toggle>();
        icon = FindChildGameObject<Image>("Icon");
        text = FindChildGameObject<Text>("Text");

        
        base.SetPanelUISerializeObj();
    }
    Item item;
    ItemData itemData;
    SelectAction<Item> SelectAction;
    private void Awake()
    {
        toggle.onValueChanged.AddListener((bool value) =>
        {
            if (value && SelectAction != null)
            {
                SelectAction(item);
            }
        });
    }

    public override async void InitData(Item t, SelectAction<Item> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        this.item = t;
        this.SelectAction = SelectAction;
        this.itemData = await GameDataManager.instance.GetAsyncData<ItemData>(item.dataId);

        toggle.group = toggleGroup;

        icon.sprite = itemData.icon;
        text.text = item.count.ToString();
        base.InitData(t, SelectAction);
    }
}
