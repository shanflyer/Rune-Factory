using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using static UnityEngine.Rendering.ReloadAttribute;

[TaskCategory("Game/农场")]
[TaskName("尝试种植")]
public class TryCreatPlantBeHavior : Action 
{
    public SharedInt characterId; 
    public SharedInt fieldId;
    public SharedInt seedId;

    public override void OnStart()
    {
        AsyncTaskRunner.Run(OnStartAsync, nameof(TryCreatPlantBeHavior));
    }

    private async System.Threading.Tasks.Task OnStartAsync()
    {
        taskStatus = TaskStatus.Running;

        int package = 0;
        if (!characterId.IsNull())
        {
            Character character = CharacterManager.instance.GetCharacter(characterId.Value);
            if (character != null)
            {
                package = character.characterPackage;
            }
        }
        ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(seedId.Value);
        if (itemData == null)
        {
            taskStatus = TaskStatus.Failure;
        }
        else
        { 
            TryCreatPlant tryCreatPlant = new TryCreatPlant
            {
                fieldId = fieldId.Value,
                plantId = itemData.typeValue,
                setResult = CheckState
            };
            GameActionManager.instance.QueueAction(tryCreatPlant);
        }

        
    }
    void CheckState(bool value)
    {
        if (value)
        {
            taskStatus = TaskStatus.Success;
            return;
        }
        taskStatus = TaskStatus.Failure;
    }
    TaskStatus taskStatus;
    public override TaskStatus OnUpdate()
    {
        return taskStatus;
    }
}
