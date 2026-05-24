using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
public class SharedQueneInt: SharedVariable<Queue<int>>
{
    public override string ToString() { return mValue == null ? "null" : mValue.ToString(); }
    public static implicit operator SharedQueneInt(Queue<int> value) { return new SharedQueneInt { mValue = value }; }
}
