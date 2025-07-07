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
        UIManager.instance.ShowGamePanel<TipsPanel>(layer: 100);
        UIManager.instance.GetGamePanel<TipsPanel>(SetPanel: tipsPanel =>{
            tipsPanel.InitTipsData(title, notice);
        });
       
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