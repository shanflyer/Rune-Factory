using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
[TaskCategory("Game/PlayerStore")]
[TaskName("选择玩家柜台")]
[TaskIcon("{SkinColor}SelectorIcon.png")]
public class SelectPlayerStoreCounter : Action
{
    public bool canSelectNullStoreCounter;
    public SharedInt selectStoreCounterId;
	public override void OnStart()
	{
        if (selectStoreCounterId == null)
        {
            selectStoreCounterId = (SharedInt)Owner.GetVariable("SelectStoreCounterId");
            if (selectStoreCounterId == null)
            {
                selectStoreCounterId = new SharedInt();
                Owner.SetVariable("TargetCoordinate", selectStoreCounterId);
            }
        }
		SelectStoreCounter();
    }
	TaskStatus taskStatus;
    void SelectStoreCounter()
	{
		List<RuntimeStoreCounter> HaveGoodStoreCounters = new List<RuntimeStoreCounter>();
		List<RuntimeStoreCounter> NoGoodStoreCounters = new List<RuntimeStoreCounter>();

		var RuntimeStoreCounters = PlayerStoreManager.instance.RuntimeStoreCounters;
		foreach(RuntimeStoreCounter RuntimeStoreCounter in RuntimeStoreCounters)
		{
			if(RuntimeStoreCounter.count == 0)
			{
				NoGoodStoreCounters.Add(RuntimeStoreCounter);
			}
			else
			{
				HaveGoodStoreCounters.Add(RuntimeStoreCounter);
			}
		}
		if (HaveGoodStoreCounters.Count > 0)
		{
			int index = GameRandom.RandomInt(0, HaveGoodStoreCounters.Count);
			selectStoreCounterId = HaveGoodStoreCounters[index].instanceId;
			taskStatus = TaskStatus.Success;
			return;
		} 
		if (canSelectNullStoreCounter&&NoGoodStoreCounters.Count > 0)
		{
            int index = GameRandom.RandomInt(0, NoGoodStoreCounters.Count);
            selectStoreCounterId = NoGoodStoreCounters[index].instanceId;
            taskStatus = TaskStatus.Success;
            return;
        }
		taskStatus = TaskStatus.Failure;
    }

    public override TaskStatus OnUpdate()
	{
		return taskStatus;
	}
}