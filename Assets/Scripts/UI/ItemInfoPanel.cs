using UnityEngine.UI;
using UnityEngine;

public class ItemInfoPanel : GamePanel<Item>
{
    [SerializeField]
    private Text Name, price, type;
    [SerializeField]
    private Button ActionButton;
    [SerializeField]
    private Text ActionName;
    [SerializeField]
    private Text Property;
    [SerializeField]
    private Text Info;

    protected override void Awake()
    {
        base.Awake();

        
        ActionButton.onClick.AddListener(() =>
        {
            if (action != null)
            {
                action(item);
            }
            Close();
        });
    }

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        Name = FindChildGameObject<Text>("Name");
        price = FindChildGameObject<Text>("price");
        type = FindChildGameObject<Text>("type");
        Property = FindChildGameObject<Text>("Property");
        Info = FindChildGameObject<Text>("Info");
        ActionName = FindChildGameObject<Text>("ActionName");
        ActionButton = FindChildGameObject<Button>("ActionButton");
    }
    public void SetAction(SelectAction<Item> action, string actionName)
    {
        this.action = action;
        ActionName.text = actionName;
    }

    private Item item; private SelectAction<Item> action;

    public override async void InitReferenceData(Item v)
    {
        base.InitReferenceData(v);
        item = v;
        ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(item.dataId);
        Name.text = itemData.itemName;
        type.text = $"[{itemData.type}]";
        price.text = $"{itemData.sellPrice}G";
        Property.text = itemData.property.ToString();
        Info.text = itemData.text1;
    }
}