using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class DefaultConfigData : ScriptableObject, IGameData
{
    public new string name;
    public string value;
    public string GetKey()
    {
        return name;
    }
    public override string ToString()
    {
        return name;
    }
    public void SetReferenceData()
    { 
    }
}
