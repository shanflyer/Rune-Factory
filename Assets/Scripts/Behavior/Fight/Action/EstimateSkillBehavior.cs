using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
[TaskCategory("NewGame")]
[TaskName("评估选择技能行为")]
public class EstimateSkillBehavior : Action
{
    public FightType fightType;
    public float estimateValue=1;
    public float randomValue = 0;
    SharedSkillList sharedSkillList;

    SkillEstimateData selectSkill;
    public override void OnStart()
	{ 
	}
    public override float GetUtility()
    {
        if (sharedSkillList.Value.TryGetValue(fightType, out var skills))
        {

        }
        return base.GetUtility();
    }

    public override TaskStatus OnUpdate()
	{
		return TaskStatus.Success;
	}
}