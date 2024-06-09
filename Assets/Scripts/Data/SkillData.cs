using System;
using System.Collections; 
using UnityEngine;
public enum SkillActionType
{
    属性值, 固定值
}
public enum TargetRangeType
{
    单体,横向,纵向,全部
}
[CreateAssetMenu(menuName ="Data/技能数据")]
public class SkillData : ScriptableObject, IGameData
{
    public int id;
    public string skillName;
    public int cd;
    public TargetType targetType;
    public TargetRangeType targetRangeType;

    public FightType fightType;
    public SkillActionType skillActionType;  
    public int actionValue;
    public int cost;
    // public int continueSkill;
    [NonSerialized]
    [HideInInspector]
    public string myTimeLineDataName;
    public MyTimeLineData myTimeLineData;
    public override string ToString()
    {
        return id.ToString();
    }
    public string GetKey()
    {
        return id.ToString();
    }

    public void SetReferenceData()
    {
        myTimeLineData = Resources.Load<MyTimeLineData>($"{DataPath.GetDataPath(typeof(MyTimeLineData))}/{myTimeLineDataName}");
    }
}