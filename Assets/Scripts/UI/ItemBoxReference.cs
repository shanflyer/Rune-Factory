using OldName;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemBoxReference : UIObjReference<Item>
{
    [SerializeField]
    private Toggle toggle;
    [SerializeField]
    private Image icon;
    [SerializeField]
    private Image mask;
    [SerializeField]
    private Image backGround;
    [SerializeField]
    private Text count;

    private int package;

    public bool isEnough;
    [HideInInspector] public bool isFull,isMatch;
    [HideInInspector] public bool isBox, isWareDisplay;
    public Item item;

    SelectAction<Item> SelectAction;
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        toggle = FindChildGameObject<Toggle>("Toggle");
        icon = FindChildGameObject<Image>("Icon");
        mask = FindChildGameObject<Image>("mask");
        backGround = FindChildGameObject<Image>("backGround");
        count = FindChildGameObject<Text>("count");
    }
    private void Awake()
    {
        toggle.onValueChanged.AddListener((bool value) => 
        {
            if (SelectAction != null)
            {
                SelectAction.Invoke(item);
            }
        });
    }
    public override async void InitData(Item t, SelectAction<Item> SelectAction = null)
    {
        base.InitData(t, SelectAction);
        item = t;
        this.SelectAction = SelectAction;
        ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(item.dataId.ToString());

        if (itemData != null)
        {
            mask.enabled = false;

            icon.sprite = itemData.icon;
            icon.color = Color.white;
            icon.enabled = true;
            icon.SetNativeSize();
            count.text = item.count.ToString();
            count.enabled = true;
        }
        else
        {
            mask.enabled = true;
            icon.enabled = false;
            count.enabled = false;
        }
    }

    void SetEnableColor(bool enable)
    {
        isFull = enable;
        mask.enabled = enable;
        isMatch = enable;
        backGround.color= enable?Color.white:new Color(1,0.506f,0.506f);
        
    }
  

    public void Hide()
    {
        mask.enabled = true;
        icon.enabled = false;
        count.enabled = false;
    }
}
