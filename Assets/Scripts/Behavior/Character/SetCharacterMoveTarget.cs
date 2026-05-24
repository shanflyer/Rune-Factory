using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

[TaskCategory("Game/Character")]
[TaskName("设置角色移动目标")]
public class SetCharacterMoveTarget : Action
{
    [Header("获取的结果")]
    public SharedInt3List results;

    public SharedInt3 targetCoordinate;
    private SharedInt characterId;
    public override void OnStart()
    {
        if (targetCoordinate == null || targetCoordinate.IsNull())
        {
            targetCoordinate = (SharedInt3)Owner.GetVariable("TargetCoordinate");
            if (targetCoordinate == null || targetCoordinate.IsNull())
            {
                targetCoordinate = new SharedInt3();
                Owner.SetVariable("TargetCoordinate", targetCoordinate);
            }
        }
        if (characterId == null || characterId.IsNull())
        {
            characterId = (SharedInt)Owner.GetVariable("CharacterId");
        }
        if (results == null)
        {
            results = (SharedInt3List)Owner.GetVariable("CoordinateResults");
            if (results == null)
            {
                results = new SharedInt3List();
                Owner.SetVariable("CoordinateResults", results);
            }
        }
        try
        {
            int index = GameRandom.RandomInt(0, results.Value.Count);
            targetCoordinate.SetValue(results.Value[index]);
        }
        catch(System.Exception e)
        {
            Character character = CharacterManager.instance.GetCharacter(characterId.Value);
            Debug.LogError($"{character.name}--behaviorTree:{Owner.ExternalBehavior.name}--{Owner.BehaviorName}-{e}");
        }

    }

    public override TaskStatus OnUpdate()
    {
        return TaskStatus.Success;
    }
}
