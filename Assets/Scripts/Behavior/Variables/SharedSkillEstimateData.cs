using BehaviorDesigner.Runtime;

[System.Serializable]
public class SharedSkillEstimateData : SharedVariable<SkillEstimateData>
{
    public override string ToString() { return mValue.skillId==0 ? "null" : mValue.ToString(); }
    public static implicit operator SharedSkillEstimateData(SkillEstimateData value) { return new SharedSkillEstimateData { mValue = value }; }

}