using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Data/职业数据")]
public class ProfessionDataList : ScriptableObject, IGameData, IDataArray<ProfessionData>
{
#if UNITY_EDITOR
    [NonSerialized]
    public ProfessionEditorData[] professionEditorDatas;
    public void SetReferenceData()
    {
        var _professionDatas = new List<ProfessionData>();
        ProfessionData professionData=default(ProfessionData);
        for (int i = 0; i < professionEditorDatas.Length; i++)
        {
            var data = professionEditorDatas[i];
            if (professionData.id != data.id)
            {
                if (professionData.id != 0)
                    _professionDatas.Add(professionData);
                professionData.skills = new List<int>();
                professionData.exp = new List<int>();
                professionData.propertys = new List<CharacterProperty>();
                professionData.id = data.id;
                professionData.professionName = data.professionName;
                professionData.attributeType = data.attributeType;
                professionData.behaviorId = data.behaviorId;
            }

            if (professionData.id == data.id)
            {
                CharacterProperty characterProperty = new CharacterProperty
                {
                    HP = data.HP,
                    MaxHP = data.HP,
                    MP = data.MP,
                    MaxMP = data.MP,
                    AT = data.AT,
                    DF = data.DF,
                    Lucky = data.Lucky,
                    Power=data.Power,
                    MaxPower=data.Power,
                    Speed=data.Speed
                };
                professionData.propertys.Add(characterProperty);
                professionData.exp.Add(data.exp);
                professionData.skills.Add(data.skill);
            }
            
        }
        _professionDatas.Add(professionData);

        professionDatas = _professionDatas.ToArray();
    }
#endif
    public ProfessionData[] professionDatas;
    public ProfessionData[] DataList => professionDatas;

    public string GetKey()
    {
        return "ProfessionDataList";
    }
    public override string ToString()
    {
        return "ProfessionDataList";
    }
   
}
[Serializable]
public class ProfessionData:IGameData
{ 
    public string professionName;
    public int id;
    public int behaviorId;
    public List<int> exp;
    public List<int> skills;
    public List<CharacterProperty> propertys;
    public AttributeType attributeType;
    public string GetKey()
    {
        return id.ToString();
    }
    public int GetLevelSkill(int level)
    {
        level = math.clamp(level, 1, skills.Count);
        return skills[level - 1];
    }
    public int GetLevelExp(int level)
    {
        if (level == 0)
        {
            return 0;
        }
        level = math.clamp(level, 1, exp.Count);
        return exp[level - 1];
    }
    public CharacterProperty GetLevelProperty(int level)
    {
        level = math.clamp(level, 1, propertys.Count);
        return propertys[level - 1];
    }
    public void SetReferenceData()
    { 
    }
    public override string ToString()
    {
        return id.ToString();
    }
}

#if UNITY_EDITOR
public struct ProfessionEditorData
{ 
    public int HP, MP, Power, AT, DF, Lucky,Speed;
    public int id;
    public int Level;
    public int behaviorId;
    public string professionName;
    public int exp;
    public int skill;
    public AttributeType attributeType;
}
#endif