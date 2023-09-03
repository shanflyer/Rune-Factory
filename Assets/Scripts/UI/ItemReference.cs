using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemReference : UIObjReference
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
    
    public async void InitItemData(Item item,Action SelectAction)
    {
        this.item = item;
        this.SelectAction = SelectAction;
        this.itemData = await GameDataManager.instance.GetAsyncData<ItemData>(item.dataId);

        icon.sprite = itemData.icon;
        text.text = item.count.ToString(); 
    }
     
}
