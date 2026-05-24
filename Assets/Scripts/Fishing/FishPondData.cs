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

    public int2 waitFishingCd;
#if UNITY_EDITOR
    private List<int2> fishRandomIds = new List<int2>();
#endif
    public SeasonRandomDictionary seasonRandomValue;
    public int2 linkMapItem;
    public string GetKey()
    {
        return id.ToString();
    }
    public override string ToString()
    {
        return id.ToString();
    }
#if UNITY_EDITOR
    public void SetReferenceData()
    {
        seasonRandomValue = new SeasonRandomDictionary();
        for (int i = 0; i < fishRandomIds.Count; i++)
        {
            int2 value = fishRandomIds[i];
            seasonRandomValue[(Season)value.x] = value.y;
        }
    }
#endif
}
