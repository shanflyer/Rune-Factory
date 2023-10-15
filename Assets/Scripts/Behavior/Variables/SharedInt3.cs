using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using Unity.Mathematics;

[System.Serializable]
public class SharedInt3 : SharedVariable<int3>
{
    public SharedInt3()
    {
        Value = new int3(int.MinValue);
    }
    public bool IsNull()
    {
        return Value.x== int.MinValue;
    }
    public override string ToString() { return mValue.ToString(); }
    public static implicit operator SharedInt3(int3 value) { return new SharedInt3 { mValue = value }; }
}