using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CostSelectPanel : GamePanel<CostEventData> 
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

    CostEventData CostEventData;

    public override bool pluralUI => true; 
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
        TitleText.text = CostEventData.title;
        noticeText.text = CostEventData.notice;
        CostValueText.text = CostEventData.costValue.ToString();
        switch (CostEventData.payType)
        {
            case PayType.金币:
                MoneyImage0.enabled = true;
                MoneyImage1.enabled = false;
                TotalMonet0.enabled = true;
                TotalMonet1.enabled = false;
                TotalText.text = PayManager.instance.NowGold.ToString();
                break;
            case PayType.钻石:
                MoneyImage1.enabled = true;
                MoneyImage0.enabled = false;
                TotalMonet1.enabled = true;
                TotalMonet0.enabled = false;
                TotalText.text = PayManager.instance.NowDiamond.ToString();
                break;
        }
    }
    protected override void Awake()
    {
        base.Awake();

        yesButton.onClick.AddListener(() =>
        {
            bool costSuccess = PayManager.instance.TryCost(CostEventData.payType,CostEventData.costValue);
            if (costSuccess)
            {
                if (CostEventData.afterAction != null)
                {
                    CostEventData.afterAction();
                    Close();
                }
            } 
                
        });
        noButton.onClick.AddListener(Close);

    }
    public override void InitReferenceData(CostEventData v)
    {
        base.InitReferenceData(v);
        CostEventData = v;
        Display();
    }
}