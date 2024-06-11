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
    private float speed=100.0f;
    private int waitTime;
    private int skillCd;

    public SkillRuntime(SkillData skillData,int instanceId)
    {
        this.skillData = skillData;
        this.instanceId = instanceId;
        InitCd();
    }
    public FightType fightType=>skillData.fightType;
     
    public bool waiteCDEnd=> waitTime>=skillCd;

    public float GetTimeValue()
    {
        return waitTime/ (float)skillCd;
    }
    public void SetSkillCd(float speed)
    {
        this.speed = speed; 
        InitCd();
    }
   
    void InitCd()
    {
        if (speed < 100)
        {
            float value = 1 + (100 - speed) * 0.01f;
            skillCd = (int)(skillData.cd * value);
        }
        else
        {
            float value = 100.0f / speed;
            skillCd = (int)(skillData.cd * value);
        }
    }
    public void Reset()
    {
        waitTime = 0;
    }
    public void UpData(int timeValue)
    { 
        if (waitTime<skillCd)
        {
            waitTime += timeValue;
        }
    }

}