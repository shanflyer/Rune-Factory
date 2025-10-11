using System;
using System.Collections.Generic;

public class GameNotificationManager : Singleton<GameNotificationManager>
{
    Queue<ItemResultInfo> itemResultInfuse = new Queue<ItemResultInfo>();
    public override void Init()
    {
        base.Init();
    }

    public async void DisplayTips(string title, string notice,Action CloseAction=null)
    {
        await UIManager.instance.ShowGamePanel<TipsPanel>(layer: 100);
        TipsPanel tipsPanel = await UIManager.instance.GetGamePanel<TipsPanel>();
        tipsPanel.InitTipsData(title, notice, CloseAction);
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
            UIManager.instance.ShowGamePanel<ItemResultPanel, ItemResultInfo>(itemResultInfo);
        }
    }
    public void TryContinueItemResultInfoShow()
    {
        if (itemResultInfuse.Count == 0)
        {
            return;
        }
        var itemResultInfo = itemResultInfuse.Dequeue();
        UIManager.instance.ShowGamePanel<ItemResultPanel, ItemResultInfo>(itemResultInfo);
    }
}