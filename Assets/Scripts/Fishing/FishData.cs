using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class FishData:ScriptableObject,IGameData,IReferenceData
{
    public int id;
    public string fishName;
    public int itemId;
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