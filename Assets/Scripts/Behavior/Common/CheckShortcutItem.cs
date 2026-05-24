using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Collections.Generic;

[TaskCategory("NewGame/Common")]
[TaskName("检查快捷栏的道具")]
public class CheckShortcutItem : Action
{
    [SerializeField]
    public SharedInt characterId; 
    public SharedIntList checkValue;
    [SerializeField]
    private SharedInt resultItem;
    private Character character;
    private ShortcutPackage shortcutPackage;
    public override void OnStart()
    {
        AsyncTaskRunner.Run(OnStartAsync, nameof(CheckShortcutItem));
    }

    private System.Threading.Tasks.Task OnStartAsync()
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

        // 保留 Task 签名给异步调度器使用，同步路径显式完成，避免 Unity CS1998 警告。
        return System.Threading.Tasks.Task.CompletedTask;
    }

    public override TaskStatus OnUpdate()
    {
        if (shortcutPackage!=null)
        { 
            for(int i = 0; i < checkValue.Value.Count; i++)
            {
                if (shortcutPackage.CheckItem(checkValue.Value[i],out List<int> item))
                {
                    if (resultItem != null)
                        resultItem.Value = item[0];
                    return TaskStatus.Success;
                }
            }
           
        }
        return TaskStatus.Failure;
    }
}
