using BehaviorDesigner.Runtime;
using System.Collections.Generic;
using UnityEngine;
using static BehaviorDesigner.Runtime.Behavior;

public class CharacterBehaviorManager : Singleton<CharacterBehaviorManager>
{
    private System.Threading.Tasks.Task initializationTask = System.Threading.Tasks.Task.CompletedTask;
    public override System.Threading.Tasks.Task InitializationTask => initializationTask;

    public override void Init()
    {
        base.Init();
        initializationTask = InitAsync();
    }

    private async System.Threading.Tasks.Task InitAsync()
    {
        obj = GameObject.Find("CharacterBehaviorManager");
        if (obj == null)
        {
            obj = new GameObject("CharacterBehaviorManager");
        }
        backHomeExternalBehavior = await GameSourceManager.instance.GetBehavior(GameCommon.backHomeBehaviorPath);

        GameActionManager.instance.AddListener<StopCharacterBehavior>(StopCharacterBehavior);
        GameActionManager.instance.AddListener<StartCharacterBehavior>(StartCharacterBehavior);
        GameActionManager.instance.AddListener<ReStartCharacterBehavior>(ReStartCharacterBehavior);
        GameActionManager.instance.AddListener<PauseCharacterBehavior>(PauseCharacterBehavior);
    }

    protected override void Clear()
    {
        initializationTask = System.Threading.Tasks.Task.CompletedTask;
        foreach (var runner in behaviorRunners.Values)
        {
            runner.Destroy();
        }
        behaviorRunners.Clear();
        behaviorTrees.Clear();
        behaviorHandlers.Clear();
        base.Clear();
    }

    public ExternalBehavior backHomeExternalBehavior { get; private set; }

    private GameObject obj;
    private Dictionary<int, BehaviorTree> behaviorTrees = new Dictionary<int, BehaviorTree>();
    private Dictionary<int, BehaviorTreeRunner> behaviorRunners = new Dictionary<int, BehaviorTreeRunner>();
    private Dictionary<int, BehaviorHandler> behaviorHandlers = new Dictionary<int, BehaviorHandler>();

    public BehaviorHandler EventStopCharacterBehavior(int characterId)
    {
        if (behaviorTrees.TryGetValue(characterId, out var behaviorTree))
        {
            if (behaviorHandlers.TryGetValue(characterId, out var behaviorHandler))
            {
                behaviorHandlers.Remove(characterId);
                return behaviorHandler;
            }

            behaviorTree.StopAllTaskCoroutines();
            behaviorTree.enabled = false;
        }
        return null;
    }

    public BehaviorTree GetCharacterBehaviorTree(int characterId)
    {
        if (behaviorTrees.TryGetValue(characterId, out var behaviorTree))
        {
            return behaviorTree;
        }
        return null;
    }

    public string GetCharacterBehaviorName(int characterId)
    {
        if (behaviorTrees.TryGetValue(characterId, out var behaviorTree))
        {
            if (!string.IsNullOrEmpty(behaviorTree.BehaviorName))
            {
                return behaviorTree.BehaviorName;
            }

            return behaviorTree.ExternalBehavior != null ? behaviorTree.ExternalBehavior.name : null;
        }

        return null;
    }

    private void PauseCharacterBehavior(PauseCharacterBehavior pauseCharacterBehavior)
    {
        if (behaviorTrees.TryGetValue(pauseCharacterBehavior.characterId, out var behaviorTree))
        {
            behaviorTree.DisableBehavior(true);
        }
    }

    private void ReStartCharacterBehavior(ReStartCharacterBehavior reStartCharacterBehavior)
    {
        if (behaviorTrees.TryGetValue(reStartCharacterBehavior.characterId, out var behaviorTree))
        {
            behaviorTree.StopAllTaskCoroutines();
            behaviorTree.DisableBehavior();
            behaviorTree.EnableBehavior();
        }
    }

    private void StopCharacterBehavior(StopCharacterBehavior stopCharacterBehavior)
    {
        if (behaviorTrees.TryGetValue(stopCharacterBehavior.characterId, out var behaviorTree))
        {
            behaviorTree.StopAllTaskCoroutines();
            behaviorTree.enabled = false;
        }
    }

    private void StartCharacterBehavior(StartCharacterBehavior startCharacterBehavior)
    {
        if (behaviorTrees.TryGetValue(startCharacterBehavior.characterId, out var behaviorTree))
        {
            behaviorTree.enabled = true;
            behaviorTree.EnableBehavior();
            if (!behaviorTree.StartWhenEnabled)
            {
                behaviorTree.Start();
            }
        }
    }

    public void AddBehavior(int characterId, ExternalBehavior externalBehavior, BehaviorHandler behaviorHandler = null,
        bool PauseWhenDisabled = false, string behaviorName = "")
    {
        if (externalBehavior == null)
        {
            Debug.LogError($"AddBehavior failed: characterId={characterId}, externalBehavior is null.");
            return;
        }

        Character character = CharacterManager.instance.GetCharacter(characterId);
        var previousBehaviorName = GetCharacterBehaviorName(characterId);
        if (!behaviorTrees.TryGetValue(characterId, out var behaviorTree))
        {
            var runnerName = !string.IsNullOrEmpty(behaviorName) ? behaviorName : character?.name;
            var runner = BehaviorTreeRunner.Create(obj, BehaviorRunnerType.Character, characterId, runnerName);
            behaviorRunners[characterId] = runner;
            behaviorTree = runner.tree;
            behaviorTree.OnBehaviorEnd += behavior => OnBehaviorEnd(characterId, behavior);
            behaviorTrees[characterId] = behaviorTree;
        }
        else
        {
            character?.RemoveMove();
            if (behaviorRunners.TryGetValue(characterId, out var runner) && !string.IsNullOrEmpty(behaviorName))
            {
                runner.Rename(behaviorName);
            }
        }

        if (behaviorHandler != null)
        {
            behaviorHandlers[characterId] = behaviorHandler;
        }
        else
        {
            behaviorHandlers.Remove(characterId);
        }

        if (behaviorTree.ExternalBehavior != null || behaviorTree.enabled)
        {
            behaviorTree.StopAllTaskCoroutines();
            behaviorTree.DisableBehavior();
            behaviorTree.enabled = false;
        }

        behaviorTree.ExternalBehavior = externalBehavior;
        behaviorTree.SetVariable(BehaviorVariableNames.CharacterId, new SharedInt { Value = characterId });
        behaviorTree.RestartWhenComplete = false;
        behaviorTree.PauseWhenDisabled = PauseWhenDisabled;
        behaviorTree.enabled = true;
        behaviorTree.EnableBehavior();
        if (!string.IsNullOrEmpty(behaviorName))
        {
            behaviorTree.BehaviorName = behaviorName;
        }

        LogBehaviorChange(characterId, character?.name, previousBehaviorName, externalBehavior.name, PauseWhenDisabled);
    }

    public void DestroyBehavior(int characterId)
    {
        if (behaviorRunners.TryGetValue(characterId, out var runner))
        {
            runner.Destroy();
            behaviorRunners.Remove(characterId);
            behaviorHandlers.Remove(characterId);
            behaviorTrees.Remove(characterId);
        }
    }

    private void OnBehaviorEnd(int characterId, Behavior behavior)
    {
        if (behavior is BehaviorTree tree)
        {
            tree.StopAllTaskCoroutines();
            tree.DisableBehavior();
            tree.enabled = false;
        }

        if (SingletonType.Cleared)
        {
            return;
        }

        if (behaviorHandlers.TryGetValue(characterId, out var result) && result != null)
        {
            result(behavior);
        }
        else
        {
            var character = CharacterManager.instance.GetCharacter(characterId);
            Debug.Log($"No behavior callback:{character?.name ?? characterId.ToString()}");
        }
    }

    private void LogBehaviorChange(int characterId, string characterName, string previousBehaviorName,
        string nextBehaviorName, bool pauseWhenDisabled)
    {
        CharacterDebugSettings.RecordEvent(
            CharacterDebugEventType.BehaviorChange,
            characterId,
            characterName,
            $"previous={previousBehaviorName ?? "null"} next={nextBehaviorName ?? "null"} pauseWhenDisabled={pauseWhenDisabled}",
            CharacterDebugSettings.EnableBehaviorLogs);
    }
}
