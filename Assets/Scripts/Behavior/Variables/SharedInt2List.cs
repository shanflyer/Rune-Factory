using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using Unity.Mathematics;

[System.Serializable]
public class SharedInt2List : SharedVariable<int2[]>
{
    public override string ToString() { return mValue == null ? "null" : mValue.ToString(); }
    public static implicit operator SharedInt2List(int2[] value) { return new SharedInt2List { mValue = value }; }
}