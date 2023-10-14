using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct TempPosRange
{
    public int mapInstance;
    public int2 posMin;
    public int2 posMax;
}
public class TempCharacterCreatData : ScriptableObject, IGameData, IReferenceData
{
    public int level;
    public string creatName;
    public int2 cd;
    public int maxCharacterCount;
    public List<int> tempCharacters;
    public List<int> tempGroupCharacters;

    public List<TempPosRange> tempEnterDatas;
    public List<TempPosRange> tempExitDatas;
    public string GetKey()
    {
        return level.ToString();
    }
    public override string ToString()
    {
        return level.ToString();
    }
    public void SetReferenceData()
    { 
    } 
}