using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Collections.Generic;

[TaskCategory("NewGame/Common")]
[TaskName("检查快捷栏的道具")]
public class CheckShortcutItem : Action
{
    public SharedInt characterId; 
    public SharedIntList checkValue;
    private SharedInt resultItem;
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
            for(int i = 0; i < checkValue.Value.Count; i++)
            {
                if (shortcutPackage.CheckItem(checkValue.Value[i],out List<int> item))
                {
                    resultItem.Value = item[0];
                    return TaskStatus.Success;
                }
            }
           
        }
        return TaskStatus.Failure;
    }
}