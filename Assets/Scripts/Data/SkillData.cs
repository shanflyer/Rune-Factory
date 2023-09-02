using System.Collections;
using UnityEngine;
public enum SkillActionType
{
    伤害,待机
}
[CreateAssetMenu(menuName ="Data/技能数据")]
public class SkillData : ScriptableObject, IGameData
{
    public int id;
    public string skillName;
    public int cd;
    public int actionCount;
    public TargetType targetType; 
    public int targetCount;

    public FightType fightType;
    public SkillActionType skillActionType;

    public int cost;

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