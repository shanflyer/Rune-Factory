using System.Collections;
using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;

[TaskCategory("NewGame/回合/事件")]
[TaskName("选择角色进行战斗")]
public class RoundStartFight : Action
{
    [SerializeField]
    SharedQueneInt fightCharacters;
    [SerializeField]
    SharedInt nowFightCharacter;
    public override void OnStart()
    {
        if (fightCharacters == null)
        {
            fightCharacters = (SharedQueneInt)Owner.GetVariable("fightCharacters");
        }
        if (nowFightCharacter==null|| nowFightCharacter.IsNull())
        {
            nowFightCharacter=(SharedInt)Owner.GetVariable("nowFightCharacter");
        }
    }

    public override TaskStatus OnUpdate()
    {
        Debug.Log("RoundStartFight");
        if(fightCharacters.Value.Count>0)
        {
            int characterId = fightCharacters.Value.Dequeue();
            nowFightCharacter.Value = characterId;
            FightController.instance.RunFightCharacter(characterId);
            return TaskStatus.Success;
        }
        return TaskStatus.Failure;
    }
}