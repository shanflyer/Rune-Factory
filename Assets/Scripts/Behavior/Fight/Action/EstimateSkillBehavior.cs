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
    public float addValue = 0;
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
                float nowValue = skillEstimateData.utlilityValue;
                nowValue= skillEstimateData.utlilityValue * (1 + GameRandom.RandomFloat(-randomValue, randomValue))+addValue;
                nowValue *= estimateValue;


                if (UtilityValue < nowValue)
                {
                    UtilityValue = nowValue;
                    selectSkill = skillEstimateData;
                }
            }
        }
        return UtilityValue; 
    }

    public override TaskStatus OnUpdate()
	{
        FightController.instance.StartSkillAction(selectSkill, fightCharacter.Value);
		return TaskStatus.Success;
	}
}