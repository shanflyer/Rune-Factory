using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
[TaskCategory("NewGame/回合/事件")]
[TaskName("本回合战斗角色初始化")]
public class RoundInit : Action
{
	ShardQueneInt fightCharacters;
	public override void OnStart()
	{
		if (fightCharacters == null)
		{
			fightCharacters = (ShardQueneInt)Owner.GetVariable("fightCharacters");
		}
        fightCharacters.Value=FightManager.instance.InitFightCharacter();
	}

	public override TaskStatus OnUpdate()
	{
		return TaskStatus.Success;
	}
}