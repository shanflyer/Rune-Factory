using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class SkillManager : Singleton<SkillManager>
{
    MyInstance myInstance = new MyInstance();
    SkillRuntime useItemRuntime;
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

    public async Task<BuffRuntime> CreatBuffRuntime(int buffId,FightCharacter fightCharacter)
    {
        BuffData buffData = await GameDataManager.instance.GetAsyncData<BuffData>(buffId);
        int randomValue = GameRandom.RandomInt(0, 100);
        randomValue = FightManager.instance.GetAttributeTypeRandomValue(buffData.attributeType, fightCharacter.AttackAttributeType, randomValue);
        if (randomValue < buffData.probability)
        {
            BuffRuntime buffRuntime = new BuffRuntime(buffData, myInstance.CreatInstanceId(), fightCharacter.instanceId);
            return buffRuntime;
        }
        return null;      
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
    public void UpData(int timeValue=1)
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
    private RuntimeObj runtimeObj;
    private BuffActionBehavior buffActionBehavior;
    public BuffRuntime(BuffData buffData,int instanceId,int characterId)
    {
        this.instanceId = instanceId;
        this.buffData = buffData; 
        Vector3 pos=FightController.instance.GetPosForCharacterId(characterId);
        if (buffData.buffObj!= null)
        {
            runtimeObj = GameRuntimeObjManager.instance.CreatRuntimeObj(FightRuntimeObjType.OTHER.ToString(), buffData.buffObj.name, buffData.buffObj, instanceId);
            buffActionBehavior = runtimeObj.obj as BuffActionBehavior;
            if (buffActionBehavior)
            {
                buffActionBehavior.stopAction = ParticleSystemStopAction;
                buffActionBehavior.transform.position = pos;
                buffActionBehavior.PlayParticle();
            }
        } 
    } 
    void ParticleSystemStopAction()
    {
        if (runtimeObj != null)
        {
            GameRuntimeObjManager.instance.RecycleRuntimeObj(runtimeObj);
            runtimeObj = null;
            buffActionBehavior = null;
        } 
    }
    public void RemoveBuff()
    {
        if (runtimeObj != null)
        {
            GameRuntimeObjManager.instance.RecycleRuntimeObj(runtimeObj);
            runtimeObj = null;
            buffActionBehavior = null;
        }
    }


    public bool BuffActionEnd()
    {
        nowActionIndex++;
        if (nowActionIndex >= buffData.lifeTime)
        {
            buffActionBehavior.StopParticle();
            return true;
        }
        return false;
    }

    public void BuffPerAction(int characterId)
    {
        void TrueBuffAction()
        {
            FightManager.instance.BuffAction(buffData, characterId,true);
        }
        if (buffData.myTimeLineData != null)
        {
            TimeLineManger.instance.PlaySkillTimeline(characterId, null, buffData.myTimeLineData
             , () =>
             {

                 TrueBuffAction();
                 // fightCharacter.fightStatus = FightStatus.准备;
             }); 
        }
        else
        {
            TrueBuffAction();
        }
       
    }
}