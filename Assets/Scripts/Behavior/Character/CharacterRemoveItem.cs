using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
[TaskCategory("Game/Character")]
[TaskName("为角色减少道具")]
public class CharacterRemoveItem : Action
{
	public SharedInt packageId;
	public SharedRandomResults randomResults;
	// Start is called before the first frame update
	public override void OnStart()
	{
		if (randomResults != null)
		{
			var results = randomResults.Value;
			for (int i = 0; i < results.Count; i++)
			{
				try
				{
					RemovePackageItem addPackageItem = new RemovePackageItem
					{
						packageId = packageId.Value,
						itemDataId = int.Parse(results[i].result),
						itemCount = results[i].count
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

	public override TaskStatus OnUpdate()
	{
		return TaskStatus.Success;

	}
}
