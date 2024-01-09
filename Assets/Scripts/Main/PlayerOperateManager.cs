using System;
using System.Collections.Generic;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;

public struct NPCTalkOperateData : IReferenceData
{
    public int characterId;
    public int nextTalkEventId;
    public TalkData defaultTalk;
    public bool displayFunction;
    public List<NPCFunctionData> npcFunctionDatas;
    public Action endAction;
}

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

    private async void PlayerTalkItem(PlayerTalkItem playerTalkItem)
    {
        if (CharacterManager.instance.GetRuntimeCharacterObj(playerTalkItem.characterId, out var characterRuntimeObj))
        {
            if (WorldMapManager.instance.GetRuntimeMapItem(playerTalkItem.ItemId, out var runtimeMapItem))
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

    private void ClickObj(object obj)
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
        int clickCharacter = MapCellController.instance.GetClickCharacter(
            new int3(targetCoordinate, WorldMapObjManager.instance.displayMap));
        if (clickCharacter != -1 && clickCharacter != controller.instanceId)
        {
            Character character = CharacterManager.instance.GetCharacter(clickCharacter);
            if (character != null)
            {
                EventReferenceData eventReferenceData = new EventReferenceData
                {
                    name = "CharacterId",
                    value = clickCharacter
                };
                EventReferenceData targetReferenceData = new EventReferenceData
                {
                    name = "TargetCharacter",
                    value = CharacterManager.instance.controllerCharacter.instanceId
                };
                EventReferenceData NextTalkReferenceData = new EventReferenceData
                {
                    name = "NextTalkEventId",
                    value = character.characterData.nextTalkEventId
                };
                bool temp = character is TempCharacter;
                GameEventManager.instance.AddGameEvent(
                temp ? character.characterData.playerOperateEventId : character.characterData.playerOperateEventId, new List<EventReferenceData>
                {
                    eventReferenceData,targetReferenceData,NextTalkReferenceData
                });
            }
        }
    }

    private void CloseMapObjTips(CloseMapObjTips closeMapObjTips)
    {
        UIManager.instance.CloseGamePanel<OperateButtonPanel>();
    }

    private async void ShowMapObjTips(ShowMapObjTips ShowMapObjTips)
    {
        if (ShowMapObjTips.id != 0)
        {
            if (WorldMapManager.instance.GetRuntimeMapItem(ShowMapObjTips.id, out var runtimMapItem))
            {
                int operateDataLength = runtimMapItem.operateDatas.Count;
                OperateDataList operateDataList = new OperateDataList
                {
                    OperateDatas = new List<OperateDataReferenceData>()
                };
                if (operateDataLength > 0)
                { 
                    foreach(var id in runtimMapItem.operateDatas)
                    {
                        OperateData operateData = await GameDataManager.instance.GetAsyncData<OperateData>(id);
                        operateDataList.OperateDatas.Add(new OperateDataReferenceData
                        {
                            targetItem=runtimMapItem.instanceId,
                            operateData=operateData,
                        });
                    }  
                }
                UIManager.instance.ShowGamePanel<OperateButtonPanel, OperateDataList>(operateDataList);
            }
        }
    }

    public void OperateAction(OperateDataReferenceData operateDataReference, bool selected = true)
    {
        Character controller = CharacterManager.instance.controllerCharacter;
        if (operateDataReference.operateData.gameActionData != null)
        {
            operateDataReference.operateData.gameActionData.Action(controller.instanceId, controller.OperateItem);
        }
        if (operateDataReference.operateData.gameEventData != null)
        {
            List<EventReferenceData> eventReferenceDatas = new List<EventReferenceData>
            {
                new EventReferenceData
                {
                  name = "CharacterId",
                  value = controller.instanceId
                },
                new EventReferenceData
                {
                  name = "TargetItem",
                  value = operateDataReference.targetItem
                },
            };
            if (operateDataReference.operateData.eventReferenceDatas!=null&& operateDataReference.operateData.eventReferenceDatas.Count > 0)
            {
                for(int i = 0; i < operateDataReference.operateData.eventReferenceDatas.Count; i++)
                {
                    eventReferenceDatas.Add(operateDataReference.operateData.eventReferenceDatas[i]);
                }
            }

            GameEventManager.instance.AddGameEvent(operateDataReference.operateData.gameEventData, eventReferenceDatas);
        }
    }

    protected override void Clear()
    {
        base.Clear();
    }
}