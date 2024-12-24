using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.UI;
public class ItemCostSelectPanel : GamePanel<ItemCostEventData>
{
    [SerializeField]
    private TextMeshProUGUI TitleText;
    [SerializeField]
    private Image MoneyImage0, MoneyImage1;
    [SerializeField]
    private TextMeshProUGUI moneyCountText;
    [SerializeField]
    private TextMeshProUGUI noticeText;
    [SerializeField]
    private Button yesButton, noButton;
    [SerializeField]
    CostItem costItem;
    [SerializeField]
    Transform costParent;
    DisplayList<CostItem,MyInt3> costItems; 

    public override void OnEnable()
    {
        base.OnEnable();
        GameActionManager.instance.AddListener<RefreshPlayerGold>(RefreshPlayerGold);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        if (!SingletonType.Cleared)
            GameActionManager.instance.RemoveListener<RefreshPlayerGold>(RefreshPlayerGold);
    }
    protected override void Awake()
    {
        base.Awake();
        costItems = new DisplayList<CostItem, MyInt3>(costItem, costParent);
        noButton.onClick.AddListener(() =>
        {
            if (itemCostEventData.afterAction != null)
            {
                itemCostEventData.afterAction(false);
            }
            Close();
        });
        yesButton.onClick.AddListener(() =>
        {
            bool costSuccess = PayManager.instance.TryCost(itemCostEventData.payType, itemCostEventData.costValue);
            if (costSuccess)
            {
                for(int i=0;i<itemCostEventData.items.Count;i++)
                {
                    int3 costValue = itemCostEventData.items[i].value;
                    int itemId = costValue.x;
                    int costCount = costValue.y;
                    if (PackageManager.instance.GetPlayerItemCount(itemId)<costCount)
                    {
                        costSuccess = false;
                        break;
                    }
                }
            }
            if (costSuccess)
            {
                for (int i = 0; i < itemCostEventData.items.Count; i++)
                {
                    int3 costValue = itemCostEventData.items[i].value;
                    int itemId = costValue.x;
                    int costCount = costValue.y;
                    PackageManager.instance.RemovePlayerPackageItem(itemId, costCount); 
                }
            }
            
            if (itemCostEventData.afterAction != null)
            {
                itemCostEventData.afterAction(costSuccess);
            }
            Close();
        });
    }
    ItemCostEventData itemCostEventData;
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        TitleText = FindChildGameObject<TextMeshProUGUI>("Title");
        noticeText = FindChildGameObject<TextMeshProUGUI>("Notice");
        moneyCountText = FindChildGameObject<TextMeshProUGUI>("MomeyTotal"); 
        MoneyImage0 = FindChildGameObject<Image>("money0");
        MoneyImage1 = FindChildGameObject<Image>("money1");  
        yesButton = FindChildGameObject<Button>("YesButton");
        noButton = FindChildGameObject<Button>("NoButton");

        costItem = FindChildGameObject<CostItem>("CostItem");
        costParent = FindChildGameObject("CostItems");
    }
    void RefreshPlayerGold(RefreshPlayerGold refreshPlayerGold)
    {
        RefreshMonneyDisplay();
    }
    void RefreshMonneyDisplay()
    {
        switch (itemCostEventData.payType)
        {
            case PayType.½ð±Ò:
                MoneyImage0.enabled = true;
                MoneyImage1.enabled = false;
                moneyCountText.text = $"{itemCostEventData.costValue}/{PayManager.instance.NowGold}";
                moneyCountText.color = itemCostEventData.costValue > PayManager.instance.NowGold ? Color.red : Color.green;
                break;

            case PayType.×êÊ¯:
                MoneyImage1.enabled = true;
                MoneyImage0.enabled = false;
                moneyCountText.text = $"{itemCostEventData.costValue}/{PayManager.instance.NowDiamond}";
                moneyCountText.color = itemCostEventData.costValue > PayManager.instance.NowDiamond ? Color.red : Color.green;
                break;
        }
    }
    public override void InitReferenceData(ItemCostEventData v)
    {
        base.InitReferenceData(v);
        itemCostEventData = v;
        TitleText.SetSWText(v.title);
        noticeText.SetSWText(v.notice);
        RefreshMonneyDisplay();
        costItems.InitListData(v.items);
    }

}
public struct ItemCostEventData : IReferenceData
{
    public string title;
    public string notice;
    public int costValue;
    public PayType payType;
    public List<MyInt3> items;
    public SetResult afterAction;
}
