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
    public override async void OnStart()
    {
        character = CharacterManager.instance.controllerCharacter;
        var package = character.characterPackage;
        if (!characterId.IsNull())
        {
            character = CharacterManager.instance.GetCharacter(characterId.Value);
            if (character != null)
            {
                package = character.characterPackage;
                Item item = PackageManager.instance.GetPackageSelectItem(package);
                itemData = await GameDataManager.instance.GetAsyncData<ItemData>(item.dataId);
                outSelectItem.SetValue(item.dataId);
                outSelectItemInstance.SetValue(item.instanceId);
            }
        }
    }

    public override TaskStatus OnUpdate()
    {
        if (character != null)
        {
            var package = character.characterPackage; 
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
                                    return TaskStatus.Success;
                                }
                                else
                                {
                                    return TaskStatus.Failure;
                                }
                            }
                            return TaskStatus.Success;
                        }
                        break;

                    case CheckItemProperty.TYPE:
                        if ((int)itemData.type == checkValue.Value)
                        {
                            if (checkCount)
                            { 
                                if (item.count >= checkValue.Value)
                                {
                                    return TaskStatus.Success;
                                }
                                else
                                {
                                    return TaskStatus.Failure;
                                }
                            }
                            return TaskStatus.Success;
                        }
                        break;

                    case CheckItemProperty.REFRESH:
                        if (itemData.isFresh ? checkValue.Value == 1 : checkValue.Value == 0)
                        {
                            if (checkCount)
                            { 
                                if (item.count >= checkValue.Value)
                                {
                                    return TaskStatus.Success;
                                }
                                else
                                {
                                    return TaskStatus.Failure;
                                }
                            }
                            return TaskStatus.Success;
                        }
                        break;
                }
            }
        }
        return TaskStatus.Failure;
    }
}