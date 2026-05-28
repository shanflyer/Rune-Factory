public partial class Character
{
    public CharacterEquipAndPropertyData CharacterEquipAndPropertyData
    {
        get
        {
            return new CharacterEquipAndPropertyData
            {
                id = instanceId,
                name = name,
                characterProperty = CharacterProperty,
                equip = equip,
                //attributeType = AttributeType
            };
        }
    }

    public CharacterProperty CharacterProperty
    {
        get => (ProfessionProperty+EquipmentProperty+OtherAddProperty)*OtherMulProperty;

    }

    private void CharacterPropertyTrigger()
    {
        CharacterPropertyTrigger CharacterPropertyTrigger = new CharacterPropertyTrigger
        {
            characterId = instanceId,
            characterProperty = CharacterProperty
        };
        GameActionManager.instance.QueueAction(CharacterPropertyTrigger,true);
    }

    private CharacterProperty ProfessionProperty
    {
        get
        {
            return professionProperty;
        }
        set
        {
            professionProperty = value;
            CharacterPropertyTrigger();
        }
    }
    private CharacterProperty EquipmentProperty
    {
        get
        {
            return equipmentProperty;
        }
        set
        {
            equipmentProperty = value;
            CharacterPropertyTrigger();
        }
    }
    private CharacterProperty OtherAddProperty
    {
        get
        {
            return otherAddProperty;
        }
        set
        {
            otherAddProperty = value;
            CharacterPropertyTrigger();
        }
    }
    private CharacterProperty OtherMulProperty
    {
        get
        {
            return otherMulProperty;
        }
        set
        {
            otherMulProperty = value;
            CharacterPropertyTrigger();
        }
    }

    private CharacterProperty professionProperty;
    private CharacterProperty equipmentProperty;
    private CharacterProperty otherAddProperty;
    private CharacterProperty otherMulProperty = CharacterProperty.One;

    public int groupId = -1;
    public ProfessionData professionData;
    public int dataId;

    public int Level
    {
        get => level;
    }

    private int level;

    public Exp exp;

    public string name { get; protected set; }

    public  void AddExp(int value)
    {
        bool levelUp = false;

        while (exp.AddExp(value))
        {
            exp.nowLevelExp = professionData.GetLevelExp(level) - professionData.GetLevelExp(level - 1);
            value = 0;
            SetLevel(level + 1);
            levelUp = true;
        }
        if (levelUp)
        {
        }
    }

    public void SetNowExp(int nowExp)
    {
        if (level > 0)
        {
            exp.totalExp = professionData.GetLevelExp(level - 1) + nowExp;
            exp.nowExp = nowExp;
        }
        else
        {
            exp.totalExp = nowExp;
            exp.nowExp = nowExp;
        }

    }
    public void SetLevel(int level, bool zero = false)
    {
        if (level != this.level)
        {
            //attributeType = profressionData.attributeType;
            if (professionData!=null)
            {
                if (zero)
                {
                    skills.Clear();
                    for (int i = 1; i <= level; i++)
                    {
                        int skillId = professionData.GetLevelSkill(i);
                        if (skillId >0)
                        {
                            skills.Add(skillId);
                        }
                    }
                }
                else
                {
                    int skillId = professionData.GetLevelSkill(level);
                    if (skillId >0)
                    {
                        skills.Add(skillId);
                    }
                }
                ProfessionProperty = professionData.GetLevelProperty(level);
            }
            exp.nowLevelExp = professionData.GetLevelExp(level) - professionData.GetLevelExp(level - 1);
            this.level = level;
            CharacterLevelUp characterLevelUp = new CharacterLevelUp
            {
                characterId = instanceId,
                level = level
            };
            GameActionManager.instance.QueueAction(characterLevelUp);
        }
    }

    public void SetProperty(int HP = -1, int MP = -1, int Power = -1, int MaxHP = -1, int MaxMP = -1, int MaxPower = -1, int AT = -1, int DF = -1, int Lucky = -1
        ,int Speed=-1, int Other = -1)
    {
        if (HP >= 0)
            professionProperty.HP = HP;
        if (MP >= 0)
            professionProperty.MP = MP;
        if (Power >= 0)
            professionProperty.Power = Power;
        if (MaxHP >= 0)
            professionProperty.MaxHP = MaxHP;
        if (MaxMP >= 0)
            professionProperty.MaxMP = MaxMP;
        if (MaxPower >= 0)
            professionProperty.MaxPower = MaxPower;
        if (AT >= 0)
            professionProperty.AT = AT;
        if (DF >= 0)
            professionProperty.DF = DF;
        if (Other >= 0)
            professionProperty.Other = Other;
        if (Speed >= 0)
            professionProperty.Speed = Speed;

        this.ProfessionProperty = professionProperty;
    }

    public void SetProperty(SetCharacterProperty setCharacterProperty)
    {
        professionProperty.SetProperty(setCharacterProperty);
        CharacterPropertyTrigger();
        RefreshCharacter refreshCharacter = new RefreshCharacter
        {
            id = instanceId
        };
        GameActionManager.instance.QueueAction(refreshCharacter, true);
    }

    public void AddProperty(ChangeCharacterProperty changeCharacterProperty)
    {
        professionProperty.ChangeProperty(changeCharacterProperty);
        CharacterPropertyTrigger();
        RefreshCharacter refreshCharacter = new RefreshCharacter
        {
            id = instanceId
        };
        GameActionManager.instance.QueueAction(refreshCharacter, true);
    }
}