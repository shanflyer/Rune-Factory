using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ManufactureData : ScriptableObject, IGameData, IReferenceData
{
    public int id;
    public string manufactureName;
    public int openItem;
    public List<int2> linkFormulas;
    public int defaultProduct;
    public string GetKey()
    {
        return id.ToString();
    }
    public string GetName()
    {
        return manufactureName;
    }
    public void SetReferenceData()
    { 
    }

    public override string ToString()
    {
        return id.ToString();
    }
}