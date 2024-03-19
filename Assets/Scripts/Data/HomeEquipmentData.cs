using System.Collections.Generic;
using UnityEngine;

public enum HomeEquipType
{
    生产设施,生活设施
}
public class HomeEquipmentData : ScriptableObject, IGameData, IReferenceData
{
    public int id;
    public string equipmentName;
    public int mapItemDataId;
    public HomeEquipType homeEquipType;
    public List<int> canSetMaps = new List<int>();

    public string GetName()
    {
        return equipmentName;
    }
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