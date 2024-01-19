using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.Mathematics;
using UnityEngine;

[TaskCategory("Game/Character")]
[TaskName("角色移动")]
public class CharacterMove : Action
{
    private SharedInt characterId;
    public SharedInt3 target;
    public SharedInt2 offset;
    public bool smartMove;

    // Use this for initialization
    [SerializeField]
    private TaskStatus taskStatus;

    private void MoveEndAction()
    {
        taskStatus = TaskStatus.Success;
        //Debug.Log($"end:{taskStatus}");
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

    private int2 offsetCoordinate = int2.zero;

    public override void OnStart()
    {
        if (!addAction)
        {
            GameActionManager.instance.AddListener<CharacterMoveFailed>(FailedMoveAction);
            addAction = true;
        }

        taskStatus = TaskStatus.Running;
        if (characterId == null || characterId.IsNull())
        {
            characterId = (SharedInt)Owner.GetVariable("CharacterId");
        }
        if (target == null || target.IsNull())
        {
            target = (SharedInt3)Owner.GetVariable("TargetCoordinate");
            if (target == null || target.IsNull())
            {
                taskStatus = TaskStatus.Failure;
                return;
            }
        }
        if (offset != null && !offset.IsNull())
        {
            offsetCoordinate = offset.Value;
        }
        var targetCoordinate = new int3(target.Value.x + offsetCoordinate.x, target.Value.y + offsetCoordinate.y, target.Value.z);
        var character = CharacterManager.instance.GetCharacter(characterId.Value);
        if (character != null)
        {
            if (character.mapInstance == targetCoordinate.z &&
                    character.coordinate.x == targetCoordinate.x && character.coordinate.y == targetCoordinate.y)
            {
                taskStatus = TaskStatus.Success;
            }
            else
            {
                if (!character.MoveCrossMap(targetCoordinate.z, targetCoordinate.xy, MoveEndAction))
                {
                    // taskStatus = TaskStatus.Failure;
                }
            }
        }
        else
        {
            taskStatus = TaskStatus.Failure;
        }
    }

    private void FailedMoveAction(CharacterMoveFailed characterMoveFailed)
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