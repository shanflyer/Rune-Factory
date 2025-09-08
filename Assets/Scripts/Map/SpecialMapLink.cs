using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/SpecialMapLink")]
public class SpecialMapLink : ScriptableObject, IGameData
{
    public int specialId;
    public SpecialMap map0;
    public SpecialMap map1;

    public string GetKey()
    {
        return name;
    }

    public void SetReferenceData()
    {
    }
}

[Serializable]
public struct SpecialMap
{
    public int specialMap;
    public List<int> specialAreas;
}