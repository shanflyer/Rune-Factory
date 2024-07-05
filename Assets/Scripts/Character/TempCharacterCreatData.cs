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
    public int id;
    public string creatName;
    public int2 cd;
    public int maxCharacterCount;
    public List<int> tempCharacters;
    public List<int> tempGroupCharacters;

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