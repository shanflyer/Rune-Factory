using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
[CreateAssetMenu(menuName ="Data/职业数据")]
public class ProfessionData : ScriptableObject,IGameData
{ 
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
        for (int i = 0; i<skills.Count; i++)
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
        for(int i=0; i < skills.Count; i++)
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
        float levelValue = (float)level/ maxLevel;
        return (int)math.lerp(ZeroExp, FinalExp, expGrowModelData.curve.Evaluate(levelValue));
    }
    public CharacterProperty GetLevelProperty(int level)
    {
        level=math.clamp(level,0,maxLevel);
        float levelValue = (float)level / maxLevel;
        return CharacterProperty.Lerp(ZeroProperty, FinalProperty, propertyGrowModelData.curve.Evaluate(levelValue));
    }
#if UNITY_EDITOR
    public void SetReferenceData()
    {
    }
#endif
    public string GetKey()
    {
       return id.ToString();
    }
}