using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManufactureData : ScriptableObject, IGameData, IReferenceData
{
    public int id;
    public string manufactureName;
    public int openItem;
    public bool open;
    public List<FormulaType> formulaTypes=new List<FormulaType>();
    public int defaultProduct;
    public string GetKey()
    {
        return id.ToString();
    }

    public void SetReferenceData()
    { 
    }

    public override string ToString()
    {
        return id.ToString();
    }
}