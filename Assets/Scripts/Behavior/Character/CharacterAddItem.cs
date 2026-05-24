using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;


[TaskCategory("Game/Character")]
[TaskName("为角色增加道具")]
public class CharacterAddItem : Action
{
	public SharedInt packageId;
	public SharedRandomResults randomResults;
	public override void OnStart()
	{
        if (randomResults != null)
        {
			var results = randomResults.Value;
			for(int i = 0; i < results.Count; i++)
            {
                try
                {
					AddPackageItem addPackageItem = new AddPackageItem
					{
						packageId = packageId.Value,
						itemDataId = results[i].x,
						itemCount = results[i].y
					};
					GameActionManager.instance.QueueAction(addPackageItem, true);
				}
                catch(System.Exception e)
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
