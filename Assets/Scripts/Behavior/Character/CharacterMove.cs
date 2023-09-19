using System.Collections;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Information;

[TaskCategory("Game/Character")]
[TaskName("角色移动")]
public class CharacterMove : Action
{
    private SharedInt characterId;
    public SharedInt3 target;
    // Use this for initialization
    TaskStatus taskStatus;
    
    void MoveEndAction()
    {
        taskStatus = TaskStatus.Success; 
    }
    public override void OnStart()
    {
        taskStatus = TaskStatus.Running;
        if (characterId == null)
        {
            characterId = (SharedInt)Owner.GetVariable("CharacterId");
        }
        if (target == null)
        {
            target = (SharedInt3)Owner.GetVariable("TargetCoordinate");
            if (target == null)
            {
                target = new SharedInt3();
                Owner.SetVariable("TargetCoordinate", target);
            }
        }
        var character = CharacterManager.instance.GetCharacter(characterId.Value);
        if (character != null)
        {
            if (character.objCoordinate.mapInstance == target.Value.z &&
                    character.objCoordinate.x == target.Value.x && character.objCoordinate.y == target.Value.y)
            {
                taskStatus = TaskStatus.Success;
            }
            else
            {
                Debug.Log($"characterPos:{character.objCoordinate}-{target.Value}");
                if (!character.MoveCrossMap(target.Value.z, target.Value.xy, MoveEndAction))
                { 
                    taskStatus = TaskStatus.Failure;
                }
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