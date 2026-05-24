using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
[TaskCategory("Game/Map")]
[TaskName("改变物体链接数据和动画状态")]
public class MapItemPlayAnimationEvent : Action
{
    public SharedInt targetId;
	public SharedInt keyX;
    public SharedInt keyY;
    public override void OnStart()
	{
		//SetA
	}

	public override TaskStatus OnUpdate()
	{
		return TaskStatus.Success;
	}
}
