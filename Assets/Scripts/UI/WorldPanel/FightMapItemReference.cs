using UnityEngine;
using UnityEngine.UI;

public class FightMapItemReference : UIObjReference<MapItemReferenceData>
{
    [SerializeField]
    private Image Icon;

    [SerializeField]
    private Sprite defaultSprite;

    private void Awake()
    {
    }

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        Icon = FindChildGameObject<Image>("Icon");
        defaultSprite = Icon.sprite;
    }

    public override async void InitData(MapItemReferenceData t, SelectAction<MapItemReferenceData> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        base.InitData(t, SelectAction, toggleGroup);
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

public struct MapItemReferenceData : IReferenceData
{
    public int itemData;
    public bool open;
}