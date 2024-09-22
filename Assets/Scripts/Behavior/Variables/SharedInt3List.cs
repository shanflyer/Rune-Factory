using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using Unity.Mathematics;

[System.Serializable]
public class SharedInt3List : SharedVariable<List<int3>>
{
	public override string ToString() { return mValue == null ? "null" : mValue.ToString(); }
	public static implicit operator SharedInt3List(List<int3> value) { return new SharedInt3List { mValue = value }; }
}