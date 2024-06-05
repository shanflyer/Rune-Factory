using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SkillManager : Singleton<SkillManager>
{
    MyInstance myInstance = new MyInstance();
    public override void Init()
    {
        base.Init();
    }
    protected override void Clear()
    {
        base.Clear();
    }
    public async Task<SkillRuntime> CreatSkillRuntime(int skillId)
    {
        SkillData skillData=await GameDataManager.instance.GetAsyncData<SkillData>(skillId);
        SkillRuntime skillRuntime = new SkillRuntime
        {
            instanceId = myInstance.CreatInstanceId(),
            skillData = skillData,
            cost = skillData.cost,
            skillCd = skillData.cd
        };
        return skillRuntime;
    } 
}
public class SkillRuntime
{
    public int instanceId;
    public SkillData skillData;
    public int skillCd;
    public int cost;
    public FightType fightType=>skillData.fightType;
     
    public bool waiteCD=>skillCd>0;

    public void SetSkillCd(float speed)
    {
        if (speed < 100)
        {
            float value =1+ (100 - speed) *0.01f;
            skillCd = (int)(skillCd * value);
        }
        else
        {
            float value = 1/(speed - 100) * 0.01f;
            skillCd = (int)(skillCd * value);
        }
    }
    public void UpData(int timeValue)
    {
        skillCd -= timeValue;
        if (skillCd < 0)
        {
            skillCd = 0;
        }
    }

}