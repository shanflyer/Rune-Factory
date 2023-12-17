using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner;
using BehaviorDesigner.Runtime;
using System.Threading.Tasks;
using Unity.Mathematics;

[System.Serializable]
public struct EventReferenceData
{
    public string name;
    public ReferenceValueType valueType;
    public int value;
    public Vector3 vectorValue;
    public int3 int3Value;
}
public enum ReferenceValueType
{
    Int,Float,Int3,Vector3
}
public class GameEventManager:Singleton<GameEventManager>
{ 
    public override void Init()
    {
        base.Init();
        obj = GameObject.Find("GameEventManager");
        if (obj == null)
        {
            obj = new GameObject("GameEventManager");
        }
        Object.DontDestroyOnLoad(obj);
    }

    private GameObject obj; 
    Dictionary<int, BehaviorTree> behaviorTrees = new Dictionary<int, BehaviorTree>();

 
   

    public async Task<bool> AddGameEvent(int eventId,List<EventReferenceData> eventReferenceDatas=null)
    {
        GameEventData gameEventData =await GameDataManager.instance.GetAsyncData<GameEventData>(eventId);
        if(gameEventData== null)
        {
            return false;
        }
        AddGameEvent(gameEventData, eventReferenceDatas);
        return true;
    }
    public void AddGameEvent(GameEventData gameEventData, List<EventReferenceData> eventReferenceDatas=null)
    {
        BehaviorTree behaviorTree = obj.AddComponent<BehaviorTree>();
        SharedInt idShared = new SharedInt();
        idShared.Name = "ID";
        idShared.SetValue(gameEventData.id);
        behaviorTree.SetVariable("ID", idShared);
          
        behaviorTree.ExternalBehavior = gameEventData.behaviorTree;
        if (gameEventData.eventReferenceDatas != null)
        {
            if (eventReferenceDatas != null)
            {
                eventReferenceDatas.AddRange(gameEventData.eventReferenceDatas);
            }
            else
            {
                eventReferenceDatas = gameEventData.eventReferenceDatas;
            }
        }
      

        if (eventReferenceDatas != null)
        {
            for(int i = 0; i < eventReferenceDatas.Count; i++)
            {
                var referenceName = eventReferenceDatas[i].name;
                
              

                if (!string.IsNullOrEmpty(referenceName))
                {
                    var shared = behaviorTree.GetVariable(referenceName);
                    switch (eventReferenceDatas[i].valueType)
                    {
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
