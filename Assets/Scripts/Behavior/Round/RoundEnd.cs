using System.Collections;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
[TaskCategory("NewGame/回合/事件")]
[TaskName("回合结束")]
public class RoundEnd : Action
{
    [SerializeField]
    SharedInt nowFightCharacter;

    public override void OnStart()
    {
       // Debug.Log("RoundEnd");
        if (nowFightCharacter==null|| nowFightCharacter.IsNull())
        {
            nowFightCharacter = (SharedInt)Owner.GetVariable("nowFightCharacter");
        }
        nowFightCharacter.Value = 0;
        GameActionManager.instance.QueueAction(new StartRoundFight(),true);
    }

    public override TaskStatus OnUpdate()
    {
        return TaskStatus.Success;
    }
}
