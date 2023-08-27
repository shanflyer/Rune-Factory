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
public struct SkillRuntime
{
    public int instanceId;
    public SkillData skillData;
    public int skillCd;
    public int cost;
    public FightType fightType;
   
}