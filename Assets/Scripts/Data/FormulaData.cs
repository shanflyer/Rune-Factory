using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum FormulaType
{
    装备 = 0,
    衣物 = 1,
    药剂 = 2,
    酒水 = 3,
    冷食 = 4,
    热食 = 5
}

public class FormulaData : ScriptableObject, IGameData, IReferenceData
{
    public string formulaName;
    public int id;
    public FormulaType formulaType;
    public List<int> Stuffs;
    public int Product;
    public int PowerCost;
    public int produceTime;

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

    public bool Check(List<int> items)
    {
        if (Stuffs.Count == items.Count)
        {
            List<int> stuffs = Stuffs.GetRange(0, Stuffs.Count);
            for (int i = 0; i < items.Count; i++)
            {
                int index = stuffs.FindIndex(s => s == items[i]);
                if (index < 0)
                {
                    return false;
                }
                stuffs.RemoveAt(index);
            }
            return true;
        }
        return false;
    }
}