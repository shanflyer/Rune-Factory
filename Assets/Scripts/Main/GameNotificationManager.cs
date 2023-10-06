using System.Collections;
using UnityEngine;

public class GameNotificationManager : Singleton<GameNotificationManager>
{
    public override void Init()
    {
        base.Init();
    }
    public async void DisplayTips(string title, string notice)
    {
       await UIManager.instance.ShowGamePanel<TipsPanel>(layer: 100);
        TipsPanel tipsPanel=await UIManager.instance.GetGamePanel<TipsPanel>();
        tipsPanel.InitTipsData(title, notice);
    }
}