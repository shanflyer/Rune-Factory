using System;
using BehaviorDesigner.Runtime;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public override void Init()
    {
        base.Init();
        GameActionManager.instance.AddAsyncListener<ShowMapObjTips>(ShowMapObjTipsAsync, nameof(ShowMapObjTips));
        GameActionManager.instance.AddAsyncListener<CloseMapObjTips>(CloseMapObjTipsAsync, nameof(CloseMapObjTips));
        GameActionManager.instance.AddAsyncListener<ShowItemResult>(ShowItemResultAsync, nameof(ShowItemResult));
    }
    async System.Threading.Tasks.Task ShowItemResultAsync(ShowItemResult showItemResult)
    {
        ItemData itemData =await GameDataManager.instance.GetAsyncData<ItemData>(showItemResult.item);
        ItemResultInfo itemResultInfo = new ItemResultInfo
        {
            actionId = showItemResult.action,
            icon = itemData.icon,
            info0 = showItemResult.info,
            info1 = ""
        };
        GameNotificationManager.instance.ShowItemResultInfo(itemResultInfo);
        //UIManager.instance.ShowGamePanel<ItemResultPanel,ItemResultInfo>(itemResultInfo);
    }
    public void ShowTwoSelectAction(string title, string notice, Action yesAction, Action noAction)
    {
        AsyncTaskRunner.Run(() => ShowTwoSelectActionAsync(title, notice, yesAction, noAction), nameof(ShowTwoSelectAction));
    }

    public async System.Threading.Tasks.Task ShowTwoSelectActionAsync(string title, string notice, Action yesAction, Action noAction)
    {
        TwoSelectData twoSelectData = new TwoSelectData
        {
            title = title,
            notice = notice,
            yesAction = yesAction,
            noAction = noAction
        };
       await UIManager.instance.ShowGamePanel<TwoSelectPanel, TwoSelectData>(twoSelectData);
    }

    private async System.Threading.Tasks.Task CloseMapObjTipsAsync(CloseMapObjTips closeMapObjTips)
    {
        var mapObjTipsPanel = await UIManager.instance.GetGamePanel<MapObjTipsPanel>();
        if (mapObjTipsPanel)
        {
            if (WorldMapObjManager.instance.GetRuntimeMapItemObj(closeMapObjTips.id, out var runtimeObj))
            {
                if (runtimeObj.transform != null)
                {
                    Transform parent = runtimeObj.transform;
                    if (mapObjTipsPanel.transform.parent == parent)
                    {
                        mapObjTipsPanel.Close();
                    }
                }
            }
        }
    }

    private async System.Threading.Tasks.Task ShowMapObjTipsAsync(ShowMapObjTips showMapObjTips)
    {
        int itemId = showMapObjTips.id;
        if (WorldMapObjManager.instance.GetRuntimeMapItemObj(itemId, out var runtimeObj))
        {
            if (runtimeObj.transform != null)
            {
                Transform parent = runtimeObj.transform;
                MapItemData mapItemData = await GameDataManager.instance.GetAsyncData<MapItemData>(runtimeObj.key);
                if (mapItemData != null && mapItemData.displayTips)
                {
                    ShowObjTips(mapItemData.itemName, parent);
                }
            }
        }
    }

    public void ShowObjTips(string info, Transform parent)
    {
        AsyncTaskRunner.Run(() => ShowObjTipsAsync(info, parent), nameof(ShowObjTips));
    }

    public async System.Threading.Tasks.Task ShowObjTipsAsync(string info, Transform parent)
    {
       await UIManager.instance.ShowGamePanel<MapObjTipsPanel>(info, parent: parent);
    }

    public int GetPlayerBoxId()
    {
        int id = (int)GlobalVariables.Instance.GetVariable(GameCommon.PlayerBoxId).GetValue();
        return id;
    }
}
