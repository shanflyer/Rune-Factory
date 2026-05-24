using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using Unity.Mathematics;

[System.Serializable]
public class SharedSharedInt3List : SharedVariable<List<Int3Shared>>
{
    public override string ToString() { return mValue == null ? "null" : mValue.ToString(); }
    public static implicit operator SharedSharedInt3List(List<Int3Shared> value) { return new SharedSharedInt3List { mValue = value }; }
}
[System.Serializable]
public struct Int3Shared
{
    public SharedInt x, y, z;

    public int3 Value => new int3(x.Value, y.Value, z.Value);
}
