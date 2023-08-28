using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System;
using Action = BehaviorDesigner.Runtime.Tasks.Action;

[TaskCategory("NewGame")]
[TaskName("获取可用技能")]
public class GetAllReadySkill : Action
{
    SharedSkillList sharedSkillList;
    SharedInt agentId;
    BehaviorTree behaviorTree;
    public override void OnStart()
    {
        if (behaviorTree == null)
        {
            behaviorTree = GetComponent<BehaviorTree>();
        }
        if (behaviorTree != null)
        {
            try
            {
                sharedSkillList = (SharedSkillList)behaviorTree.GetVariable("ReadySkill");
                agentId = (SharedInt)behaviorTree.GetVariable("AgentId");
            }
            catch(Exception e)
            {
                Debug.LogError(e.Message);
            }
            
        }
        
    }

    public override TaskStatus OnUpdate()
    {
        if (sharedSkillList == null || agentId == null)
        {
            return TaskStatus.Failure;
        }
       var skills=  FightManager.instance.GetReadySkills(agentId.Value);
        sharedSkillList.SetValue(skills);

        return TaskStatus.Success;
    }
}
