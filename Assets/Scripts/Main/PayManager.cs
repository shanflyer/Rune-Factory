 using UnityEngine;
using UnityEngine.Purchasing;

public class PayManager : Singleton<PayManager>
{
    public override async void Init()
    {
        base.Init();
        nowGold = GameDataSaveManager.instance.UserGameSaveData.otherSaveData.gold;
        nowDiamond = GameDataSaveManager.instance.UserGameSaveData.otherSaveData.diamond;
        goldIcon = await GameSourceManager.instance.GetSprite(DataPath.goldSpritePath);
        diamondIcon = await GameSourceManager.instance.GetSprite(DataPath.diamondSpritePath);

        GameActionManager.instance.AddListener<AddPlayerGold>(AddPlayerGold);
    }
 
    public int NowGold => nowGold;
    public int NowDiamond => nowDiamond;

    private Sprite goldIcon, diamondIcon;

    private int nowGold;
    private int nowDiamond;

    public Sprite GetPayMoneySprite(PayType payType)
    {
        if (payType == PayType.金币)
        {
            return goldIcon;
        }
        if (payType == PayType.钻石)
        {
            return diamondIcon;
        }
        return null;
    }

    public void AddGold(int count)
    {
        nowGold += count;
        GameActionManager.instance.QueueAction(default(RefreshPlayerGold));
    }
    void AddPlayerGold(AddPlayerGold addPlayerGold)
    {
        nowGold += addPlayerGold.value;
        GameActionManager.instance.QueueAction(default(RefreshPlayerGold));
    }
    public void AddGold(MoneyCreatData MoneyCreatData)
    {
        PayAction("炼金", $"{string.Format(LanguageManage.SwitchStr("提炼{0}金币"), MoneyCreatData.getValue)}", MoneyCreatData.costValue, MoneyCreatData.costPayType,
                   (bool result) =>
                   {
                       if (result)
                       {
                           nowGold += MoneyCreatData.getValue;
                           GameActionManager.instance.QueueAction(default(RefreshPlayerGold));
                       }
                   });
    }

    public async void PayAction(string title, string notice, int cost, PayType payType, SetResult afterAction)
    {
        CostEventData CostEventData = new CostEventData
        {
            title = title,
            notice = notice,
            costValue = cost,
            payType = payType,
            afterAction = afterAction
        };
       await UIManager.instance.ShowGamePanel<CostSelectPanel, CostEventData>(CostEventData);
    }

    public bool TryCost(PayType payType, int count)
    {
        switch (payType)
        {
            case PayType.金币:
                if (nowGold >= count)
                {
                    nowGold -= count;
                    GameActionManager.instance.QueueAction(default(RefreshPlayerGold));
                    return true;
                }
                else
                {
                    TryCreatGold();
                }
                break;

            case PayType.钻石:
                if (nowDiamond >= count)
                {
                    nowDiamond -= count;
                    GameActionManager.instance.QueueAction(default(RefreshPlayerGold));
                    return true;
                }
                else
                {
                    TryCreatMoney();
                }
                break;
        }

        return false;
    }

    public async void TryCreatGold()
    {
       await UIManager.instance.ShowGamePanel<GoldCreatPanel, IReferenceData>(null);
    }

    public void AddDiamond(int value)
    {
        nowDiamond += value;
        GameActionManager.instance.QueueAction(default(RefreshPlayerGold));
    }
    public void TryCreatMoney()
    {
        UIManager.instance.ShowGamePanel<StoreProductPanel>();

#if UNITY_EDITOR
       // nowDiamond += 200;
      //  GameActionManager.instance.QueueAction(default(RefreshPlayerGold));
#endif
    }
}

public enum ShopItemType
{
    道具 = 0, 动物 = 1, 家具 = 2
}

public enum PayType
{
    金币 = 0, 钻石 = 1, 货币 = 2
}

public struct CostEventData : IReferenceData
{
    public string title;
    public string notice;
    public int costValue;
    public PayType payType;
    public SetResult afterAction;
}