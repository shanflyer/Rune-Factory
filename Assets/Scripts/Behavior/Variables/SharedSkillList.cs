using BehaviorDesigner.Runtime;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class SharedSkillList : SharedVariable<Dictionary<FightType, List<int>>>
{
    public override string ToString() { return mValue == null ? "null" : mValue.ToString(); }
    public static implicit operator SharedSkillList(Dictionary<FightType, List<int>> value) { return new SharedSkillList { mValue = value }; }
}
