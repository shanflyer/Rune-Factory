using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using static UnityEngine.Rendering.ReloadAttribute;

[TaskCategory("Game/Character")]
[TaskName("为角色减少道具")]
public class CharacterRemoveItem : Action
{
	public SharedInt characterId;
	public SharedRandomResults randomResults;
	// Start is called before the first frame update
	public override void OnStart()
	{
		if (randomResults != null)
		{
			int package = 0;
            Character character = CharacterManager.instance.GetCharacter(characterId.Value);
            if (character != null)
            {
                package = character.characterPackage;

                var results = randomResults.Value;
                for (int i = 0; i < results.Count; i++)
                {
                    try
                    {
                        RemovePackageItem addPackageItem = new RemovePackageItem
                        {
                            packageId = package,
                            itemDataId = results[i].x,
                            itemCount = results[i].y
                        };
                        GameActionManager.instance.QueueAction(addPackageItem, true);
                    }
                    catch (System.Exception e)
                    {
                        Debug.Log(e.ToString());
                    }

                }
            }
         
		}
	}

	public override TaskStatus OnUpdate()
	{
		return TaskStatus.Success;

	}
}
