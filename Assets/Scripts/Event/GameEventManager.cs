using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner;
using BehaviorDesigner.Runtime;
using System.Threading.Tasks;

public struct EventReferenceData
{
    public string name;
    public int value;
}
public class GameEventManager:Singleton<GameEventManager>
{ 
    public override void Init()
    {
        base.Init();
        obj = GameObject.Find("GameEventManager");
    }

    private GameObject obj; 
    Dictionary<int, BehaviorTree> behaviorTrees = new Dictionary<int, BehaviorTree>();

    public async Task AddGameEvent(int eventId,List<EventReferenceData> eventReferenceDatas=null)
    {
        GameEventData gameEventData =await GameDataManager.instance.GetAsyncData<GameEventData>(eventId);
        AddGameEvent(gameEventData, eventReferenceDatas);
    }
    private void AddGameEvent(GameEventData gameEventData, List<EventReferenceData> eventReferenceDatas=null)
    {
        BehaviorTree behaviorTree = obj.AddComponent<BehaviorTree>();
        SharedInt idShared = new SharedInt();
        idShared.Name = "ID";
        idShared.SetValue(gameEventData.id);
        behaviorTree.SetVariable("ID", idShared);
          
        behaviorTree.ExternalBehavior = gameEventData.behaviorTree;

        if (eventReferenceDatas != null)
        {
            for(int i = 0; i < eventReferenceDatas.Count; i++)
            {
                var referenceName = eventReferenceDatas[i].name;
                var intReferenceId= eventReferenceDatas[i].value;

                if (!string.IsNullOrEmpty(referenceName))
                {
                    var shared = behaviorTree.GetVariable(referenceName);
                    if (shared != null)
                    {
                        var sharedrefrence = (SharedInt)shared;
                        sharedrefrence.SetValue(intReferenceId);
                    }
                }
            }
        }
        

        behaviorTree.enabled = gameEventData.defaultAwake;
        behaviorTrees[gameEventData.id] = behaviorTree;
    }
    
    public void RemoveGameEvent(int id)
    {
        if(behaviorTrees.TryGetValue(id,out BehaviorTree behaviorTree))
        {
            GameObject.Destroy(behaviorTree);
        }
    }
    public void SetGameEventAwake(int id,bool awake, bool pause)
    {
        if (behaviorTrees.TryGetValue(id, out BehaviorTree behaviorTree))
        {
            behaviorTree.PauseWhenDisabled = pause;
            behaviorTree.enabled = awake;
          
        }
    }
  

}
