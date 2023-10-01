using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum FormulaType
{
    武器 = 0,
    防具=1,
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
    public bool isOpen;
    public string GetKey()
    {
        return id.ToString();
    }

    public void SetReferenceData()
    { 
    }
}