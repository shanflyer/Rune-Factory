using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class SkillManager : Singleton<SkillManager>
{ 
    SkillRuntime useItemRuntime;
    public override void Init()
    {
        base.Init();
    }
    protected override void Clear()
    {
        base.Clear();
    }
   
    public async Task<SkillRuntime> CreateSkillRuntime(int skillId)
    {
        SkillData skillData=await GameDataManager.instance.GetAsyncData<SkillData>(skillId);
        SkillRuntime skillRuntime = new SkillRuntime(skillData, ExploreManager.instance.NewUid); 
        return skillRuntime;
    }

    public async Task<BuffRuntime> CreateBuffRuntime(int buffId,FightCharacter fightCharacter, int2 overrideAddValue, int2 overrideMulValue, int overrideLifeTime = -1)
    {
        BuffData buffData = await GameDataManager.instance.GetAsyncData<BuffData>(buffId);
        int randomValue = GameRandom.RandomInt(0, 100);
        randomValue = FightManager.instance.GetAttributeTypeRandomValue(buffData.attributeType, fightCharacter.AttackAttributeType, randomValue);
        if (randomValue < buffData.probability)
        {
            BuffRuntime buffRuntime = new BuffRuntime(buffData, ExploreManager.instance.NewUid, fightCharacter.instanceId, overrideAddValue, overrideMulValue, overrideLifeTime);
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

    public void ReBlindData(SkillData skillData)
    {
        this.skillData = skillData;
    }
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
    public void Update(int timeValue=1)
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
    private BuffData buffData;
    private int nowActionIndex;
    private RuntimeObj runtimeObj;
    private BuffActionBehavior buffActionBehavior;

    public int id => buffData.id;
    public BuffActionType BuffActionType => buffData.buffactionType;
    public AttributeType AttributeType => buffData.attributeType;
    public int lifeTime { get; private set; }
    public int2 addActionValue { get; private set; }
    public int2 mulActionValue { get; private set; }
    public bool CheckCoverBuff(int buffId)
    {
        return buffData.coverBuffs.Contains(buffId);
    }
    public BuffRuntime(BuffData buffData, int instanceId, int characterId, int overrideLifeTime = -1)
    {
        this.instanceId = instanceId;
        this.buffData = buffData;
        if (overrideLifeTime > -1)
        {
            lifeTime = overrideLifeTime;
        }
        else
        {
            lifeTime = buffData.lifeTime;
        }
        mulActionValue = buffData.mulActionValue;
        addActionValue = buffData.addActionValue;

        Vector3 pos = FightController.instance.GetPosForCharacterId(characterId);
        if (buffData.buffObj != null)
        {
            InitRuntimeObj();
            void InitRuntimeObj()
            {
                runtimeObj = GameRuntimeObjManager.instance.CreateRuntimeObj(FightRuntimeObjType.OTHER.ToString(), buffData.buffObj.name, buffData.buffObj, instanceId,
                  setComponent:(RuntimeObj runtime) =>
                    {
                        buffActionBehavior = runtime.obj as BuffActionBehavior;
                        if (buffActionBehavior)
                        {
                            buffActionBehavior.stopAction = ParticleSystemStopAction;
                            buffActionBehavior.transform.position = pos;
                            buffActionBehavior.PlayParticle();
                        }
                    });
               
            }

        }
    }
    public BuffRuntime(BuffData buffData, int instanceId, int characterId,  int2 overrideAddValue,int2 overrideMulValue, int overrideLifeTime = -1)
    {
        this.instanceId = instanceId;
        this.buffData = buffData;
        if (overrideLifeTime > -1)
        {
            lifeTime = overrideLifeTime;
        }
        else
        {
            lifeTime = buffData.lifeTime;
        }
        if (overrideMulValue.Equals(int2.zero))
        {
            mulActionValue = overrideMulValue;
        }
        else
        {
            mulActionValue = buffData.mulActionValue;
        }
        if (overrideAddValue.Equals(int2.zero))
        {
            addActionValue = overrideAddValue;
        }
        else
        {
            addActionValue = buffData.addActionValue;
        }

        Vector3 pos=FightController.instance.GetPosForCharacterId(characterId);
        if (buffData.buffObj!= null)
        {
            InitRuntimeObj();
            void InitRuntimeObj()
            {
                runtimeObj =GameRuntimeObjManager.instance.CreateRuntimeObj(FightRuntimeObjType.OTHER.ToString(), buffData.buffObj.name, buffData.buffObj, instanceId,
                      setComponent: (RuntimeObj runtime) =>
                      {
                        
                          buffActionBehavior = runtime.obj as BuffActionBehavior;
                          if (buffActionBehavior)
                          {
                              buffActionBehavior.stopAction = ParticleSystemStopAction;
                              buffActionBehavior.transform.position = pos;
                              buffActionBehavior.PlayParticle();
                          }
                      }); 
            }
            
        } 
    } 
   
    public void Hide(bool hide)
    {
        if (buffActionBehavior)
        {
            Vector3 localPos = buffActionBehavior.transform.localPosition;
            localPos.z = hide ? -999999 : 0;
            buffActionBehavior.transform.localPosition = localPos;
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
        if (nowActionIndex >= lifeTime)
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