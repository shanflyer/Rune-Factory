using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

public class FishPondData : ScriptableObject, IGameData
{
    public int id;
    public string pondName;
    public int showValue; 

#if UNITY_EDITOR
    public List<int2> fishRandomIds = new List<int2>();
#endif
    public int maxFishCount;
    public int produceCD;
    public SeasonRandomDictionary seasonRandomValue;
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