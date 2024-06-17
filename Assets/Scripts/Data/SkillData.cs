using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum SkillActionType
{
    属性值, 固定值
}
public enum TargetRangeType
{
    单体,横向,纵向,全部,Null
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

    public bool haveNextAction;
    public bool holdTarget;
    public TargetType nextTargetType;
    public FightType nextFightType;
    public SkillActionType nextSkillActionType;
    public int nextActionValue;
    private string nextTimeLineDataName;
    public MyTimeLineData nextTimeLineData;

    public int cost;
    public Sprite icon; 
    private string iconName; 
    private string myTimeLineDataName;
    public MyTimeLineData myTimeLineData;
    public override string ToString()
    {
        return id.ToString();
    }
    public string GetKey()
    {
        return id.ToString();
    }
#if UNITY_EDITOR
    static Dictionary<string, Sprite> allSprites = new Dictionary<string, Sprite>(); 
    public static void Clear()
    {
        allSprites.Clear(); 
    }
 
    public void SetReferenceData()
    {
        myTimeLineData = Resources.Load<MyTimeLineData>($"{DataPath.GetDataPath(typeof(MyTimeLineData))}/{myTimeLineDataName}");
        nextTimeLineData = Resources.Load<MyTimeLineData>($"{DataPath.GetDataPath(typeof(MyTimeLineData))}/{nextTimeLineDataName}");
        if (allSprites.Count == 0)
        {
            var sprites = Resources.LoadAll<Sprite>($"Icon/{iconName}");
            for (int i = 0; i < sprites.Length; i++)
            {
                allSprites.Add(sprites[i].name, sprites[i]);
            }
        }

        if (!allSprites.TryGetValue(iconName, out icon))
        {
             
        }

        
    }
#endif
  
}