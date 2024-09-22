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
    [SerializeField]
    SharedSkillList sharedSkillList;
    [SerializeField]
    SharedInt fightCharacter;
    SkillEstimateData selectSkill;
    public override void OnStart()
	{
       
    }

    public override void OnAwake()
    {
        base.OnAwake();
        if (sharedSkillList == null)
            sharedSkillList = (SharedSkillList)Owner.GetVariable("ReadySkill");
        if (fightCharacter==null|| fightCharacter.IsNull())
            fightCharacter = (SharedInt)Owner.GetVariable("fightCharacter");

        selectSkill = null;
    }

    public override float GetUtility()
    {  
        float UtilityValue = 0;
        if (sharedSkillList.Value.TryGetValue(fightType, out var skills))
        {
            for(int i = 0; i < skills.Count; i++)
            {
                var skill = skills[i];
                float Value=GetSkillUtilityValue(skill);
                if (Value >= UtilityValue)
                {
                    UtilityValue = Value;
                }
            }
        }
        return UtilityValue; 
    }

    float GetSkillUtilityValue(int skill)
    {
        float UtilityValue = 0;
        var skillEstimateDatas = FightManager.instance.EstimateSkills(skill, fightCharacter.Value);
        for (int j = 0; j < skillEstimateDatas.Count; j++)
        {
            var skillEstimateData = skillEstimateDatas[j];
            float nowValue = skillEstimateData.utlilityValue;
            nowValue = skillEstimateData.utlilityValue * (1 + GameRandom.RandomFloat(-randomValue, randomValue)) + addValue;
            nowValue *= estimateValue;

            if (UtilityValue <= nowValue)
            {
                UtilityValue = nowValue;
                selectSkill = skillEstimateData;
            }
        }
        return UtilityValue;
    }
    public override TaskStatus OnUpdate()
	{
        if (selectSkill==null||selectSkill.targets==null||selectSkill.targets.Count == 0)
        {
            return TaskStatus.Failure;
        }

        FightController.instance.StartSkillAction(selectSkill, fightCharacter.Value);
		return TaskStatus.Success;
	}
}