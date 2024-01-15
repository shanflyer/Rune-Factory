using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using Unity.Mathematics;

[System.Serializable]
public class SharedIntList : SharedVariable<List<int>>
{
    public override string ToString() { return mValue == null ? "null" : mValue.ToString(); }
    public static implicit operator SharedIntList(List<int> value) { return new SharedIntList { mValue = value }; }
}