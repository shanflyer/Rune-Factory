using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Data/职业数据")]
public class ProfessionData : ScriptableObject, IGameData
{
#if UNITY_EDITOR
    [NonSerialized]
    public int Zero_HP, Zero_MP, Zero_Power,  Zero_AT, Zero_DF, Zero_Lucky;
    [NonSerialized]
    public int Zero_Other;
    [NonSerialized]
    public int Final_HP, Final_MP, Final_Power, Final_AT, Final_DF, Final_Lucky;
    [NonSerialized]
    public int Final_Other;
#endif

    public int id;
    public string professionName;
    public int maxLevel;
    public int ZeroExp, FinalExp;
    public AttributeType attributeType;
    public CharacterProperty ZeroProperty;
    public CharacterProperty FinalProperty;
    public int propertyGrowModel;
    public int expGrowModel;
    public int behaviorId;
    public List<int2> skills = new List<int2>();

    private GrowModelData propertyGrowModelData, expGrowModelData;

    public async void Init()
    {
        expGrowModelData = await GameDataManager.instance.GetAsyncData<GrowModelData>(expGrowModel.ToString());
        propertyGrowModelData = await GameDataManager.instance.GetAsyncData<GrowModelData>(propertyGrowModel.ToString());
    }

    public int GetLevelSkill(int level)
    {
        for (int i = 0; i < skills.Count; i++)
        {
            int2 skill = skills[i];
            if (skill.x == level)
            {
                return skill.y;
            }
        }
        return -1;
    }

    public List<int> GetLevelSkills(int level)
    {
        List<int> result = new List<int>();
        for (int i = 0; i < skills.Count; i++)
        {
            if (skills[i].x <= level)
            {
                result.Add(skills[i].y);
            }
            else
            {
                break;
            }
        }
        return result;
    }

    public int GetLevelExp(int level)
    {
        level = math.clamp(level, 0, maxLevel);
        float levelValue = (float)level / maxLevel;
        return (int)math.lerp(ZeroExp, FinalExp, expGrowModelData.curve.Evaluate(levelValue));
    }

    public CharacterProperty GetLevelProperty(int level)
    {
        level = math.clamp(level, 0, maxLevel);
        float levelValue = (float)level / maxLevel;
        return CharacterProperty.Lerp(ZeroProperty, FinalProperty, propertyGrowModelData.curve.Evaluate(levelValue));
    }

#if UNITY_EDITOR

    public void SetReferenceData()
    {
        ZeroProperty = new CharacterProperty();
        ZeroProperty.HP = Zero_HP;
        ZeroProperty.MP = Zero_MP;
        ZeroProperty.Power = Zero_Power;
        ZeroProperty.Other = Zero_Other;
        ZeroProperty.MaxHP = Zero_HP;
        ZeroProperty.MaxMP = Zero_MP;
        ZeroProperty.MaxPower = Zero_Power;
        ZeroProperty.AT = Zero_AT;
        ZeroProperty.DF = Zero_DF;
        ZeroProperty.Lucky = Zero_Lucky;

        FinalProperty = new CharacterProperty();
        FinalProperty.HP = Final_HP;
        FinalProperty.MP = Final_MP;
        FinalProperty.Power = Final_Power;
        FinalProperty.Other = Final_Other;
        FinalProperty.MaxHP = Final_HP;
        FinalProperty.MaxMP = Final_MP;
        FinalProperty.MaxPower = Final_Power;
        FinalProperty.AT = Final_AT;
        FinalProperty.DF = Final_DF;
        FinalProperty.Lucky = Final_Lucky;
    }

#endif

    public string GetKey()
    {
        return id.ToString();
    }
    public override string ToString()
    {
        return id.ToString();
    }
}