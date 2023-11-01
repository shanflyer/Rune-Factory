using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class FishPondData : ScriptableObject, IGameData
{
    public int id;
    public string pondName;
    public int showValue;
   
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