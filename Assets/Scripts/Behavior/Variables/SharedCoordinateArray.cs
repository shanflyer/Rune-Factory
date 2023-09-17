using UnityEngine;
using BehaviorDesigner.Runtime;
using Unity.Mathematics;

[System.Serializable]
public class SharedCoordinateArray : SharedVariable<int2[]>
{
	public override string ToString() { return mValue == null ? "null" : mValue.ToString(); }
	public static implicit operator SharedCoordinateArray(int2[] value) { return new SharedCoordinateArray { mValue = value }; }
}