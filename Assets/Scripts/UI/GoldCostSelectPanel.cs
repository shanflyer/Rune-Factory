using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GoldCostSelectPanel : GamePanel<GoldCostEventData> 
{
    [SerializeField]
    private Text TitleText;
    [SerializeField]
    private Text CostValueText;
    [SerializeField]
    private Image MoneyImage0, MoneyImage1, TotalMonet0, TotalMonet1;
    [SerializeField]
    private Text noticeText;
    [SerializeField]
    private Text TotalText;
    [SerializeField]
    Button yesButton, noButton;

    GoldCostEventData goldCostEventData;

    public override void OnEnable()
    {
        base.OnEnable();
        GameActionManager.instance.AddListener<RefreshPlayerGold>(RefreshPlayerGold);
    }
    public override void OnDisable()
    {
        base.OnDisable();
        GameActionManager.instance.RemoveListener<RefreshPlayerGold>(RefreshPlayerGold);
    }
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        TitleText = FindChildGameObject<Text>("Title");
        noticeText = FindChildGameObject<Text>("Notice");
        CostValueText = FindChildGameObject<Text>("Cost");
        TotalText = FindChildGameObject<Text>("Total");
        MoneyImage0 = FindChildGameObject<Image>("money0");
        MoneyImage1 = FindChildGameObject<Image>("money1");
        TotalMonet0 = FindChildGameObject<Image>("TotalMoney0");
        TotalMonet1= FindChildGameObject<Image>("TotalMoney1");
    }
    void RefreshPlayerGold(RefreshPlayerGold refreshPlayerGold)
    {
        Display();
    }
    void Display()
    {
        TitleText.text = goldCostEventData.title;
        noticeText.text = goldCostEventData.notice;
        CostValueText.text = goldCostEventData.costValue.ToString();
        switch (goldCostEventData.payType)
        {
            case PayType.金币:
                MoneyImage0.enabled = true;
                MoneyImage1.enabled = false;
                TotalMonet0.enabled = true;
                TotalMonet1.enabled = false;
                TotalText.text = PayManager.instance.NowGold.ToString();
                break;
            case PayType.红晶:
                MoneyImage1.enabled = true;
                MoneyImage0.enabled = false;
                TotalMonet1.enabled = true;
                TotalMonet0.enabled = false;
                TotalText.text = PayManager.instance.NowMoney.ToString();
                break;
        }
    }
    protected override void Awake()
    {
        base.Awake();

        yesButton.onClick.AddListener(() =>
        {
            bool costSuccess = PayManager.instance.TryCostGold(goldCostEventData.costValue);
            if (costSuccess)
            {
                if (goldCostEventData.afterAction != null)
                {
                    goldCostEventData.afterAction();
                    Close();
                }
            } 
                
        });
        noButton.onClick.AddListener(Close);

    }
    public override void InitReferenceData(GoldCostEventData v)
    {
        base.InitReferenceData(v);
        goldCostEventData = v;
        Display();
    }
}