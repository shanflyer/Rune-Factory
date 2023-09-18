using BehaviorDesigner.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBehaviorManager : Singleton<CharacterBehaviorManager>
{
    public override void Init()
    {
        base.Init();
        obj = GameObject.Find("CharacterBehaviorManager");
        Object.DontDestroyOnLoad(obj);
    }

    private GameObject obj;
    Dictionary<int, BehaviorTree> behaviorTrees = new Dictionary<int, BehaviorTree>();

    public void AddBehavior(int characterId, ExternalBehaviorTree externalBehavior)
    {
        if (!behaviorTrees.TryGetValue(characterId, out BehaviorTree behaviorTree))
        {
            behaviorTree = obj.AddComponent<BehaviorTree>();
            behaviorTrees[characterId] = behaviorTree;
        }
        behaviorTree.StopAllTaskCoroutines();
        behaviorTree.ExternalBehavior = externalBehavior;
        behaviorTree.SetVariableValue("CharacterId", characterId);
        behaviorTree.RestartWhenComplete = true;
        behaviorTree.Start();
    }
    public void DestroyBehavior(int characterId)
    {
        if(behaviorTrees.TryGetValue(characterId,out BehaviorTree behaviorTree))
        {
            Object.Destroy(behaviorTree);
        }
    }

}