using UnityEngine;
using BehaviorDesigner.Runtime;
using System.Collections.Generic;
using Unity.Mathematics;

[System.Serializable]
public class SharedRandomResults : SharedVariable<List<int2>>
{
	public override string ToString() { return mValue == null ? "null" : mValue.ToString(); }
	public static implicit operator SharedRandomResults(List<int2> value) { return new SharedRandomResults { mValue = value }; }
}
