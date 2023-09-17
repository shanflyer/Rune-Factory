using System.Collections;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Information;

[TaskCategory("Game/Character")]
[TaskName("角色移动")]
public class CharacterMove : Action
{
    public SharedInt characterId;
    public SharedInt3 target;
    // Use this for initialization
    TaskStatus taskStatus;
    
    void MoveEndAction()
    {
        taskStatus = TaskStatus.Success;
    }
    public override void OnStart()
    {
        var character = CharacterManager.instance.GetCharacter(characterId.Value);
        if (character != null)
        {
            if(character.MoveCrossMap(target.Value.z, target.Value.xy, MoveEndAction))
            {
                taskStatus = TaskStatus.Running;
            }
            else
            {
                taskStatus = TaskStatus.Failure;
            }
        }
        else
        {
            taskStatus = TaskStatus.Failure;
        }
    }

    public override TaskStatus OnUpdate()
    {
        return taskStatus;
    }
}