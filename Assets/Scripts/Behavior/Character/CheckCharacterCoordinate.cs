using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.Mathematics;

[TaskCategory("Game/Character")]
[TaskName("检查角色位置")]
[TaskIcon("{SkinColor}SelectorIcon.png")]
public class CheckCharacterCoordinate : Action
{
    public SharedInt characterId;
    public SharedInt3 coordinate; 
    public bool continued;
    Character character; 
    private int3 oldCoordinate;

    public override void OnStart()
    {
        if (character == null)
        {
            character = CharacterManager.instance.GetCharacter(characterId.Value);
        }
    }
    public override TaskStatus OnUpdate()
    {
        if (character != null)
        { 
            if (continued)
            { 
                if (character.ObjCoordinate.Equals(coordinate.Value))
                {
                    return TaskStatus.Success;
                }
            }
            else
            { 
                if (!character.ObjCoordinate.Equals(oldCoordinate))
                {
                    if (character.ObjCoordinate.Equals(coordinate.Value))
                    {
                        return TaskStatus.Success;
                    }
                    oldCoordinate = character.ObjCoordinate;
                }
            }

           
        }
        return TaskStatus.Failure;
    }
}