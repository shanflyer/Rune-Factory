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
    SharedSkillList sharedSkillList;
    SharedInt agentId;
    BehaviorTree behaviorTree;
    public override void OnStart()
    {
        try
        {
            sharedSkillList = (SharedSkillList)Owner.GetVariable("ReadySkill");
            agentId = (SharedInt)Owner.GetVariable("AgentId");
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }

    }

    public override TaskStatus OnUpdate()
    {
        if (sharedSkillList == null || agentId == null)
        {
            return TaskStatus.Failure;
        }
       var skills=  FightManager.instance.GetReadySkills(agentId.Value,fightType);
        sharedSkillList.SetValue(skills);

        return TaskStatus.Success;
    }
}
