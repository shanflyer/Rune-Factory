using System.Collections;
using UnityEngine;
using System;
using UnityEngine.TextCore.Text;
using Unity.Mathematics;
using BehaviorDesigner.Runtime;

public class GameManager : Singleton<GameManager>
{
    public override void Init()
    {
        base.Init();
        GameActionManager.instance.AddListener<ShowMapObjTips>(ShowMapObjTips);
        GameActionManager.instance.AddListener<CloseMapObjTips>(CloseMapObjTips);
      
    }
    
    public void ShowTwoSelectAction(string title, string notice, Action yesAction, Action noAction)
    {
        TwoSelectData twoSelectData = new TwoSelectData
        {
            title = title,
            notice = notice,
            yesAction = yesAction,
            noAction = noAction
        };
        UIManager.instance.ShowGamePanel<TwoSelectPanel,TwoSelectData>(twoSelectData);
    }
    async void CloseMapObjTips(CloseMapObjTips closeMapObjTips)
    {
        var mapObjTipsPanel =await UIManager.instance.GetGamePanel<MapObjTipsPanel>();
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
    async void ShowMapObjTips(ShowMapObjTips showMapObjTips)
    {
        int itemId = showMapObjTips.id;
        if (WorldMapObjManager.instance.GetRuntimeMapItemObj(itemId, out var runtimeObj))
        {
            if (runtimeObj.transform != null)
            {
                Transform parent = runtimeObj.transform; 
                MapItemData mapItemData = await GameDataManager.instance.GetAsyncData<MapItemData>(runtimeObj.key);
                if (mapItemData != null&&mapItemData.displayTips)
                {
                    ShowObjTips(mapItemData.itemName, parent);
                }
            }
        }
    }
    public void ShowObjTips(string info,Transform parent)
    {
        UIManager.instance.ShowGamePanel<MapObjTipsPanel>(info, parent: parent);
    }
    public int GetPlayerBoxId()
    {
       int id=(int) GlobalVariables.Instance.GetVariable(GameCommon.PlayerBoxId).GetValue();
        return id;
    }
}