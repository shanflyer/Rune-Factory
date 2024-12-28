using System;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(menuName ="Data/指引记录数据")]
public class GameGuideFilmData : ScriptableObject, IGameData
{
    public int id;
    public string guidName; 
    public int beforeEventId;
    public int3 fixedMap;
    public int fixedDate;
    public int fixedHour;
    public bool displayCharacter;
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