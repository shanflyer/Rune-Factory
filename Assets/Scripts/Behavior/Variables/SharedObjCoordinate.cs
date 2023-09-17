using UnityEngine;
using BehaviorDesigner.Runtime;
using UnityEditor;
 

[System.Serializable]
public class SharedObjCoordinate : SharedVariable<ObjCoordinate>
{
	public override string ToString() 
	{ 
		return mValue.ToString();
	}
	public static implicit operator SharedObjCoordinate(ObjCoordinate value) { return new SharedObjCoordinate { mValue = value }; }
}



