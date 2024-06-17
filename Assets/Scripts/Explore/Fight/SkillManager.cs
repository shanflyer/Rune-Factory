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
        SkillRuntime skillRuntime = new SkillRuntime(skillData, myInstance.CreatInstanceId()); 
        return skillRuntime;
    }

  
}
public class SkillRuntime
{
    public int instanceId;
    public SkillData skillData;  
    private int skillCd;

    public SkillRuntime(SkillData skillData,int instanceId)
    {
        this.skillData = skillData;
        this.instanceId = instanceId;
        skillCd = skillData.cd;
    }
    public FightType fightType=>skillData.fightType;
     
    public bool waiteCDEnd=> skillCd<=0;

    public float GetTimeValue()
    {
        return skillCd / (float)skillData.cd;
    }
    
 
    public void Reset()
    {
        skillCd = skillData.cd;
    }
    public void UpData(int timeValue)
    { 
        if (skillCd>0)
        {
            skillCd -= 1 ;
        }
    }

}

public class BuffRuntime
{
    public int instanceId;
    public BuffData buffData;
    private int nowActionIndex;
    public BuffRuntime(int instanceId, BuffData buffData)
    {
        this.instanceId = instanceId;
        this.buffData = buffData; 
    }
    public void BuffAction()
    {
        nowActionIndex++;
    }
}