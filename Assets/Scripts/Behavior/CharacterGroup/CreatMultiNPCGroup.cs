using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;


[TaskCategory("Game/CharacterGroup")]
[TaskName("创建NPC群体")]
public class CreatMultiNPCGroup : Action
{
    [SerializeField]
    private List<SharedInt> npcList;
    [SerializeField]
    private SharedInt multiDataId;
    [SerializeField]
    private bool faceCenter;
    public override void OnStart()
    {
        List<int> npcs = new List<int>();
        for(int i = 0; i < npcList.Count; i++)
        {
            npcs.Add(npcList[i].Value);
        }
        CreatMultiNPCBehaviorGroup creatMultiNPCBehaviorGroup = new CreatMultiNPCBehaviorGroup
        {
            characters = npcs,
            dataId = multiDataId.Value,
            faceCenter = faceCenter
        };
        GameActionManager.instance.QueueAction(creatMultiNPCBehaviorGroup, true);
    }

    public override TaskStatus OnUpdate()
    {
        return TaskStatus.Success;
    }
}