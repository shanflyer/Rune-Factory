using System.Collections;
using UnityEngine;
public enum SkillActionType
{
    伤害,
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
    string IGameData.GetKey()
    {
        return id.ToString();
    }

    void IGameData.SetReferenceData()
    {
        myTimeLineData = Resources.Load<MyTimeLineData>($"{DataPath.GetDataPath(typeof(MyTimeLineData))}/{myTimeLineData}");
    }
}