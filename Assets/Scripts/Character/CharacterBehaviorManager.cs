using BehaviorDesigner.Runtime;
using System.Collections.Generic;
using UnityEngine;
using System;
using static BehaviorDesigner.Runtime.Behavior;

public class CharacterBehaviorManager : Singleton<CharacterBehaviorManager>
{
    public override async void Init()
    {
        base.Init();
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
       // Object.DontDestroyOnLoad(obj);
    }

    public ExternalBehavior backHomeExternalBehavior { get; private set; }

    private GameObject obj;
    private Dictionary<int, BehaviorTree> behaviorTrees = new Dictionary<int, BehaviorTree>();
    private Dictionary<int, BehaviorHandler> behaviorHandlers = new Dictionary<int, BehaviorHandler>();
    public BehaviorHandler EventStopCharacterBehavior(int characterId)
    {
        if(behaviorTrees.TryGetValue(characterId,out var behaviorTree))
        {
            if(behaviorHandlers.TryGetValue(characterId,out var behaviorHandler))
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
        if(behaviorTrees.TryGetValue(characterId,out var behaviorTree))
        {
            return behaviorTree;
        }
        return null;
    }
    void PauseCharacterBehavior(PauseCharacterBehavior pauseCharacterBehavior)
    {
        if (behaviorTrees.TryGetValue(pauseCharacterBehavior.characterId, out BehaviorTree behaviorTree))
        { 
            behaviorTree.DisableBehavior(true); 
        }
    }
    void ReStartCharacterBehavior(ReStartCharacterBehavior reStartCharacterBehavior)
    {
        if (behaviorTrees.TryGetValue(reStartCharacterBehavior.characterId, out BehaviorTree behaviorTree))
        {
            behaviorTree.StopAllTaskCoroutines();
            // Stop the behavior tree
            behaviorTree.DisableBehavior();
            // Start the behavior tree back up
            behaviorTree.EnableBehavior();
        }
    }
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
            behaviorTree.EnableBehavior();
            if (!behaviorTree.StartWhenEnabled)
            {
                behaviorTree.Start();
            } 
        }
    }
    
    public void AddBehavior(int characterId, ExternalBehavior externalBehavior, BehaviorHandler behaviorHandler=null,
        bool PauseWhenDisabled = false,string behaviorName="")
    {
       
        Character character = CharacterManager.instance.GetCharacter(characterId);
       // Debug.Log($"AddBehavior:{character.name}--{externalBehavior.name}");
        if (!behaviorTrees.TryGetValue(characterId, out BehaviorTree behaviorTree))
        {
            behaviorTree = obj.AddComponent<BehaviorTree>();
            behaviorTrees[characterId] = behaviorTree;
            if (behaviorHandler != null)
            {
                behaviorTree.OnBehaviorEnd += CallBack;
            }
        }
        else
        { 
            character.RemoveMove();
        }
       
        behaviorHandlers[characterId] = behaviorHandler;
        behaviorTree.ExternalBehavior = externalBehavior; 
        behaviorTree.SetVariable("CharacterId", new SharedInt { Value = characterId });
        behaviorTree.RestartWhenComplete = false;
        //behaviorTree.r
        behaviorTree.PauseWhenDisabled = PauseWhenDisabled;
        behaviorTree.enabled = true;
        behaviorTree.EnableBehavior();
        if (!string.IsNullOrEmpty(behaviorName))
        {
            behaviorTree.BehaviorName = behaviorName;
        }

        void CallBack(Behavior behavior)
        {
            if(behavior is  BehaviorTree tree) 
            { 
                tree.StopAllTaskCoroutines();
                tree.DisableBehavior();
                behaviorTree.enabled = false;
                //  tree.SaveResetValues();
               // Debug.Log($"CallBack:{character.name}--{characterId}");
                if (SingletonType.Cleared)
                {
                    return;
                }
                if (behaviorHandlers.TryGetValue(characterId, out var result))
                {
                    result(behavior);
                }
                else
                {
                    Debug.Log($"没有行为回调:{character.name}");
                }
            }
             
        }
    }

    public void DestroyBehavior(int characterId)
    {
        if (behaviorTrees.TryGetValue(characterId, out BehaviorTree behaviorTree))
        {
            GameObject.Destroy(behaviorTree);
            behaviorHandlers.Remove(characterId);
            behaviorTrees.Remove(characterId);
        }
    }
}