using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.Mathematics;
using UnityEngine;

[TaskCategory("Game/Character")]
[TaskName("角色移动")]
public class CharacterMove : Action
{
    [SerializeField]
    private SharedInt characterId;
    [SerializeField]
    private SharedInt3 target;
    public SharedInt2 offset;
    public bool smartMove;
    public float waitDuration=70;
    // Use this for initialization
    [SerializeField]
    private TaskStatus taskStatus;


    Character character;
    private void MoveEndAction()
    {
        taskStatus = TaskStatus.Success;
        if(character is TempCharacter tempCharacter)
        {
            tempCharacter.EndMove();
        }
        //Debug.Log($"end:{taskStatus}");
    }

    public override void OnAwake()
    {
        base.OnAwake();
    }
     
    public override void OnBehaviorComplete()
    {
        base.OnBehaviorComplete();  
    }

    public override void OnEnd()
    {
        base.OnEnd();  
    }

    private int2 offsetCoordinate = int2.zero;
    private float startTime;
    public override void OnStart()
    {
        startTime = Time.time;
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
         character = CharacterManager.instance.GetCharacter(characterId.Value);
        if (character != null)
        {
            if (character.mapInstance == targetCoordinate.z &&
                    character.coordinate.x == targetCoordinate.x && character.coordinate.y == targetCoordinate.y)
            {
                taskStatus = TaskStatus.Success;
            }
            else
            {
                if (!character.TryMove(targetCoordinate.z, targetCoordinate.xy, MoveEndAction,failedMoveAction:FailedMoveAction))
                {
                    Debug.Log(
                        $"{character.name}character.mapInstance {character.mapInstance}-character.coordinate{character.coordinate}-targetCoordinate.z{targetCoordinate.z}targetCoordinate.xy{targetCoordinate.xy}-MoveCrossMapFailure");
                     taskStatus = TaskStatus.Failure;
                }
            }
        }
        else
        {
            taskStatus = TaskStatus.Failure;
        }
    }

    private void FailedMoveAction(int3 value,int count=0)
    {
        if (smartMove)
        {
          
            //var character = CharacterManager.instance.GetCharacter(characterId.Value);
            if (!character.TryMove(target.Value.z, target.Value.xy, MoveEndAction))
            {
                taskStatus = TaskStatus.Failure;
                Debug.Log($"faile;value{value}");
            }
        }
        else
        {
            taskStatus = TaskStatus.Failure;
        }
    }

    public override TaskStatus OnUpdate()
    {
        if (waitDuration > 0)
        {
            if (startTime + waitDuration < Time.time)
            {
                Debug.Log($"行为树移动失败--等待时间太长");
                return TaskStatus.Failure;
            }

        }
       
        //return TaskStatus.Success;
        return taskStatus;
    }
}