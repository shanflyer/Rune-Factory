using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.Mathematics;
using BehaviorDesigner.Runtime;
[Serializable]
public class SeasonRandomDictionary : SerializableDictionary<Season, int> { }
[Serializable]
public class ItemAnimationDictionary : SerializableDictionary<int2, AnimationStateData> { }
[Serializable]
public class IntBehaviorDictionary: SerializableDictionary<int,ExternalBehaviorTree> { }

[Serializable]
public class StringStringDictionary : SerializableDictionary<string, string> {}

[Serializable]
public class ObjectColorDictionary : SerializableDictionary<UnityEngine.Object, Color> {}

[Serializable]
public class ColorArrayStorage : SerializableDictionary.Storage<Color[]> {}

[Serializable]
public class StringColorArrayDictionary : SerializableDictionary<string, Color[], ColorArrayStorage> {}

[Serializable]
public class MyClass
{
    public int i;
    public string str;
}

[Serializable]
public class QuaternionMyClassDictionary : SerializableDictionary<Quaternion, MyClass> {}