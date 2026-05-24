using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

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
                action(ItemInfo.item,index);
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
        ActionName.SetSWText(actionName);
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

    public override void InitReferenceData(ItemInfo v)
    {
        base.InitReferenceData(v);
        // 物品详情加载绑定面板生命周期，关闭后旧详情不再覆盖当前内容。
        RunLifecycleTask(token => InitReferenceDataAsync(v, token), nameof(InitReferenceData));
    }

    private async System.Threading.Tasks.Task InitReferenceDataAsync(ItemInfo v, System.Threading.CancellationToken cancellationToken)
    {
        ItemInfo = v;
        switch (v.item.itemType)
        {

            case ItemType.家具:
                HomeEquipmentData homeEquipmentData = await GameDataManager.instance.GetAsyncData<HomeEquipmentData>(v.item.dataId);
                if (ShouldStopLifecycleTask(cancellationToken))
                {
                    return;
                }

                Icon.sprite = homeEquipmentData.icon;
                Name.SetSWText(homeEquipmentData.equipmentName);
                type.SetSWText(homeEquipmentData.homeEquipType);
                Icon.rectTransform.sizeDelta=GameCommon.SetImageSize(homeEquipmentData.icon, new Vector2(32, 32));

                MoneyValue.text = "";
                MoneyIcon.enabled = false;

                Info.SetSWText(homeEquipmentData.info);
                InfoItemValueBg.localScale = Vector3.zero;

                string roomValueText = "所有地方";
                List<string> roomList = new List<string>();
                roomList.Add(roomValueText);
                if (homeEquipmentData.canSetMaps != null && homeEquipmentData.canSetMaps.Count > 0)
                {
                    roomList.Clear();
                    for (int i = 0; i < homeEquipmentData.canSetMaps.Count; i++)
                    {
                        int roomId = homeEquipmentData.canSetMaps[i];
                        roomList.Add(WorldMapManager.instance.GetWorldMap(roomId).mapRoomData.name);
                        if (i < homeEquipmentData.canSetMaps.Count - 1)
                        {
                            roomList.Add(",");
                        }
                    }
                }
                Property.SetADDText("可布置地点:", roomList.ToArray());
                break;
            default:
                ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(v.item.dataId);
                if (ShouldStopLifecycleTask(cancellationToken))
                {
                    return;
                }

                Icon.sprite = itemData.icon;
                Name.SetSWText(itemData.itemName);
                type.SetSWText(itemData.type);
                MoneyIcon.enabled = true;
                MoneyValue.text = $"{itemData.sellPrice}";
                Property.text = itemData.GetProperty();
                Info.SetSWText(itemData.GetInfo());
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
            ActionName.SetSWText(v.ActionName);
            ActionButton.transform.localScale = Vector3.one;
        }
        center.localPosition = new Vector3(centerPos.x, centerPos.y + v.OffsetPos, centerPos.z);
        CloseObj.localScale = ItemInfo.showClose ? Vector3.one : Vector3.zero;
    }

}
