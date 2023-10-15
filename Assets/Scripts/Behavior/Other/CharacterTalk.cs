using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.Mathematics;

[TaskCategory("Game/Character")]
[TaskName("角色说话")]
public class CharacterTalk : Action
{
    private SharedInt characterId;
    public SharedInt talkId;
    public SharedBool displayFunction;
    public override void OnStart()
    {
        if (characterId == null || characterId.IsNull())
        {
            characterId = (SharedInt)Owner.GetVariable("CharacterId");
        }
        Talk talk = new Talk
        {
            characterId = characterId.Value,
            talkId = talkId.Value,
            displayFunction = displayFunction.Value
        };
        GameActionManager.instance.QueueAction(talk);

        taskStatus = TaskStatus.Success;
    }
   

    TaskStatus taskStatus;
    public override TaskStatus OnUpdate()
    {
        return taskStatus;
    }
}