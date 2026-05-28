using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Mathematics;

public partial class Character
{
    private HashSet<int> NeighborhoodCharacters=new HashSet<int>();

    public void TryRefreshNeighborhood(Character character, int range = 8)
    {
        if (mapInstance != character.mapInstance)
        {
            if (NPCManager.instance.GetNPCFormInstance(character.instanceId, out var npc) &&
                npc.startSleepHour >= 0) return;
            if (NeighborhoodCharacters.Contains(character.instanceId))
            {
                RefreshOperateCharacter refreshOperateCharacter = new RefreshOperateCharacter
                {
                    characterId = character.instanceId,
                    join = false
                };
                GameActionManager.instance.QueueAction(refreshOperateCharacter);
            }
        }
        else
        {
            var isSleepNpc = false;
            if (!character.isController && NPCManager.instance.GetNPCFormInstance(character.instanceId, out var npc))
                if (npc.startSleepHour >= 0)
                    isSleepNpc = true;

            int absX = math.abs(character.coordinate.x - coordinate.x);
            int absY = math.abs(character.coordinate.y - coordinate.y);

            if (NeighborhoodCharacters.Contains(character.instanceId))
            {
                if (isSleepNpc)
                {
                    var refreshOperateCharacter = new RefreshOperateCharacter
                    {
                        characterId = character.instanceId,
                        join = false
                    };
                    GameActionManager.instance.QueueAction(refreshOperateCharacter);
                    NeighborhoodCharacters.Remove(character.instanceId);
                    return;
                }
                if (absX > range || absY > range)
                {
                    RefreshOperateCharacter refreshOperateCharacter = new RefreshOperateCharacter
                    {
                        characterId = character.instanceId,
                        join = false
                    };
                    GameActionManager.instance.QueueAction(refreshOperateCharacter);
                    NeighborhoodCharacters.Remove(character.instanceId);
                }
            }
            else
            {
                if (absX <= range && absY <= range)
                {
                    if (!character.isController || isSleepNpc)
                    {
                        RefreshOperateCharacter refreshOperateCharacter = new RefreshOperateCharacter
                        {
                            characterId = character.instanceId,
                            join = false
                        };
                        GameActionManager.instance.QueueAction(refreshOperateCharacter);
                        NeighborhoodCharacters.Remove(character.instanceId);
                    }
                    else
                    {
                        RefreshOperateCharacter refreshOperateCharacter = new RefreshOperateCharacter
                        {
                            characterId = character.instanceId,
                            join = true
                        };
                        GameActionManager.instance.QueueAction(refreshOperateCharacter);
                        NeighborhoodCharacters.Add(character.instanceId);
                    }

                }
            }
        }

    }

    private void RefreshNeighborhood()
    {
        RefreshOperateCharacters refreshOperateCharacters = new RefreshOperateCharacters();
        var NeighborhoodCharacters1 = MapCellController.instance.GetCharacters(objCoordinate);

        NeighborhoodCharacters1.Remove(instanceId);
        if (TeamManager.instance.playerTeam != null)
            NeighborhoodCharacters1.ExceptWith(TeamManager.instance.playerTeam.TeamCharacters);

        HashSet<int> sleepCharacters = new HashSet<int>();
        foreach(var id in NeighborhoodCharacters1)
        {
            if (NPCManager.instance.GetNPCFormInstance(id, out var npc))
            {
                if (npc.startSleepHour >= 0)
                {
                    sleepCharacters.Add(id);
                }
            }
        }
        NeighborhoodCharacters1.ExceptWith(sleepCharacters);

        {
            refreshOperateCharacters.leaveCharacters = NeighborhoodCharacters.Except(NeighborhoodCharacters1).ToHashSet<int>();
            refreshOperateCharacters.joinCharacters= NeighborhoodCharacters1.Except(NeighborhoodCharacters).ToHashSet<int>();
        }
        NeighborhoodCharacters = NeighborhoodCharacters1;
        GameActionManager.instance.QueueAction(refreshOperateCharacters);

        EnvironmentManger.instance.UpDataAudio2DPolygon();
    }

    public void SetNeighborhood(int characterId)
    {
        AsyncTaskRunner.Run(() => SetNeighborhoodAsync(characterId), nameof(SetNeighborhood));
    }

    public async System.Threading.Tasks.Task SetNeighborhoodAsync(int characterId)
    {
        Character character = CharacterManager.instance.GetCharacter(characterId);
        if (character != null)
        {
            EventReferenceData eventReferenceData = new EventReferenceData
            {
                name = "CharacterId",
                value = character.instanceId
            };
            EventReferenceData targetReferenceData = new EventReferenceData
            {
                name = "TargetCharacter",
                value = instanceId
            };

            int nextTalkEventId = 0;
            int eventId = 0;
            if (character is TempCharacter tempCharacter)
            {
                nextTalkEventId = tempCharacter.tempCharacterData.nextTalkEventId;
                eventId = tempCharacter.tempCharacterData.tempTalkEventId;
            }
            else if (NPCManager.instance.GetNPCFormInstance(character.instanceId, out var NPC))
            {
                nextTalkEventId = NPC.nextTalkEventId;
                eventId = NPC.playerOperateEventId;
                /*AddFriendShipValue addFriendShipValue = new AddFriendShipValue
                {
                    characterId = character.instanceId,
                    friendAddType = FriendAddType.对话,
                    value = 1
                };
                GameActionManager.instance.QueueAction(addFriendShipValue);*/
            }

            EventReferenceData NextTalkReferenceData = new EventReferenceData
            {
                name = "NextTalkEventId",
                value = nextTalkEventId
            };
            await GameEventManager.instance.AddGameEvent(
                eventId, new List<EventReferenceData>
                {
                    eventReferenceData,targetReferenceData,NextTalkReferenceData
                });

        }

    }

    /// <summary>
    /// 事件触发
    /// </summary>
    /// <param name="eventid">事件id</param>
    /// <param name="reference">数据id</param>
    /// <param name="enter">是否进入事件</param>
    private void TriggerEventAction(int eventid, int reference, bool enter, bool controller = false)
    {
        AsyncTaskRunner.Run(() => TriggerEventActionAsync(eventid, reference, enter, controller), nameof(TriggerEventAction));
    }

    private async System.Threading.Tasks.Task TriggerEventActionAsync(int eventid, int reference, bool enter, bool controller = false)
    {
        if (team!=null && this != CharacterManager.instance.controllerCharacter)
        {
            return;
        }
        if (eventid == 0 && reference == 0)
        {
            return;
        }

        if (controller)
        {
            if (enter)
            {
                //Debug.Log($"进入触发：{reference}");

                OperateItem = reference;
                ShowMapObjTips showMapObjTips = new ShowMapObjTips
                {
                    id = reference
                };
                GameActionManager.instance.QueueAction(showMapObjTips, true);

                TriggerEnter triggerEnter = new TriggerEnter
                {
                    eventId = reference
                };
                GameActionManager.instance.QueueAction(triggerEnter,true);
            }
            else
            {
                // Debug.Log($"离开触发：{reference}");
                if (OperateItem == reference)
                {
                    OperateItem = -1;
                }
                CloseMapObjTips closeMapObjTips = new CloseMapObjTips
                {
                    id = reference
                };
                GameActionManager.instance.QueueAction(closeMapObjTips, true);
                TriggerExit triggerExit = new TriggerExit
                {
                    eventId = reference
                };
                GameActionManager.instance.QueueAction(triggerExit, true);
            }
        }
        else
        {
            if (enter)
            {
                TriggerEnter triggerEnter = new TriggerEnter
                {
                    eventId = reference
                };
                GameActionManager.instance.QueueAction(triggerEnter, true);
            }
            else
            {
                TriggerExit triggerExit = new TriggerExit
                {
                    eventId = reference
                };
                GameActionManager.instance.QueueAction(triggerExit, true);
            }
        }
        List<EventReferenceData> eventReferenceDatas = new List<EventReferenceData>(2);
        eventReferenceDatas.Add(new EventReferenceData
        {
            name = GameCommon.characterTriggerRenferenceName,
            value = instanceId
        });
        eventReferenceDatas.Add(new EventReferenceData
        {
            name = GameCommon.triggerRenferenceName,
            value = reference
        });

        await GameEventManager.instance.AddGameEvent(eventid, eventReferenceDatas);
    }

    public void SetTriggerMapItem(int reference, int eventId)
    {
        OperateItem = reference;
        var showMapObjTips = new ShowMapObjTips
        {
            id = reference
        };
        GameActionManager.instance.QueueAction(showMapObjTips, true);

        var triggerEnter = new TriggerEnter
        {
            eventId = reference
        };
        GameActionManager.instance.QueueAction(triggerEnter, true);
        var eventReferenceDatas = new List<EventReferenceData>(2);
        eventReferenceDatas.Add(new EventReferenceData
        {
            name = GameCommon.characterTriggerRenferenceName,
            value = instanceId
        });
        eventReferenceDatas.Add(new EventReferenceData
        {
            name = GameCommon.triggerRenferenceName,
            value = reference
        });

        // 触发器在同步坐标更新链路中派发，事件执行失败不能静默丢失。
        AsyncTaskRunner.Run(GameEventManager.instance.AddGameEvent(eventId, eventReferenceDatas), nameof(TriggerEventAction));
    }
}