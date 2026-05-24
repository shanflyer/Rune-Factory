using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;


[TaskCategory("NewGame/Common")]
[TaskName("检查快捷栏道具值")]
public class CheckShortcutItemValue : Action
{
    public SharedInt characterId;
    public SharedInt checkValue;
    public SharedIntList checkItem;

    public override TaskStatus OnUpdate()
    {

        ShortcutPackage shortcutPackage = ShortcutManager.instance.GetShortcutPackage(characterId.Value);
        var items = shortcutPackage.items;

        for (int i = 0; i < items.Length; i++)
        {
            if (checkItem.Value.Contains(items[i].dataId))
            {
                if (items[i].value >= checkValue.Value)
                {
                    return TaskStatus.Success;
                }
            }

        }
        return TaskStatus.Failure;
    }
}
