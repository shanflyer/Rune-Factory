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

    FightCharacter fightCharacter;
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
        if(fightCharacter == null||fightCharacter.fightStatus==FightStatus.准备)
        {
            if (fightCharacters.Value.Count > 0)
            {
                int characterId = fightCharacters.Value.Dequeue();
                if (FightManager.instance.GetFightCharacter(characterId,out fightCharacter))
                { 
                    nowFightCharacter.Value = characterId;
                    FightManager.instance.cdTimeMoving = false; 
                    FightController.instance.RunFightCharacter(characterId);
                } 
            }
            else
            {
                FightManager.instance.cdTimeMoving = true;
                return TaskStatus.Failure;
            }
        }

        return TaskStatus.Running; 
         
    }
}