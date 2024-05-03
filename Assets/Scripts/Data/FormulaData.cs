using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum FormulaType
{
    铸造 = 0,纺织 = 1,书写=2,木工=3, 粉磨 = 10, 徒手 = 11,蒸煮 = 12,烘烤 = 13,酿造 = 14,搅拌=15
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
        items.RemoveAll(item => item == 0);
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