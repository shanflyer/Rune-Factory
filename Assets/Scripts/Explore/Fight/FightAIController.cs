using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum FightType
{
    攻击,回复,复活,待机,增益,负面,移除负面
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