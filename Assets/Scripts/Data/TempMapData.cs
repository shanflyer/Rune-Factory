using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/虚拟地图")]
public class TempMapData : ScriptableObject, IGameData
{
    public int tempMapId;
    public int mapId;
    public List<int4> grids;

    public string GetKey()
    {
        return tempMapId.ToString();
    }

    public override string ToString()
    {
        return tempMapId.ToString();
    }

    public void SetReferenceData()
    {
    }
}