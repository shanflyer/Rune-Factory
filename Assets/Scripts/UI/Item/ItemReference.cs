using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Analytics;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemReference : UIObjReference<Item>
{
    
    [SerializeField]
    Image icon;
    [SerializeField]
    TextMeshProUGUI text;

    public override void SetPanelUISerializeObj()
    {
        icon = FindChildGameObject<Image>("Icon");
        text = FindChildGameObject<TextMeshProUGUI>("count");

        
        base.SetPanelUISerializeObj();
    }
    Item item;
    ItemData itemData;
    private void Awake()
    {
       
    }

    public override async void InitData(Item t, SelectAction<Item> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        this.item = t;
        this.SelectAction = SelectAction;
        this.itemData = await GameDataManager.instance.GetAsyncData<ItemData>(item.dataId);


        icon.sprite = itemData.icon;
        text.text = item.count.ToString();
        base.InitData(t, SelectAction);
    }
}
