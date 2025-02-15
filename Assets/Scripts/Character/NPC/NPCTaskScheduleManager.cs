using BehaviorDesigner.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

public class NPCTaskScheduleManager:Singleton<NPCTaskScheduleManager>
{
    Dictionary<int, NPCTaskScheduleData> NPCTaskScheduleDatas = new Dictionary<int, NPCTaskScheduleData>();
    public override async void Init()
    {
        base.Init();
        GameActionManager.instance.AddListener<UpdateGameTime>(UpdateGameTime);
        GameActionManager.instance.AddListener<TryContinueBehavior>(TryContinueBehavior);
        var datas=await GameDataManager.instance.GetAllAsyncData<NPCTaskScheduleData>();
        for(int i = 0; i < datas.Count; i++)
        {
            NPCTaskScheduleDatas[datas[i].id] = datas[i];
        }
    }
    public bool GetTaskScheduleData(int id,out NPCTaskScheduleData data)
    {
        return NPCTaskScheduleDatas.TryGetValue(id, out data);
    }

    MyDic<int, NPCBehavior> npcBehaviorDic = new MyDic<int, NPCBehavior>();

    void UpdateGameTime(UpdateGameTime updateGameTime)
    {
        for(int i = 0; i < npcBehaviorDic.length; i++)
        {
            npcBehaviorDic[i].SetTimeBehaviorTree(updateGameTime);
        }
    }
    void TryContinueBehavior(TryContinueBehavior tryContinueBehavior)
    {
        if (npcBehaviorDic.TryGetValue(tryContinueBehavior.characterId, out var nPCBehavior))
        {
            nPCBehavior.RemoveOverrideHold();
            nPCBehavior.ResetCharacterBehavior();
        }
    }

    public bool SetNowBehaviorTree(int instanceId, UpdateGameTime UpdateGameTime)
    {
        if (npcBehaviorDic.TryGetValue(instanceId, out var nPCBehavior))
        {
          return  nPCBehavior.SetTimeBehaviorTree(UpdateGameTime);
        }
        return false;
    }

    public void AddNpcBehavior(int instanceId, ExternalBehavior externalBehavior)
    {
        if (npcBehaviorDic.TryGetValue(instanceId, out var nPCBehavior))
        {
            nPCBehavior.AddNpcBehavior(externalBehavior);
        }
    }
    public void AddNPCBehavior(int instance)
    {
        NPCBehavior nPCBehavior = new NPCBehavior(instance);
        npcBehaviorDic.Add(instance, nPCBehavior);
    } 
    public void RemoveBehavior(int instance)
    {
        npcBehaviorDic.Remove(instance);
    }

    public NPCBehaviorState GetNPCBehaviorState(int instanceId)
    {
        if(npcBehaviorDic.TryGetValue(instanceId,out var nPCBehavior))
        {
            return nPCBehavior.behaviorState;
        }
        return NPCBehaviorState.NULL;
    }
    public void SetOverrideHold(int instanceId,bool hold)
    {
        if (npcBehaviorDic.TryGetValue(instanceId, out var nPCBehavior))
        {
            nPCBehavior.SetOverrideHold(hold);
        }
    }
    public void PauseCharacterBehavior(int instanceId)
    {
        if (npcBehaviorDic.TryGetValue(instanceId, out var nPCBehavior))
        {
            nPCBehavior.PauseCharacterBehavior();
        }
    }
    public bool GetNPCHoldPos(int instanceId)
    {
        if (npcBehaviorDic.TryGetValue(instanceId, out var nPCBehavior))
        {
            return nPCBehavior.holdPos;
        }
        return false;
    }
    public void SetNowBehaviorTree(int instanceId)
    {
        if (npcBehaviorDic.TryGetValue(instanceId, out var nPCBehavior))
        {
           nPCBehavior.SetNowBehaviorTree();
        }
    }
    public string NowTaskName(int instanceId)
    {
        if (npcBehaviorDic.TryGetValue(instanceId, out var nPCBehavior))
        {
          return  nPCBehavior.NowTaskName;
        }
        return null;
    }
    public void SetNPCTaskScheduleTimeList(int instanceId,List<int> dailyTasks, ExternalBehaviorTree externalBehavior)
    {
        if (npcBehaviorDic.TryGetValue(instanceId, out var nPCBehavior))
        {
            nPCBehavior.SetNPCTaskScheduleTimeList(dailyTasks, externalBehavior);
        }
       
    }
    internal class NPCBehavior
    {
        internal NPCBehavior(int instanceId)
        {
            characterInstance = instanceId; 
            endBehavior = true;
            behaviorCanBreak = false;
        }
        internal async void SetNPCTaskScheduleTimeList(List<int> dailyTasks, ExternalBehaviorTree externalBehavior)
        {
            List<TaskScheduleModelData> taskSheduleModelDatas = new List<TaskScheduleModelData>();
            for (int i = 0; i < dailyTasks.Count; i++)
            {
                var taskSheduleModelData = await GameDataManager.instance.GetAsyncData<TaskScheduleModelData>(dailyTasks[i]);
                taskSheduleModelDatas.Add(taskSheduleModelData);
            }
            nPCTaskScheduleTimeList = new NPCTaskScheduleTimeList(taskSheduleModelDatas);
            //Debug.Log($"nPCTaskScheduleTimeList.ini{npcData.npcName}");
            if (!SetNowBehaviorTree())
            {
                AddNpcBehavior(externalBehavior, true);
            }
        }

        private int characterInstance;
        private Character character
        {
            get
            {
                if (_character == null)
                {
                    _character = CharacterManager.instance.GetCharacter(characterInstance);
                }
                return _character;
            }
        }
        private Character _character;
        private NPCTaskScheduleTimeList nPCTaskScheduleTimeList;
        private bool endBehavior = true;
        private bool behaviorCanBreak = false;

        private void ResetBehaviorState(Behavior behavior)
        {
            if (SingletonType.Cleared)
            {
                return;
            }
            endBehavior = true;
            behaviorCanBreak = false;
            // Debug.Log($"进入回调:{npcData.npcName}");
            var externalBehavior = GetNowTaskScheduleBehavior(out behaviorCanBreak, out var pauseWhenDisabled);
            if (externalBehavior != null)
            {
                // Debug.Log($"ResetBehaviorStat:{externalBehavior.name}--{npcData.npcName}");
                AddNpcBehavior(externalBehavior, PauseWhenDisabled: pauseWhenDisabled);
            }
        }

        public bool SetTimeBehaviorTree(UpdateGameTime UpdateGameTime)
        {
            if (endBehavior || behaviorCanBreak)
            {
                var externalBehavior = GetTimeTaskScheduleBehavior(UpdateGameTime, out behaviorCanBreak, out var pauseWhenDisabled);
                if (externalBehavior != null)
                {
                    AddNpcBehavior(externalBehavior, PauseWhenDisabled: pauseWhenDisabled);
                    return true;
                }
            }
            return false;
        }


        internal string NowTaskName => nowScheduleData.taskName;
        private NPCTaskScheduleData nowScheduleData;
        internal NPCBehaviorState behaviorState => nowScheduleData.behaviorState;

        internal bool holdPos
        {
            get
            {
                if (overrideHold)
                {
                    return _holdPos;
                }
                else
                {
                    return nowScheduleData.holdPos;
                }
            }
        }

        private bool overrideHold;
        private bool _holdPos;

        internal void SetOverrideHold(bool hold)
        {
            overrideHold = true;
            _holdPos = hold;
        }

        internal void RemoveOverrideHold()
        {
            overrideHold = false;
        }

        private bool behaviorIsPause;
        private float pauseTime;

        internal void PauseCharacterBehavior()
        {
            behaviorIsPause = true;
            pauseTime = Time.time;
        }

        internal void ResetCharacterBehavior()
        {
            if (behaviorIsPause)
            {
                behaviorIsPause = false;
                if (Time.time - pauseTime > nowScheduleData.maxPauseTime)
                {
                    SetNowBehaviorTree();
                }
                else
                {
                    StartCharacterBehavior startCharacterBehavior = new StartCharacterBehavior
                    {
                        characterId = characterInstance
                    };
                    GameActionManager.instance.QueueAction(startCharacterBehavior);
                }
            }
        }

        internal ExternalBehaviorTree GetNowTaskScheduleBehavior(out bool behaviorCanBreak, out bool PauseWhenDisabled)
        {
            try
            {
                if (nPCTaskScheduleTimeList != null)
                {
                    if (nPCTaskScheduleTimeList.GetTaskScheduleDataOrder(GameTimeManager.instance.nowHourMinute, ref nowScheduleData))
                    {
                        behaviorCanBreak = nowScheduleData.canBreak;
                        PauseWhenDisabled = nowScheduleData.PauseWhenDisabled;
                        return nowScheduleData.externalBehavior;
                    }
                }
                else
                {
                    Debug.Log($"{character.characterData.characterName}-无nPCTaskScheduleTimeList");
                }
            }
            catch
            {
            }

            behaviorCanBreak = false;
            PauseWhenDisabled = false;
            return null;
        }

        internal void AddNpcBehavior(ExternalBehavior externalBehavior, bool PauseWhenDisabled = false)
        {
            if (externalBehavior == null)
            {
                return;
            }
            try
            {
                if (character.linkItem != 0)
                {
                    TryRemoveLinkMapItemCharacter tryRemoveLinkMapItemCharacter = new TryRemoveLinkMapItemCharacter
                    {
                        linkInstanceId = characterInstance,
                        mapItemInstanceId = character.linkItem
                    };
                    GameActionManager.instance.QueueAction(tryRemoveLinkMapItemCharacter);
                }

                CharacterBehaviorManager.instance.AddBehavior(characterInstance, externalBehavior,
                                      ResetBehaviorState, PauseWhenDisabled, character.name);
                endBehavior = false;
            }
            catch
            {
                Debug.LogError($"NPCbehavior:{character.characterData.characterName}!!!!");
            }
        }

        internal bool SetNowBehaviorTree()
        {
            var externalBehavior = GetNowTaskScheduleBehavior(out behaviorCanBreak, out var pauseWhenDisabled);
            if (externalBehavior != null)
            {
                AddNpcBehavior(externalBehavior, PauseWhenDisabled: pauseWhenDisabled);
                return true;
            }
            return false;
        }

        internal ExternalBehaviorTree GetTimeTaskScheduleBehavior(UpdateGameTime UpdateGameTime, out bool behaviorCanBreak
            , out bool pauseWhenDisabled)
        {
            if (nPCTaskScheduleTimeList == null)
            {
                // Debug.Log($"null nPCTaskScheduleTimeList{npcData.npcName}");
                behaviorCanBreak = false;
                pauseWhenDisabled = false;
                return null;
            }
            if (nPCTaskScheduleTimeList.GetTaskScheduleDataOrder(new int2(UpdateGameTime.hour, UpdateGameTime.minute), ref nowScheduleData))
            {
                behaviorCanBreak = nowScheduleData.canBreak;
                pauseWhenDisabled = nowScheduleData.PauseWhenDisabled;
                return nowScheduleData.externalBehavior;
            }
            behaviorCanBreak = false;
            pauseWhenDisabled = false;
            return null;
        }
    }

}
public enum NPCBehaviorState
{
    NULL = -1, 闲置 = 0, 工作 = 1, 睡眠 = 2,
}

public class NPCTaskScheduleTimeList
{
    private List<TaskScheduleModelData> taskScheduleModelDatas = new List<TaskScheduleModelData>();

    public NPCTaskScheduleTimeList(List<TaskScheduleModelData> taskScheduleModelDatas)
    {
        this.taskScheduleModelDatas = taskScheduleModelDatas;
        nowTimeKeyIndex = 0;
    }

    private int nowTimeKeyIndex;

    public bool GetTaskScheduleDataOrder(int2 time, ref NPCTaskScheduleData nPCTaskScheduleData)
    {
        if (taskScheduleModelDatas[nowTimeKeyIndex].gameTimeKey != time)
        {
            for (int i = 0; i < taskScheduleModelDatas.Count; i++)
            {
                if (taskScheduleModelDatas[i].gameTimeKey == time)
                {
                    nowTimeKeyIndex = i;
                    break;
                }
            }
        }
        if (nowTimeKeyIndex >= taskScheduleModelDatas.Count)
        {
            nowTimeKeyIndex = 0;
        }
        nPCTaskScheduleData = GetTaskSheduleData(time);

        return true;
    }

    private NPCTaskScheduleData GetTaskSheduleData(int2 time)
    {
        if (nowTimeKeyIndex >= taskScheduleModelDatas.Count)
        {
            return null;
        }
        TaskScheduleModelData taskScheduleModelData = taskScheduleModelDatas[nowTimeKeyIndex];
        int startM = taskScheduleModelData.gameTimeKey.minHour * 60 + taskScheduleModelData.gameTimeKey.minMinute;
        int endM = taskScheduleModelData.gameTimeKey.maxHour * 60 + taskScheduleModelData.gameTimeKey.maxMinute;
        int nowM = time.x * 60 + time.y;

        float e_value = (nowM - startM) / (float)(endM - startM);

        GameRandomData gameRandomData = new GameRandomData
        {
            id = -1,
            weightRandom = true,
            barrels = new List<int3>(),
            randomItems = new List<RandomItem>(),
            text = "选择目标"
        };
        for (int i = 0; i < taskScheduleModelData.dailyTaskDataItems.Count; i++)
        {
            int2 dailyItem = taskScheduleModelData.dailyTaskDataItems[i].GetNowTaskRandomValue(e_value);
            RandomItem randomItem = new RandomItem
            {
                itemValue = dailyItem.x,
                randomValue = dailyItem.y,
                maxCount = 1,
                minCount = 1
            };
            gameRandomData.randomItems.Add(randomItem);
        }
        gameRandomData.Pretreatment();
        var randomResults = GameRandom.instance.GetRandomValue(gameRandomData, 1);
        if (randomResults.Count > 0)
        {
            if (NPCTaskScheduleManager.instance.GetTaskScheduleData(randomResults[0].x, out var nPCTaskScheduleData))
            {
                return nPCTaskScheduleData;
            }
        }
        return null;
    }
}