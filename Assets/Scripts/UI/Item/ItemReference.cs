using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemReference : UIObjReference<Item>
{
    [SerializeField]
    private Image icon;

    [SerializeField]
    private TextMeshProUGUI text;

    public override void SetPanelUISerializeObj()
    {
        icon = FindChildGameObject<Image>("Icon");
        text = FindChildGameObject<TextMeshProUGUI>("count");

        base.SetPanelUISerializeObj();
    }

    private Item item;
    private ItemData itemData;

    private void Awake()
    {
    }

    public override async Task InitData(Item t, SelectAction<Item> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        this.item = t;
        this.SelectAction = SelectAction;
        this.itemData = await GameDataManager.instance.GetAsyncData<ItemData>(item.dataId);

        icon.sprite = itemData.icon;
        text.text = item.count.ToString();
        base.InitData(t, SelectAction);
    }
}