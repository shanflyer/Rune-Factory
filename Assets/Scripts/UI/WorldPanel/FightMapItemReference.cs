using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class FightMapItemReference : UIObjReference<MapItemReferenceData>
{
    [SerializeField]
    Image Icon;
    [SerializeField]
    Sprite defaultSprite;
    private void Awake()
    {
        
    }
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        Icon = FindChildGameObject<Image>("Icon");
        defaultSprite = Icon.sprite;
    }
    public override async Task InitData(MapItemReferenceData t, SelectAction<MapItemReferenceData> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        await base.InitData(t, SelectAction, toggleGroup);
        if (!t.open)
        {
            Icon.sprite = defaultSprite;
            Icon.color = Color.red;
        }
        else
        {
            ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(t.itemData);
            if (itemData != null)
            {
                Icon.color = Color.white;
                Icon.sprite = itemData.icon; 
            }
        }
        Icon.SetNativeSize();
    }
}
public struct MapItemReferenceData:IReferenceData
{
    public int itemData;
    public bool open;
}