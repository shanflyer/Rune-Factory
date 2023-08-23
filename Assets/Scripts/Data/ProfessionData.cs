using System.Collections;
using Unity.Mathematics;
using UnityEngine;
public class ProfessionData : ScriptableObject,IGameData
{ 
    public int id;
    public int maxLevel;
    public int ZeroExp, FinalExp;
    public CharacterProperty ZeroProperty; 
    public CharacterProperty FinalProperty;
    public int propertyGrowModel;
    public int expGrowModel;
    public int behaviorId;

    private GrowModelData propertyGrowModelData, expGrowModelData;

    public async void Init()
    {
        expGrowModelData = await GameDataManager.instance.GetAsyncData<GrowModelData>(expGrowModel.ToString());
        propertyGrowModelData = await GameDataManager.instance.GetAsyncData<GrowModelData>(propertyGrowModel.ToString());
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