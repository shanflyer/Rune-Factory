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

        toggle.onValueChanged.AddListener((bool value) =>
        {
            if (value&&SelectAction!=null)
            {
                SelectAction();
            }
        });
        base.SetPanelUISerializeObj();
    }
    Item item;
    ItemData itemData;
    Action SelectAction;
     
    public override async void InitData(Item t, Action SelectAction = null)
    {
        this.item = t;
        this.SelectAction = SelectAction;
        this.itemData = await GameDataManager.instance.GetAsyncData<ItemData>(item.dataId);

        icon.sprite = itemData.icon;
        text.text = item.count.ToString();
        base.InitData(t, SelectAction);
    }
}
