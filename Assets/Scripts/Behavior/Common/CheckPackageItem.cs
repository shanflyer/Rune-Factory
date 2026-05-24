using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
[TaskCategory("NewGame/Common")]
[TaskName("检查背包道具")]
public class CheckPackageItem : Action
{
	public SharedInt characterId;
    public SharedInt packageId;
	public SharedInt itemId;
	public SharedInt itemCount;


	public override void OnStart()
	{

	}

	public override TaskStatus OnUpdate()
	{
		int package = packageId.Value;
		if (!characterId.IsNull())
		{
			Character character = CharacterManager.instance.GetCharacter(characterId.Value);
			if (character != null)
			{
				package = character.characterPackage;
			}
        }

        int count = PackageManager.instance.GetPackageItemCount(package, itemId.Value);
        if (count >= itemCount.Value)
        {
			return TaskStatus.Success;
		}
		return TaskStatus.Failure;

	}
}
