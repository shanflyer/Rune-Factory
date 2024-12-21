using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;


[TaskCategory("NewGame/Common")]
[TaskName("改变快捷栏道具值")]
public class ChangeShortcutItemValue : Action
{
    public SharedInt characterId;
    public SharedInt value;
    public SharedInt item;

    public override TaskStatus OnUpdate()
    { 
        ShortcutPackage shortcutPackage = ShortcutManager.instance.GetShortcutPackage(characterId.Value);
        shortcutPackage.TryChangeItemValue(item.Value, item.Value);
        return TaskStatus.Failure;
    }
}