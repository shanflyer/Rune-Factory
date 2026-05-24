using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
[TaskCategory("NewGame")]
[TaskName("战斗行为选择")]
public class FightTypeSelect : Action
{
    public override float GetUtility()
    {
        return base.GetUtility();
    }
    public override void OnStart()
	{

	}

	public override TaskStatus OnUpdate()
	{
		return TaskStatus.Success;
	}
}
