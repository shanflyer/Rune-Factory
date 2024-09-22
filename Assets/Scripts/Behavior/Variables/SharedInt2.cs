using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using Unity.Mathematics;

[System.Serializable]
public class SharedInt2 : SharedVariable<int2>
{
    public SharedInt2()
    {
        Value = new int2(int.MinValue);
    }
    public bool IsNull()
    {
        return Value.x == int.MinValue;
    }
    public override string ToString() { return mValue.ToString(); }
    public static implicit operator SharedInt2(int2 value) { return new SharedInt2 { mValue = value }; }
}