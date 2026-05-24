using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public enum CheckItemProperty
{
    ID, TYPE, REFRESH
}

[TaskCategory("NewGame/Common")]
[TaskName("检查选中的道具")]
public class CheckSelectItem : Action
{
    public SharedInt characterId;
    public CheckItemProperty checkItemProperty;
    public SharedInt checkValue;
    public bool checkCount;
    public SharedInt itemCount;

    public SharedInt outSelectItem;
    public SharedInt outSelectItemInstance;

    private ItemData itemData;
    private Character character;
    private Item item;
    public override void OnStart()
    {
        AsyncTaskRunner.Run(OnStartAsync, nameof(CheckSelectItem));
    }

    private async System.Threading.Tasks.Task OnStartAsync()
    {
        taskStatus = TaskStatus.Running;
        character = CharacterManager.instance.controllerCharacter;
        var package = character.characterPackage;
        if (!characterId.IsNull())
        {
            character = CharacterManager.instance.GetCharacter(characterId.Value);
        }
        if (character != null)
        {
            package = character.characterPackage;
            Item item = PackageManager.instance.GetPackageSelectItem(package);
            itemData = await GameDataManager.instance.GetAsyncData<ItemData>(item.dataId);
            outSelectItem.SetValue(item.dataId);
            outSelectItemInstance.SetValue(item.instanceId);

            if (itemData != null)
            {
                switch (checkItemProperty)
                {
                    case CheckItemProperty.ID:
                        if (itemData.id == checkValue.Value)
                        {
                            if (checkCount)
                            {
                                if (item.count >= checkValue.Value)
                                {
                                    taskStatus=TaskStatus.Success;
                                    return;
                                }
                                else
                                {
                                    taskStatus = TaskStatus.Failure;
                                    return;
                                }
                            }
                            taskStatus = TaskStatus.Success;
                            return;
                        }
                        break;

                    case CheckItemProperty.TYPE:
                        if ((int)itemData.type == checkValue.Value)
                        {
                            if (checkCount)
                            {
                                if (item.count >= checkValue.Value)
                                {
                                    taskStatus = TaskStatus.Success;
                                    return;
                                }
                                else
                                {
                                    taskStatus = TaskStatus.Failure;
                                    return;
                                }
                            }
                            taskStatus = TaskStatus.Success;
                            return;
                        }
                        break;

                    case CheckItemProperty.REFRESH:
                        if (itemData.isFresh ? checkValue.Value == 1 : checkValue.Value == 0)
                        {
                            if (checkCount)
                            {
                                if (item.count >= checkValue.Value)
                                {
                                    taskStatus = TaskStatus.Success;
                                    return;
                                }
                                else
                                {
                                    taskStatus = TaskStatus.Failure;
                                    return;
                                }
                            }
                            taskStatus = TaskStatus.Success;
                            return;
                        }
                        break;
                }
            }
            taskStatus = TaskStatus.Failure;
        }
        else
        {
            taskStatus = TaskStatus.Failure;
        }

    }
    TaskStatus taskStatus;
    public override TaskStatus OnUpdate()
    {
        return taskStatus;
    }
}
