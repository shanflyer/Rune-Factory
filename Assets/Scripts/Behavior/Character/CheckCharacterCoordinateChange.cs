using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Game/Character")]
[TaskName("检查角色位置改变")]
[TaskIcon("{SkinColor}SelectorIcon.png")]
public class CheckCharacterCoordinateChange:Action
{
    public SharedInt characterId; 
    public SharedInt3 coordinate;
    public bool continued;

    private bool addAction = false;
    private bool result = false;

    private void CharacterCoordinateTriggerAction(CharacterCoordinateTrigger characterCoordinateTrigger)
    {
        if (characterId.Value == characterCoordinateTrigger.characterId&&
            coordinate.Equals(characterCoordinateTrigger.coordinate))
        {
            result = true;
        }
        result = false;
    }
    public override void OnStart()
    {
        if (!addAction)
        {
            GameActionManager.instance.AddListener<CharacterCoordinateTrigger>(CharacterCoordinateTriggerAction);
            addAction = false;
        }
    }
    public override TaskStatus OnUpdate()
    {
        if (result)
        {
            return TaskStatus.Success;
        }
        if (!continued)
        {
            result = false;
        }
        return TaskStatus.Failure;
    }
}