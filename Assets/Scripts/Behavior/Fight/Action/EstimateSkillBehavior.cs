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
    SharedInt fightCharacter;
    SkillEstimateData selectSkill;
    public override void OnStart()
	{ 
	}
    public override float GetUtility()
    {
        float UtilityValue = 0;
        if (sharedSkillList.Value.TryGetValue(fightType, out var skills))
        {
            for(int i = 0; i < skills.Count; i++)
            {
                var skill = skills[i];
                SkillEstimateData skillEstimateData = FightManager.instance.EstimateSkill(skill, fightCharacter.Value);
                if (UtilityValue < skillEstimateData.utlilityValue)
                {
                    UtilityValue = skillEstimateData.utlilityValue;
                    selectSkill = skillEstimateData;
                }
            }
        }
        return UtilityValue; 
    }

    public override TaskStatus OnUpdate()
	{
		return TaskStatus.Success;
	}
}