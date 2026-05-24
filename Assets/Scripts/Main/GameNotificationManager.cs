using System.Collections.Generic;

public class GameNotificationManager : Singleton<GameNotificationManager>
{
    Queue<ItemResultInfo> itemResultInfuse = new Queue<ItemResultInfo>();
    public override void Init()
    {
        base.Init();
    }

    public void DisplayTips(string title, string notice)
    {
        AsyncTaskRunner.Run(() => DisplayTipsAsync(title, notice), nameof(DisplayTips));
    }

    public async System.Threading.Tasks.Task DisplayTipsAsync(string title, string notice)
    {
        await UIManager.instance.ShowGamePanel<TipsPanel>(layer: 100);
        TipsPanel tipsPanel = await UIManager.instance.GetGamePanel<TipsPanel>();
        tipsPanel.InitTipsData(title, notice);
    }

    bool showItemResultInfo = false;
    public void ShowItemResultInfo(ItemResultInfo itemResultInfo)
    {
        if (showItemResultInfo)
        {
            itemResultInfuse.Enqueue(itemResultInfo);
        }
        else
        {
            // 通知入口是同步调用，面板加载异常由统一异步兜底记录。
            AsyncTaskRunner.Run(UIManager.instance.ShowGamePanel<ItemResultPanel, ItemResultInfo>(itemResultInfo), nameof(ShowItemResultInfo));
        }
    }
    public void TryContinueItemResultInfoShow()
    {
        if (itemResultInfuse.Count == 0)
        {
            return;
        }
        var itemResultInfo = itemResultInfuse.Dequeue();
        AsyncTaskRunner.Run(UIManager.instance.ShowGamePanel<ItemResultPanel, ItemResultInfo>(itemResultInfo), nameof(TryContinueItemResultInfoShow));
    }
}
