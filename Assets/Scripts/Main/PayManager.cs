using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Purchasing;

public class PayManager : Singleton<PayManager>
{
    private Task initializationTask = Task.CompletedTask;
    public override Task InitializationTask => initializationTask;
    public override System.Collections.Generic.IReadOnlyList<System.Type> InitializationDependencies => new[] { typeof(GameSourceManager), typeof(GameActionManager) };

    public override void Init()
    {
        base.Init();
        initializationTask = InitAsync();
    }

    private async Task InitAsync()
    {
        goldIcon = await GameSourceManager.instance.GetSprite(DataPath.goldSpritePath);
        diamondIcon = await GameSourceManager.instance.GetSprite(DataPath.diamondSpritePath);
        if (goldIcon == null)
        {
            Debug.LogError($"PayManager init failed: missing gold sprite at {DataPath.goldSpritePath}");
        }

        if (diamondIcon == null)
        {
            Debug.LogError($"PayManager init failed: missing diamond sprite at {DataPath.diamondSpritePath}");
        }

        GameActionManager.instance.AddListener<AddPlayerGold>(AddPlayerGold);
    }

    protected override void Clear()
    {
        initializationTask = Task.CompletedTask;
        base.Clear();
    }

    public int NowGold => GameDataSaveManager.instance.UserGameSaveData.otherSaveData.gold;
    public int NowDiamond => GameDataSaveManager.instance.UserGameSaveDataList.commonSaveData.diamond;

    private Sprite goldIcon, diamondIcon;

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
        GameDataSaveManager.instance.UserGameSaveData.otherSaveData.gold += count;
        GameActionManager.instance.QueueAction(default(RefreshPlayerGold));
    }

    void AddPlayerGold(AddPlayerGold addPlayerGold)
    {
        GameDataSaveManager.instance.UserGameSaveData.otherSaveData.gold += addPlayerGold.value;
        GameActionManager.instance.QueueAction(default(RefreshPlayerGold));
    }

    public void AddGold(MoneyCreatData MoneyCreatData)
    {
        PayAction("炼金", $"{string.Format(LanguageManage.SwitchStr("提炼{0}金币"), MoneyCreatData.getValue)}", MoneyCreatData.costValue, MoneyCreatData.costPayType,
                   (bool result) =>
                   {
                       if (result)
                       {
                           GameDataSaveManager.instance.UserGameSaveData.otherSaveData.gold += MoneyCreatData.getValue;
                           GameActionManager.instance.QueueAction(default(RefreshPlayerGold));
                       }
                   });
    }

    public void PayAction(string title, string notice, int cost, PayType payType, SetResult afterAction)
    {
        AsyncTaskRunner.Run(() => PayActionAsync(title, notice, cost, payType, afterAction), nameof(PayAction));
    }

    public async System.Threading.Tasks.Task PayActionAsync(string title, string notice, int cost, PayType payType, SetResult afterAction)
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
                if (NowGold >= count)
                {
                    GameDataSaveManager.instance.UserGameSaveData.otherSaveData.gold -= count;
                    GameActionManager.instance.QueueAction(default(RefreshPlayerGold));
                    return true;
                }
                else
                {
                    TryCreatGold();
                }
                break;

            case PayType.钻石:
                if (NowDiamond >= count)
                {
                    GameDataSaveManager.instance.UserGameSaveDataList.commonSaveData.diamond -= count;
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

    public void TryCreatGold()
    {
        AsyncTaskRunner.Run(TryCreatGoldAsync, nameof(TryCreatGold));
    }

    public async System.Threading.Tasks.Task TryCreatGoldAsync()
    {
        await UIManager.instance.ShowGamePanel<GoldCreatPanel, IReferenceData>(null);
    }

    public void AddDiamond(int value)
    {
        GameDataSaveManager.instance.UserGameSaveDataList.commonSaveData.diamond += value;
        GameActionManager.instance.QueueAction(default(RefreshPlayerGold));
        GameDataSaveManager.instance.RefreshUserCommonSaveData(NowDiamond);
    }

    public void TryCreatMoney()
    {
        // 打开商品面板不阻塞按钮回调，异步失败统一记录。
        AsyncTaskRunner.Run(UIManager.instance.ShowGamePanel<StoreProductPanel>(), nameof(TryCreatMoney));

#if UNITY_EDITOR
        // nowDiamond += 200;
        // GameActionManager.instance.QueueAction(default(RefreshPlayerGold));
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
