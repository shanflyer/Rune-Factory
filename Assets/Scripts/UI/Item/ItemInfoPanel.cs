using UnityEngine.UI;
using UnityEngine;
using TMPro;

public struct ItemInfo:IReferenceData
{
    public Item item;
    public string ActionName;
    public bool showClose;
    public SelectAction<Item> action;
    public float OffsetPos;
}
public class ItemInfoPanel : GamePanel<ItemInfo>
{
    [SerializeField]
    private TextMeshProUGUI Name, type;
    [SerializeField]
    private Transform CloseObj;
    [SerializeField]
    private Button ActionButton,CloseButton;
    [SerializeField]
    private TextMeshProUGUI ActionName;
    [SerializeField]
    private TextMeshProUGUI Property;
    [SerializeField]
    private TextMeshProUGUI Info;
    [SerializeField]
    private Image Icon;
    [SerializeField]
    private Image MoneyIcon;
    [SerializeField]
    private TextMeshProUGUI MoneyValue;
    [SerializeField]
    private Transform center;

    [SerializeField]
    Transform InfoItemValueBg;
    [SerializeField]
    Image InfoItemValue;

    private Vector3 centerPos;
    protected override void Awake()
    {
        base.Awake();
        centerPos = center.transform.localPosition;
        CloseButton.onClick.AddListener(Close);
        
        ActionButton.onClick.AddListener(() =>
        {
            if (action != null)
            {
                action(ItemInfo.item);
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

        InfoItemValueBg = FindChildGameObject("InfoItemValueBg");
        InfoItemValue = FindChildGameObject<Image>("InfoItemValue");

        center = FindChildGameObject("Center");

        CloseObj = FindChildGameObject("CloseObj");
        CloseButton = FindChildGameObject<Button>("Close");
    }
    public void SetAction(SelectAction<Item> action, string actionName)
    {
        this.action = action;
        ActionName.text = actionName;
        if (action == null || string.IsNullOrEmpty(actionName))
        {
            ActionButton.transform.localScale = Vector3.zero;
        }
        else
        {
            ActionButton.transform.localScale = Vector3.one;
        }
    }

    private ItemInfo ItemInfo; private SelectAction<Item> action;

    public override async void InitReferenceData(ItemInfo v)
    {
        base.InitReferenceData(v);
        ItemInfo = v;
        switch (v.item.itemType)
        {
           
            case ItemType.家具:
                HomeEquipmentData homeEquipmentData = await GameDataManager.instance.GetAsyncData<HomeEquipmentData>(v.item.dataId);
                Icon.sprite = homeEquipmentData.icon;
                Name.text = homeEquipmentData.equipmentName;
                type.text = homeEquipmentData.homeEquipType.ToString(); 
                Icon.rectTransform.sizeDelta=GameCommon.SetImageSize(homeEquipmentData.icon, new Vector2(32, 32));

                MoneyValue.text = "";
                MoneyIcon.enabled = false;

                Info.text = homeEquipmentData.info;
                InfoItemValueBg.localScale = Vector3.zero;

                string roomValueText = "所有地方";
                if (homeEquipmentData.canSetMaps != null && homeEquipmentData.canSetMaps.Count > 0)
                {
                    roomValueText = "";
                    for (int i = 0; i < homeEquipmentData.canSetMaps.Count; i++)
                    {
                        int roomId = homeEquipmentData.canSetMaps[i];
                        roomValueText += WorldMapManager.instance.GerMapDataName(roomId);
                        if (i < homeEquipmentData.canSetMaps.Count - 1)
                        {
                            roomValueText += ",";
                        }
                    }
                }
                Property.text = $"可布置地点:{roomValueText}";
                break;
            default:
                ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(v.item.dataId);
                Icon.sprite = itemData.icon;
                Name.text = itemData.itemName;
                type.text = $"[{itemData.type}]";
                MoneyIcon.enabled = true;
                MoneyValue.text = $"{itemData.sellPrice}";
                Property.text = itemData.property.ToString();
                Info.text = itemData.info;
                InfoItemValueBg.localScale = itemData.itemValue ? Vector3.one : Vector3.zero;
                InfoItemValue.fillAmount = v.item.value;
                break;
        }
       
        this.action = v.action;
         
        if (string.IsNullOrEmpty(v.ActionName))
        {
            ActionButton.transform.localScale = Vector3.zero;
        }
        else
        {
            this.action = v.action;
            ActionName.text = v.ActionName;
            ActionButton.transform.localScale = Vector3.one;
        }
        center.localPosition = new Vector3(centerPos.x, centerPos.y + v.OffsetPos, centerPos.z);
        CloseObj.localScale = ItemInfo.showClose ? Vector3.one : Vector3.zero;
    }
    
}