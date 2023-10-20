using UnityEngine.UI;
using UnityEngine;
using TMPro;

public struct ItemInfo:IReferenceData
{
    public int itemId;
    public int dataId;
    public int otherValue;
    public string ActionName;
    public SelectAction<ItemInfo> action;
}
public class ItemInfoPanel : GamePanel<ItemInfo>
{
    [SerializeField]
    private TextMeshProUGUI Name, type;
    [SerializeField]
    private Button ActionButton,CloseButton;
    [SerializeField]
    private TextMeshProUGUI ActionName;
    [SerializeField]
    private TextMeshProUGUI Property;
    [SerializeField]
    private TextMeshProUGUI Info;

    private Image Icon; 
    private Image MoneyIcon;
    private TextMeshProUGUI MoneyValue;
    protected override void Awake()
    {
        base.Awake();

        CloseButton.onClick.AddListener(Close);
        
        ActionButton.onClick.AddListener(() =>
        {
            if (action != null)
            {
                action(ItemInfo);
            }
            Close();
        });
    }

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        Name = FindChildGameObject<TextMeshProUGUI>("Name");

        Icon = FindChildGameObject<Image>("Icon");
        MoneyIcon = FindChildGameObject<Image>("MoneyIcon");
        MoneyValue = FindChildGameObject<TextMeshProUGUI>("MoneyValue");

        type = FindChildGameObject<TextMeshProUGUI>("type");
        Property = FindChildGameObject<TextMeshProUGUI>("Property");
        Info = FindChildGameObject<TextMeshProUGUI>("Info");
        ActionName = FindChildGameObject<TextMeshProUGUI>("ActionName");
        ActionButton = FindChildGameObject<Button>("ActionButton");
    }
    public void SetAction(SelectAction<ItemInfo> action, string actionName)
    {
        this.action = action;
        ActionName.text = actionName;
    }

    private ItemInfo ItemInfo; private SelectAction<ItemInfo> action;

    public override async void InitReferenceData(ItemInfo v)
    {
        base.InitReferenceData(v);
        ItemInfo = v;

        ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(v.dataId);
        Icon.sprite = itemData.icon;
        Name.text = itemData.itemName;
        type.text = $"[{itemData.type}]";
        MoneyValue.text = $"{itemData.sellPrice}";
        Property.text = itemData.property.ToString();
        Info.text = itemData.info;
        this.action = v.action;

        if (v.ActionName == null)
        {
            ActionButton.transform.localScale = Vector3.zero;
        }
        else
        {
            this.action = v.action;
            ActionName.text = v.ActionName;
            ActionButton.transform.localScale = Vector3.one;
        }
    }
    
}