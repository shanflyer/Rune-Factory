using System.Collections.Generic;
using System.Net.Http.Headers;
using Unity.Mathematics;
using UnityEngine;

public enum FightStatus
{
    准备, 行动,
}

public enum FightCharacterStaues
{
    正常, 死亡, 濒死
}

public class FightCharacter : IReferenceData
{
    public FightStatus fightStatus = FightStatus.准备;
    public FightCharacterStaues fightCharacterStaues = FightCharacterStaues.正常;
    public virtual AttributeType AttackAttributeType { get; }
    public virtual AttributeType DefenceAttributeType { get; }
    public virtual CharacterProperty characterProperty { get; }
    public virtual Sprite icon { get; set; }
    public int instanceId { get; set; }
    public virtual int behaviorId { get; }
    public int2 fightPos;
    public Dictionary<int, SkillRuntime> skillRuntimes { get; set; }

    public FightCharacter()
    {
        buffMulProperty = CharacterProperty.FullPercent;
    }
    public virtual CharacterProperty buffAddProperty { get; }
    public virtual CharacterProperty buffMulProperty { get; }
    public List<BuffRuntime> buffRuntimes { get; set; } 
    public virtual bool IsEquipSkill(int skillId)
    {
        return false;
    }

    public virtual bool CheckAction()
    { return false; }

    public virtual void CreatSkillRuntime(IGameData gameData = null)
    {
    }

    public SkillRuntime GetSkillRuntime(int id)
    {
        if (skillRuntimes.TryGetValue(id, out var skillRuntime))
        {
            return skillRuntime;
        }
        return null;
    }

    public virtual int attackType { get; }

    private float PerRoundTime = 0;
    private float waiteTime = 0;
    public bool waiteEnd;

    public float WaiteValue()
    {
        float value = 1 - waiteTime / PerRoundTime;
        return math.clamp(value, 0, 1);
    }

    public void InitRundTime()
    {
        float speed = characterProperty.Speed;
        if (speed <= 0)
        {
            PerRoundTime = GameCommon.DefaultPerRoundCd*2;
        }
        else
        {
            PerRoundTime = GameCommon.DefaultPerRoundCd * (100.0f / speed);
        }
        
    }

    public void Reset()
    {
        fightStatus = FightStatus.准备;
        waiteTime = 0;
        waiteEnd = false;
    }
    void AddBuffAction(BuffRuntime buffRuntime)
    {
        switch (buffRuntime.buffData.buffactionType)
        {
            case BuffActionType.属性改变:
                buffAddProperty.AddProperty((CharacterPropertyType)buffRuntime.buffData.addActionValue.x, buffRuntime.buffData.addActionValue.y);
                buffMulProperty.AddProperty((CharacterPropertyType)buffRuntime.buffData.mulActionValue.x, buffRuntime.buffData.mulActionValue.y);
                break;
            case BuffActionType.伤害:
                break;
            case BuffActionType.回复:
                break;
        }
    }
    void RemoveBuffAction(BuffRuntime buffRuntime)
    {
        switch (buffRuntime.buffData.buffactionType)
        {
            case BuffActionType.属性改变:
                buffAddProperty.AddProperty((CharacterPropertyType)buffRuntime.buffData.addActionValue.x, -buffRuntime.buffData.addActionValue.y);
                buffMulProperty.AddProperty((CharacterPropertyType)buffRuntime.buffData.mulActionValue.x, -buffRuntime.buffData.mulActionValue.y);
                break;
            case BuffActionType.伤害:
                break;
            case BuffActionType.回复:
                break;
        }
    }
    public async void CreatBuffRuntime(int buffId)
    {
        var buffRuntime = await SkillManager.instance.CreatBuffRuntime(buffId,this);
        if (buffRuntime == null)
        {
            return;
        }
        for (int i = buffRuntimes.Count - 1; i >= 0; i--)
        {
            var oldBuffRuntime = buffRuntimes[i];
            if (buffRuntime.buffData.coverBuffs.Contains(oldBuffRuntime.buffData.id))
            {
                RemoveBuffAction(oldBuffRuntime);
                buffRuntimes.RemoveAt(i);
            }
        }
        if (buffRuntime.buffData.lifeTime > 0)
        {
            AddBuffAction(buffRuntime);
            buffRuntimes.Add(buffRuntime);
        }
    }
    public void UpData(float timeValue)
    {
        if (fightCharacterStaues == FightCharacterStaues.正常)
        {
            if (waiteTime < PerRoundTime)
            {
                waiteTime += timeValue;

                if (waiteTime >= PerRoundTime)
                {
                    if (characterProperty.Speed <= 0)
                    {
                        waiteEnd = false;
                        waiteTime = 0;
                    }
                    else
                    {
                        waiteEnd = true;
                    }
                   
                    foreach (var skillRuntime in skillRuntimes)
                    {
                        skillRuntime.Value.UpData(1);
                    }

                    if (buffRuntimes != null)
                    {
                        for (int i = buffRuntimes.Count - 1; i >= 0; i--)
                        {
                            buffRuntimes[i].BuffPerAction(instanceId);
                            if (buffRuntimes[i].BuffActionEnd())
                            {
                                RemoveBuffAction(buffRuntimes[i]);
                                buffRuntimes.RemoveAt(i);
                            }
                        }
                    }
                   
                }
            }
        }
    }

    public virtual void SetCharacterValue(SetCharacterProperty setCharacterProperty)
    {
        InitRundTime();
    }

    public virtual Dictionary<FightType, List<int>> GetReadySkills(FightType fightType = FightType.All)
    {
        Dictionary<FightType, List<int>> results = new Dictionary<FightType, List<int>>();
        return results;
    }

    public virtual void Clear()
    {
    }

    public void ClearBuff()
    {
        for(int i = buffRuntimes.Count - 1; i >= 0; i--)
        {
            RemoveBuffAction(buffRuntimes[i]);
            buffRuntimes.RemoveAt(i);
        }
    }
}

public class FightPlayer : FightCharacter
{
    public override CharacterProperty characterProperty
    {
        get
        {
            return character.CharacterProperty*buffMulProperty*0.01f+buffAddProperty;
        }
    }
 

    public Character character;
    public override Sprite icon { get => character.characterData.icon.sprite; set => base.icon = value; }
    public override AttributeType AttackAttributeType => character.AttackAttributeType;
    public override AttributeType DefenceAttributeType => character.DefenceAttributeType;
    private int characterSkill, equipSkill;

    public override bool IsEquipSkill(int skillId)
    {
        return equipSkill == skillId;
    }

    public SkillRuntime GetPlayerEquipSkill()
    {
        return GetSkillRuntime(equipSkill);
    }

    public int dataId => character.dataId;

    public override int attackType
    {
        get
        {
            return character.attackType;
        }
    }

    public override bool CheckAction()
    {
        Character character = CharacterManager.instance.GetCharacter(instanceId);
        if (character.CharacterProperty.HP > 0 && fightCharacterStaues == FightCharacterStaues.正常 && fightStatus == FightStatus.准备)
        {
            return true;
        }

        return false;
    }

    public override Dictionary<FightType, List<int>> GetReadySkills(FightType fightType = FightType.All)
    {
        var character = CharacterManager.instance.GetCharacter(instanceId);
        Dictionary<FightType, List<int>> results = new Dictionary<FightType, List<int>>();
        using (var e = skillRuntimes.GetEnumerator())
        {
            while (e.MoveNext())
            {
                var skillRuntime = e.Current.Value;
                if (fightType != FightType.All && skillRuntime.fightType != fightType)
                {
                    continue;
                }
                if (skillRuntime.instanceId == equipSkill && !FightController.instance.AutoExplore)
                {
                    continue;
                }
                if (skillRuntime.waiteCDEnd && character.CharacterProperty.MP >= skillRuntime.skillData.cost)
                {
                    if (!results.TryGetValue(skillRuntime.fightType, out var skills))
                    {
                        skills = new List<int>();
                        results.Add(skillRuntime.fightType, skills);
                    }
                    skills.Add(skillRuntime.instanceId);
                }
            }
        }
        return results;
    }

    public override async void CreatSkillRuntime(IGameData gameData = null)
    {
        Character character = CharacterManager.instance.GetCharacter(instanceId);
        skillRuntimes = new Dictionary<int, SkillRuntime>();
        for (int i = 0; i < character.skills.Count; i++)
        {
            int skillId = character.skills[i];
            SkillRuntime skillRuntime = await SkillManager.instance.CreatSkillRuntime(skillId);
            skillRuntimes.Add(skillRuntime.instanceId, skillRuntime);
        }
        equipSkill = 0;
        var equip = character.Equip;
        ItemData weappon = await GameDataManager.instance.GetAsyncData<ItemData>(equip.weapon.x);
        if (weappon != null)
        {
            int skillId = weappon.typeValue;

            if (skillId != 0)
            {
                SkillRuntime skillRuntime = await SkillManager.instance.CreatSkillRuntime(skillId);
                skillRuntimes.Add(skillRuntime.instanceId, skillRuntime);

                equipSkill = skillRuntime.instanceId;
            }
        }
        ItemData clothes = await GameDataManager.instance.GetAsyncData<ItemData>(equip.clothes.x);
        if (clothes != null)
        {
            int skillId = clothes.typeValue;
            if (skillId != 0)
            {
                SkillRuntime skillRuntime = await SkillManager.instance.CreatSkillRuntime(skillId);
                skillRuntimes.Add(skillRuntime.instanceId, skillRuntime);
            }
        }
        ItemData shoes = await GameDataManager.instance.GetAsyncData<ItemData>(equip.shoes.x);
        if (shoes != null)
        {
            int skillId = shoes.typeValue;
            if (skillId != 0)
            {
                SkillRuntime skillRuntime = await SkillManager.instance.CreatSkillRuntime(skillId);
                skillRuntimes.Add(skillRuntime.instanceId, skillRuntime);
            }
        }
    }

    public override void SetCharacterValue(SetCharacterProperty setCharacterProperty)
    {
        character.SetProperty(setCharacterProperty);
        base.SetCharacterValue(setCharacterProperty);
    }

    public FightPlayer(Character character) : base()
    {
        this.character = character;
        instanceId = character.instanceId;
        InitRundTime();
    }
    
    public override void Clear()
    {
        base.Clear();
    }
}

public class FightMonster : FightCharacter
{
    public override int attackType => monsterData.attackType;

    public MonsterData monsterData;
    public override int behaviorId => monsterData.behaviorId;
    public override Sprite icon { get => monsterData.monsterSprite.sprite; set => base.icon = value; }

    public FightMonster(MonsterData monsterData, int instanceId, int2 fightPos):base()
    {
        
        this.monsterData = monsterData;
        this.instanceId = instanceId;
        this.fightPos = fightPos;

        InitCharacterProperty();
        CreatSkillRuntime(monsterData);
        InitRundTime();
    }

    public override Dictionary<FightType, List<int>> GetReadySkills(FightType fightType = FightType.All)
    {
        Dictionary<FightType, List<int>> results = new Dictionary<FightType, List<int>>();

        using (var e = skillRuntimes.GetEnumerator())
        {
            while (e.MoveNext())
            {
                var skillRuntime = e.Current.Value;
                if (fightType != FightType.All && skillRuntime.fightType != fightType)
                {
                    continue;
                }
                if (skillRuntime.waiteCDEnd)
                {
                    if (!results.TryGetValue(skillRuntime.fightType, out var skills))
                    {
                        skills = new List<int>();
                        results.Add(skillRuntime.fightType, skills);
                    }
                    skills.Add(skillRuntime.instanceId);
                }
            }
        }

        return results;
    }

    public override bool CheckAction()
    {
        if (characterProperty.HP > 0 && fightCharacterStaues == FightCharacterStaues.正常 && fightStatus == FightStatus.准备)
        {
            return true;
        }

        return false;
    }

    public override async void CreatSkillRuntime(IGameData gameData)
    {
        MonsterData monsterData = gameData as MonsterData;
        if (monsterData != null)
        {
            skillRuntimes = new Dictionary<int, SkillRuntime>();
            for (int i = 0; i < monsterData.skills.Count; i++)
            {
                int skillId = monsterData.skills[i];
                SkillRuntime skillRuntime = await SkillManager.instance.CreatSkillRuntime(skillId);
                skillRuntimes.Add(skillRuntime.instanceId, skillRuntime);
            }
        }
    }

    public override CharacterProperty characterProperty
    {
        get
        {
            return _characterProperty * buffMulProperty*0.01f + buffAddProperty;
        }
    }

    private CharacterProperty _characterProperty;
    public override AttributeType AttackAttributeType => attributeType;
    public override AttributeType DefenceAttributeType => attributeType;
    private AttributeType attributeType;

    private void InitCharacterProperty()
    {
        _characterProperty.HP = monsterData.HP;
        _characterProperty.AT = monsterData.AT;
        _characterProperty.DF = monsterData.DF;
        _characterProperty.Speed = monsterData.Speed;
        _characterProperty.Lucky = monsterData.Lucky;
        attributeType = monsterData.attributeType;
    }

    public override void SetCharacterValue(SetCharacterProperty setCharacterProperty)
    {
        _characterProperty.SetProperty(setCharacterProperty);
        CharacterPropertyTrigger CharacterPropertyTrigger = new CharacterPropertyTrigger
        {
            characterId = instanceId,
            characterProperty = _characterProperty
        };
        GameActionManager.instance.QueueAction(CharacterPropertyTrigger, true);
        base.SetCharacterValue(setCharacterProperty);
        InitRundTime();
    }
}