using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
[TaskCategory("NewGame/回合/事件")]
[TaskName("本回合战斗角色初始化")]
public class RoundInit : Action
{
    [SerializeField]
    SharedQueneInt fightCharacters;
	public override void OnStart()
	{
		Debug.Log("角色初始化");
		if (fightCharacters == null)
		{
			fightCharacters = (SharedQueneInt)Owner.GetVariable("fightCharacters");
		}
        fightCharacters.Value=FightManager.instance.InitFightCharacter();
	}

	public override TaskStatus OnUpdate()
	{
		return TaskStatus.Success;
	}
}