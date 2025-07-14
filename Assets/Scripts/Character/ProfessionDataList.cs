using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using System;
using Unity.Collections;
using System.Linq;
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
        ProfessionData professionData=null;
        for (int i = 0; i < professionEditorDatas.Length; i++)
        {
            var data = professionEditorDatas[i];
            if (professionData==null||professionData.id != data.id)
            {
                professionData = new ProfessionData();
                _professionDatas.Add(professionData);
               // if (professionData.id != 0)
                  
                professionData.skills = new List<int>();
                professionData.exp = new List<int>();
                professionData.propertys = new List<GameProperty>();
                professionData.id = data.id;
                professionData.professionName = data.professionName;
                professionData.attributeType = data.attributeType;
                professionData.behaviorId = data.behaviorId;
            }

            if (professionData.id == data.id)
            {
                GameProperty characterProperty = new GameProperty
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
    public List<GameProperty> propertys;
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
    public GameProperty GetLevelProperty(int level)
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
    public void Init()
    {
        GameDataManager.instance.CreateProfessionEcsData();
        if (GameDataManager.instance.eCSProfessionLevelDatas.Length == 0)
        {
            GameDataManager.instance.professionRange.Add(id, exp.Count);
        }
        else
        {
            GameDataManager.instance.professionRange.Add(id, 
                GameDataManager.instance.eCSProfessionLevelDatas.Length+exp.Count);
        }
        for (int i = 0; i < exp.Count; i++)
        {
            GameDataManager.instance.eCSProfessionLevelDatas.Add(new int2(exp[i], skills[i]));
        }
    }
}
 

public partial class GameDataManager
{ 

    public NativeHashMap<int, int> professionRange;
    public NativeList<int2> eCSProfessionLevelDatas;

    public void CreateProfessionEcsData()
    {
        if (professionRange.IsCreated)
        {
            professionRange = new NativeHashMap<int, int>(16, Allocator.Persistent);
            clearAction += ClearProfessionData;
        }
        if (eCSProfessionLevelDatas.IsCreated)
        {
            eCSProfessionLevelDatas = new NativeList<int2>(32, Allocator.Persistent);
        }
    }
    public void ClearProfessionData() 
    {
        if (!professionRange.IsCreated)
        {
            professionRange.Dispose();
        }
        if (!eCSProfessionLevelDatas.IsCreated)
        {
            eCSProfessionLevelDatas.Dispose();
        }
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