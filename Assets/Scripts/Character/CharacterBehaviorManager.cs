using BehaviorDesigner.Runtime;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBehaviorManager : Singleton<CharacterBehaviorManager>
{
    public override void Init()
    {
        base.Init();
        obj = GameObject.Find("CharacterBehaviorManager");
        if (obj == null)
        {
            obj = new GameObject("CharacterBehaviorManager");
        }
        GameActionManager.instance.AddListener<StopCharacterBehavior>(StopCharacterBehavior);
        GameActionManager.instance.AddListener<StartCharacterBehavior>(StartCharacterBehavior);
       // Object.DontDestroyOnLoad(obj);
    }

    private GameObject obj;
    private Dictionary<int, BehaviorTree> behaviorTrees = new Dictionary<int, BehaviorTree>();

    private void StopCharacterBehavior(StopCharacterBehavior stopCharacterBehavior)
    {
        if (behaviorTrees.TryGetValue(stopCharacterBehavior.characterId, out BehaviorTree behaviorTree))
        {
            behaviorTree.StopAllTaskCoroutines();
            behaviorTree.enabled = false;
        }
    }

    private void StartCharacterBehavior(StartCharacterBehavior startCharacterBehavior)
    {
        if (behaviorTrees.TryGetValue(startCharacterBehavior.characterId, out BehaviorTree behaviorTree))
        {
            behaviorTree.enabled = true;
            behaviorTree.Start();
        }
    }

    public void AddBehavior(int characterId, ExternalBehaviorTree externalBehavior)
    {
        if (!behaviorTrees.TryGetValue(characterId, out BehaviorTree behaviorTree))
        {
            behaviorTree = obj.AddComponent<BehaviorTree>();
            behaviorTrees[characterId] = behaviorTree;
        }
        behaviorTree.StopAllTaskCoroutines();
        behaviorTree.ExternalBehavior = externalBehavior;
        behaviorTree.SetVariable("CharacterId", new SharedInt { Value = characterId });
        behaviorTree.RestartWhenComplete = true;
        behaviorTree.Start();
    }

    public void DestroyBehavior(int characterId)
    {
        if (behaviorTrees.TryGetValue(characterId, out BehaviorTree behaviorTree))
        {
            Object.Destroy(behaviorTree);
        }
    }
}