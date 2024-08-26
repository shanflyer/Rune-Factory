using System.Collections.Generic;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using System;

public struct MyString : IReferenceData
{
    
}
[Serializable]
public struct MyListInt : IReferenceData
{
    public List<int> intList;
}
public struct MyInt : IReferenceData
{
    public int value;
}
public struct MyInt3 : IReferenceData
{
    public int3 value;
}