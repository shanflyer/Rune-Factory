
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerOperateManager : Singleton<PlayerOperateManager>
{
    public override void Init()
    {
        base.Init();
        InputManager.instance.AddInputActionDelegate(MyInputNameData.Player_ClickPos, ClickObj);
        GameActionManager.instance.AddListener<ShowMapObjTips>(ShowMapObjTips);
        GameActionManager.instance.AddListener<CloseMapObjTips>(CloseMapObjTips);
        GameActionManager.instance.AddListener<PlayerTalkItem>(PlayerTalkItem);
    }

    async void PlayerTalkItem(PlayerTalkItem playerTalkItem)
    { 
        if(CharacterManager.instance.GetRuntimeCharacterObj(playerTalkItem.characterId,out var characterRuntimeObj))
        {
            if(WorldMapManager.instance.GetRuntimeMapItem(playerTalkItem.ItemId,out var runtimeMapItem))
            {
                MapItemData mapItemData = await GameDataManager.instance.GetAsyncData<MapItemData>(runtimeMapItem.dataId);
                if (mapItemData != null)
                {
                    CharacterResponseData responseData = new CharacterResponseData
                    {
                        talkValue = mapItemData.playerOperateInfo,
                        displayTime = GameCommon.defaultPlayerTalkTime
                    };
                    UIManager.instance.ShowGamePanel<CharacterResponsePanel, CharacterResponseData>(responseData,
                        parent: characterRuntimeObj.runtimeObj.obj as Transform);
                }
            } 
        }
          
    }
    /// <summary>
    /// 事件触发
    /// </summary>
    /// <param name="eventid">事件id</param>
    /// <param name="reference">数据id</param>
    /// <param name="enter">是否进入事件</param>

    void ClickObj(object obj)
    {
        EventSystem.current.UpData();
        if (EventSystem.current.currentSelectedGameObject != null)
        {
            return;
        }

        var mouseScreenPos = (Vector2)obj;
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        int2 targetCoordinate = GameCommon.GetMapCoordinateInt(mousePos);
        Character controller = CharacterManager.instance.controllerCharacter;

        controller.SetPlayerOperate(targetCoordinate);
    }
    void CloseMapObjTips(CloseMapObjTips closeMapObjTips)
    {
        UIManager.instance.CloseGamePanel<OperateButtonPanel>();
    }
    async void ShowMapObjTips(ShowMapObjTips ShowMapObjTips)
    {
        if (ShowMapObjTips.id != 0)
        {
            if (WorldMapManager.instance.GetRuntimeMapItem(ShowMapObjTips.id, out var runtimMapItem))
            {
                MapItemData mapItemData = await GameDataManager.instance.GetAsyncData<MapItemData>(runtimMapItem.dataId);
                if (mapItemData != null&& mapItemData.operateDatas.Count>0)
                {
                    OperateDataList operateDataList = new OperateDataList
                    {
                        OperateDatas = mapItemData.operateDatas
                    };
                    UIManager.instance.ShowGamePanel<OperateButtonPanel, OperateDataList>(operateDataList);
                }
            } 
        }
    }

    public void OperateAction(OperateData operateData, bool selected=true)
    {
        Character controller = CharacterManager.instance.controllerCharacter;
        if (operateData.gameActionData != null)
        {
            operateData.gameActionData.Action(controller.instanceId, controller.triggerItem) ;
        }
    }
    protected override void Clear()
    {
        base.Clear();
    }
}