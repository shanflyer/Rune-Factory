using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System;
using Action = BehaviorDesigner.Runtime.Tasks.Action;

[TaskCategory("NewGame")]
[TaskName("获取可用技能")]
public class GetAllReadySkill : Action
{
    public FightType fightType;
    [SerializeField]
    SharedSkillList sharedSkillList;
    [SerializeField]
    SharedInt fightCharacter;
    public override void OnStart()
    {
        if (sharedSkillList == null)
            sharedSkillList = (SharedSkillList)Owner.GetVariable("ReadySkill");
        if (fightCharacter == null)
            fightCharacter = (SharedInt)Owner.GetVariable("fightCharacter");

        var skills = FightManager.instance.GetReadySkills(fightCharacter.Value, fightType);
        sharedSkillList.SetValue(skills);
    }

    public override TaskStatus OnUpdate()
    { 

        return TaskStatus.Success;
    }
}
