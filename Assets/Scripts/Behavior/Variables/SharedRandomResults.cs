using UnityEngine;
using BehaviorDesigner.Runtime;
using System.Collections.Generic;

[System.Serializable]
public class SharedRandomResults : SharedVariable<List<RandomResult>>
{
	public override string ToString() { return mValue == null ? "null" : mValue.ToString(); }
	public static implicit operator SharedRandomResults(List<RandomResult> value) { return new SharedRandomResults { mValue = value }; }
}