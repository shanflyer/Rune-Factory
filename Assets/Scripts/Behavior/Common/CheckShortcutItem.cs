using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Collections.Generic;

[TaskCategory("NewGame/Common")]
[TaskName("检查快捷栏的道具")]
public class CheckShortcutItem : Action
{
    public SharedInt characterId; 
    public SharedInt checkValue; 
    public  SharedIntList outItemInstance;
    private Character character;
    private ShortcutPackage shortcutPackage;
    public override async void OnStart()
    {
        character = CharacterManager.instance.controllerCharacter;
        
        if (!characterId.IsNull())
        {
            character = CharacterManager.instance.GetCharacter(characterId.Value);
            if (character != null)
            {
                shortcutPackage = ShortcutManager.instance.GetShortcutPackage(character.instanceId); 
                 
            }
        }
    }

    public override TaskStatus OnUpdate()
    {
        if (shortcutPackage!=null)
        {
            List<int> itemInstance = new List<int>();
            if (shortcutPackage.CheckItem(checkValue.Value,out itemInstance))
            {
                outItemInstance.SetValue(itemInstance);
                return TaskStatus.Success;
            }
            else
            {
                return TaskStatus.Failure;
            }
        }
        return TaskStatus.Failure;
    }
}