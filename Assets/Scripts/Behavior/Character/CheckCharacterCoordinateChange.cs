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
        else if (!continued)
        {
            result = false;
        }
    }

    public override void OnBehaviorComplete()
    {
        base.OnBehaviorComplete();
        RemoveListener();
    }

    public override void OnEnd()
    {
        base.OnEnd();
        RemoveListener();
    }

    private void RemoveListener()
    {
        if (!SingletonType.Cleared)
        {
            // 行为树节点结束时主动解绑，避免下一次 OnStart 叠加监听。
            GameActionManager.instance.RemoveListener<CharacterCoordinateTrigger>(CharacterCoordinateTriggerAction);
        }
        addAction = false;
    }

    public override void OnStart()
    {
        if (!addAction)
        {
            GameActionManager.instance.AddListener<CharacterCoordinateTrigger>(CharacterCoordinateTriggerAction);
            addAction = true;
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
