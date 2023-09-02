using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
public class ShardQueneInt: SharedVariable<Queue<int>>
{
    public override string ToString() { return mValue == null ? "null" : mValue.ToString(); }
    public static implicit operator ShardQueneInt(Queue<int> value) { return new ShardQueneInt { mValue = value }; }
}