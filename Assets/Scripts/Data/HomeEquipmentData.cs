using System.Collections.Generic;
using UnityEngine;

public class HomeEquipmentData : ScriptableObject, IGameData, IReferenceData
{
    public int id;
    public string equipmentName;
    public int mapItemDataId;
    public List<int> canSetMaps = new List<int>();

    public string GetKey()
    {
        return id.ToString();
    }

    public override string ToString()
    {
        return id.ToString();
    }

    public void SetReferenceData()
    {
    }
}