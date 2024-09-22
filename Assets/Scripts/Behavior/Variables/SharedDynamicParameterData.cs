using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using Unity.Mathematics;

[System.Serializable]
public class SharedDynamicParameterData : SharedVariable<DynamicParameterData>
{
    public SharedDynamicParameterData()
    { 
    } 
    public override string ToString() { return mValue.ToString(); }
    public static implicit operator SharedDynamicParameterData(DynamicParameterData value) { return new SharedDynamicParameterData { mValue = value }; }
}