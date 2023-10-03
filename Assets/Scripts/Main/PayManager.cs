using System;
using System.Collections;
using UnityEngine;

public class PayManager : Singleton<PayManager>
{
    public override void Init()
    {
        base.Init();
        nowGold= GameDataManager.instance.UserGameSaveData.otherSaveData.gold;
        nowMoney = GameDataManager.instance.UserGameSaveData.otherSaveData.money;
    }
    public int NowGold=>nowGold;
    public int NowMoney=>nowMoney;

    int nowGold;
    int nowMoney;

    public void PayAction(string title,string notice,int cost, PayType payType,Action afterAction)
    {
        GoldCostEventData goldCostEventData = new GoldCostEventData
        {
            title = title,
            notice = notice,
            costValue = cost,
            payType = payType,
            afterAction = afterAction
        };
        UIManager.instance.ShowGamePanel<GoldCostSelectPanel, GoldCostEventData>(goldCostEventData, 3);
    }


    public bool TryCostGold(int count)
    {
        if (nowGold >= count)
        {
            nowGold -= count;
            GameActionManager.instance.QueueAction(default(RefreshPlayerGold));
            return true;
        }
        else
        {
            PayGold();
        }
        return false;
    }
    void PayGold()
    {

    }
}

public enum PayType
{
    金币,红晶
}
public struct GoldCostEventData : IReferenceData
{
    public string title;
    public string notice;
    public int costValue;
    public PayType payType;
    public Action afterAction;
}