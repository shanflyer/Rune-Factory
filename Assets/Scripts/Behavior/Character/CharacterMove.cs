using System.Collections;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks; 

[TaskCategory("Game/Character")]
[TaskName("角色移动")]
public class CharacterMove : Action
{
    private SharedInt characterId;
    public SharedInt3 target;
    public bool smartMove;
    // Use this for initialization
    TaskStatus taskStatus;
    
    void MoveEndAction()
    {
        taskStatus = TaskStatus.Success; 
    }
    public override void OnAwake()
    {
        base.OnAwake();
    }
    private bool addAction;
    public override void OnBehaviorComplete()
    {
        base.OnBehaviorComplete();
        GameActionManager.instance.RemoveListener<CharacterMoveFailed>(FailedMoveAction);
        addAction = false;
    }

    public override void OnEnd()
    {
        base.OnEnd();
        GameActionManager.instance.RemoveListener<CharacterMoveFailed>(FailedMoveAction);
        addAction = false;
    }
    public override void OnStart()
    {
        if (!addAction)
        {
            GameActionManager.instance.AddListener<CharacterMoveFailed>(FailedMoveAction);
            addAction = true;
        }

        taskStatus = TaskStatus.Running;
        if (characterId==null|| characterId.IsNull())
        {
            characterId = (SharedInt)Owner.GetVariable("CharacterId");
        }
        if (target==null|| target.IsNull())
        {
            target = (SharedInt3)Owner.GetVariable("TargetCoordinate");
            if (target==null|| target.IsNull())
            {
                taskStatus = TaskStatus.Failure;
                return;
            }
        }
        var character = CharacterManager.instance.GetCharacter(characterId.Value);
        if (character != null)
        {
            if (character.mapInstance == target.Value.z &&
                    character.coordinate.x == target.Value.x && character.coordinate.y == target.Value.y)
            {
                taskStatus = TaskStatus.Success;
            }
            else
            { 
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

    void FailedMoveAction(CharacterMoveFailed characterMoveFailed)
    {
        if (characterMoveFailed.characterId == characterId.Value)
        {
            if (smartMove)
            {
                var character = CharacterManager.instance.GetCharacter(characterId.Value);
                if (!character.MoveCrossMap(target.Value.z, target.Value.xy, MoveEndAction))
                {
                    taskStatus = TaskStatus.Failure;
                }
            }
            else
            {
                taskStatus = TaskStatus.Failure;
            }
            
        }
    }
    public override TaskStatus OnUpdate()
    {
        return taskStatus;
    }
}