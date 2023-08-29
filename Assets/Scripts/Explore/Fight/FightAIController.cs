using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum FightType
{
   All, 攻击,回复,复活,待机,增益,负面,移除负面
}
public enum TargetType
{
    敌方, 我方, 自身
}
public class FightAIController :Singleton<FightAIController>
{
    public override void Init()
    {
        base.Init();
    }

    public List<int> GetNowReadySkills(int characterid)
    {
        List<int> readySkills = new List<int>();
        return readySkills;
    }

   
}
[System.Serializable]
public struct SkillEstimateData
{
    public int skillId;
    public List<List<int>> target;
    public float utlilityValue;

    public void SetUtlility(SkillData skillData)
    {
        switch (skillData.skillActionType)
        {
            case SkillActionType.伤害:
                for (int i = 0; i < target.Count; i++)
                {
                    for(int j=0; j < target[i].Count; j++)
                    {
                        int t = target[i][j];


                    }
                } 
                break;
        }
    }
}