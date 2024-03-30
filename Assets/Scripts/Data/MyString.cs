using System.Collections.Generic;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public struct MyString : IReferenceData
{
    
}
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