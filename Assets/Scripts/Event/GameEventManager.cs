using BehaviorDesigner.Runtime;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;


[System.Serializable]
public struct EventReferenceData
{
    public string name;
    public ReferenceValueType valueType;
    public int value;
    public Vector3 vectorValue;
    public int3 int3Value;
    public List<int> valeList;
}

[System.Serializable]
public struct MapItemEventReferenceData
{
    public string name;
    public int value;
}

public enum ReferenceValueType
{
    Int, Float, Int3, Vector3, IntList
}

public class GameEventManager : Singleton<GameEventManager>
{
    public override void Init()
    {
        base.Init();
        obj = GameObject.Find("GameEventManager");
        if (obj == null)
        {
            obj = new GameObject("GameEventManager");
        }
       // Object.DontDestroyOnLoad(obj);
        GameActionManager.instance.AddListener<ResetGameEvent>(ResetGameEvent);
        GameActionManager.instance.AddListener<RemoveGameEvent>(RemoveGameEvent);
        GameActionManager.instance.AddListener<SampleGameEvent>(SampleGameEvent);
    }

    private GameObject obj;
    private readonly Dictionary<int, BehaviorTreeRunner> behaviorRunners = new Dictionary<int, BehaviorTreeRunner>();
    private readonly List<BehaviorTreeRunner> activeRunners = new List<BehaviorTreeRunner>();
    void SampleGameEvent(SampleGameEvent sampleGameEvent)
    {
        AsyncTaskRunner.Run(SampleGameEventAsync(sampleGameEvent), nameof(SampleGameEvent));
    }

    private async Task SampleGameEventAsync(SampleGameEvent sampleGameEvent)
    {
        // 采样事件需要等事件数据加载完成后再回写结果，避免调用方拿到过早的成功。
        bool result = await AddGameEvent(sampleGameEvent.eventId);
        sampleGameEvent.setResult?.Invoke(result);
    }

    private void RemoveGameEvent(RemoveGameEvent removeGameEvent)
    {
        RemoveGameEvent(removeGameEvent.eventId);
    }

    private void ResetGameEvent(ResetGameEvent resetGameEvent)
    {
        if (behaviorRunners.TryGetValue(resetGameEvent.eventId, out var runner))
        {
            RestartBehavior(runner.tree);
        }
    }

    public async Task<bool> AddGameEvent(int eventId, List<EventReferenceData> eventReferenceDatas = null, bool restEvent = false)
    {
        if (restEvent)
        {
            if (behaviorRunners.TryGetValue(eventId, out var runner))
            {
                if (eventReferenceDatas != null)
                {
                    SetBehaviorTreeReference(runner.tree, eventReferenceDatas);
                }
                RestartBehavior(runner.tree);
                return true;
            }
        }
        GameEventData gameEventData = await GameDataManager.instance.GetAsyncData<GameEventData>(eventId);
        if (gameEventData == null)
        {
            return false;
        }
        AddGameEvent(gameEventData, eventReferenceDatas);
        return true;
    }

    private void SetBehaviorTreeReference(BehaviorTree behaviorTree, List<EventReferenceData> eventReferenceDatas)
    {
        for (int i = 0; i < eventReferenceDatas.Count; i++)
        {
            var referenceName = eventReferenceDatas[i].name;

            if (!string.IsNullOrEmpty(referenceName))
            {
                var shared = behaviorTree.GetVariable(referenceName);
                switch (eventReferenceDatas[i].valueType)
                {
                    case ReferenceValueType.IntList:
                        {
                            var intReferenceId = eventReferenceDatas[i].valeList;
                            if (shared != null)
                            {
                                var sharedrefrence = (SharedIntList)shared;
                                sharedrefrence.SetValue(intReferenceId);
                            }
                            else
                            {
                                SharedIntList sharedList = new SharedIntList();
                                sharedList.SetValue(intReferenceId);
                                behaviorTree.SetVariable(referenceName, sharedList);
                            }
                        }
                        break;

                    case ReferenceValueType.Int:
                        {
                            var intReferenceId = eventReferenceDatas[i].value;
                            if (shared != null)
                            {
                                var sharedrefrence = (SharedInt)shared;
                                sharedrefrence.SetValue(intReferenceId);
                            }
                            else
                            {
                                SharedInt sharedInt = new SharedInt();
                                sharedInt.SetValue(intReferenceId);
                                behaviorTree.SetVariable(referenceName, sharedInt);
                            }
                        }
                        break;

                    case ReferenceValueType.Float:
                        {
                            var intReferenceId = eventReferenceDatas[i].value;
                            if (shared != null)
                            {
                                var sharedrefrence = (SharedFloat)shared;
                                sharedrefrence.SetValue(intReferenceId);
                            }
                            else
                            {
                                SharedFloat sharedFloat = new SharedFloat();
                                sharedFloat.SetValue(intReferenceId);
                                behaviorTree.SetVariable(referenceName, sharedFloat);
                            }
                        }
                        break;

                    case ReferenceValueType.Int3:
                        {
                            var intReferenceId = eventReferenceDatas[i].int3Value;
                            if (shared != null)
                            {
                                var sharedrefrence = (SharedInt3)shared;
                                sharedrefrence.SetValue(intReferenceId);
                            }
                            else
                            {
                                SharedInt3 sharedInt = new SharedInt3();
                                sharedInt.SetValue(intReferenceId);
                                behaviorTree.SetVariable(referenceName, sharedInt);
                            }
                        }
                        break;

                    case ReferenceValueType.Vector3:
                        {
                            var intReferenceId = eventReferenceDatas[i].vectorValue;
                            if (shared != null)
                            {
                                var sharedrefrence = (SharedVector3)shared;
                                sharedrefrence.SetValue(intReferenceId);
                            }
                            else
                            {
                                SharedVector3 sharedVector3 = new SharedVector3();
                                sharedVector3.SetValue(intReferenceId);
                                behaviorTree.SetVariable(referenceName, sharedVector3);
                            }
                        }
                        break;
                }
            }
        }
    }

    public void AddGameEvent(GameEventData gameEventData, List<EventReferenceData> eventReferenceDatas = null)
    {
        if (gameEventData == null)
        {
            Debug.LogError("AddGameEvent failed: gameEventData is null.");
            return;
        }

        if (gameEventData.behaviorTree == null)
        {
            Debug.LogError($"AddGameEvent failed: eventId={gameEventData.id}, eventName={gameEventData.eventName}, behaviorTree is null.");
            return;
        }

        if (gameEventData.bindEvent && behaviorRunners.ContainsKey(gameEventData.id))
        {
            RemoveGameEvent(gameEventData.id);
        }

        var runner = BehaviorTreeRunner.Create(obj, BehaviorRunnerType.Event, gameEventData.id, gameEventData.eventName);
        runner.bound = gameEventData.bindEvent;
        activeRunners.Add(runner);

        BehaviorTree behaviorTree = runner.tree;
        behaviorTree.BehaviorName = $"Event_{gameEventData.id}_{gameEventData.eventName}";
        behaviorTree.RestartWhenComplete = false;
        SharedInt idShared = new SharedInt();
        idShared.Name = BehaviorVariableNames.EventId;
        idShared.SetValue(gameEventData.id);
        behaviorTree.SetVariable(BehaviorVariableNames.EventId, idShared);

        behaviorTree.ExternalBehavior = gameEventData.behaviorTree;
        var references = MergeReferenceData(eventReferenceDatas, gameEventData.eventReferenceDatas);
        if (references != null)
        {
            SetBehaviorTreeReference(behaviorTree, references);
        }

        behaviorTree.enabled = gameEventData.defaultAwake;
        if (gameEventData.defaultAwake)
        {
            behaviorTree.EnableBehavior();
        }

        behaviorTree.OnBehaviorEnd += behavior =>
        {
            if (!runner.bound)
            {
                RemoveRunner(runner);
            }
        };

        if (gameEventData.bindEvent)
        {
            behaviorRunners[gameEventData.id] = runner;
        }

    }

    public void RemoveGameEvent(int id)
    {
        if (behaviorRunners.TryGetValue(id, out var runner))
        {
            RemoveRunner(runner);
        }
    }

    public void RemoveGameEvent(BehaviorTree behaviorTree)
    {
        if (behaviorTree == null)
        {
            return;
        }

        for (int i = activeRunners.Count - 1; i >= 0; i--)
        {
            var runner = activeRunners[i];
            if (runner.tree == behaviorTree)
            {
                RemoveRunner(runner);
                return;
            }
        }
    }

    public void SetGameEventAwake(int id, bool awake, bool pause)
    {
        if (behaviorRunners.TryGetValue(id, out var runner))
        {
            runner.tree.PauseWhenDisabled = pause;
            runner.tree.enabled = awake;
            if (awake)
            {
                runner.tree.EnableBehavior();
            }
        }
    }

    protected override void Clear()
    {
        for (int i = activeRunners.Count - 1; i >= 0; i--)
        {
            activeRunners[i].Destroy();
        }
        activeRunners.Clear();
        behaviorRunners.Clear();
        base.Clear();
    }

    private static List<EventReferenceData> MergeReferenceData(List<EventReferenceData> runtimeReferences, List<EventReferenceData> dataReferences)
    {
        if ((runtimeReferences == null || runtimeReferences.Count == 0) && (dataReferences == null || dataReferences.Count == 0))
        {
            return null;
        }

        var result = new List<EventReferenceData>();
        if (runtimeReferences != null)
        {
            result.AddRange(runtimeReferences);
        }

        if (dataReferences != null)
        {
            result.AddRange(dataReferences);
        }

        return result;
    }

    private void RemoveRunner(BehaviorTreeRunner runner)
    {
        if (runner == null)
        {
            return;
        }

        if (runner.bound)
        {
            behaviorRunners.Remove(runner.ownerId);
        }

        activeRunners.Remove(runner);
        runner.Destroy();
    }

    private static void RestartBehavior(BehaviorTree behaviorTree)
    {
        if (behaviorTree == null)
        {
            return;
        }

        behaviorTree.StopAllTaskCoroutines();
        behaviorTree.DisableBehavior();
        behaviorTree.enabled = false;
        behaviorTree.enabled = true;
        behaviorTree.EnableBehavior();
    }
}
