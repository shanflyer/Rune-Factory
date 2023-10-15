using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Game/Character")]
[TaskName("检查角色位置")]
[TaskIcon("{SkinColor}SelectorIcon.png")]
public class CheckCharacterCoordinate : Action
{
    public SharedInt characterId;
    public SharedObjCoordinate coordinate; 
    public bool continued;

    Character character; 
    private ObjCoordinate oldCoordinate;

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
                if (character.ObjCoordinate==coordinate.Value)
                {
                    return TaskStatus.Success;
                }
            }
            else
            { 
                if (character.ObjCoordinate != oldCoordinate)
                {
                    if (character.ObjCoordinate == coordinate.Value)
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