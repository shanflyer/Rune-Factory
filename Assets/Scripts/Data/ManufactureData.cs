using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ManufactureData : ScriptableObject, IGameData, IReferenceData
{
    public int id;
    public string manufactureName;
    public int openItem;
    public bool hideNull;
    public List<int> needItem;
    public int noItemTalk;
    public List<int2> linkFormulas;
    public int defaultProduct;
    public int2 defaultCostItem;
    public int defaultProduceTime;
    public int characterAnimatorState = -1;
    public float2 cameraOffset;

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
